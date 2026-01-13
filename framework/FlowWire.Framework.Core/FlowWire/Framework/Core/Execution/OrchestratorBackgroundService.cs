using System.Threading.Channels;
using FlowWire.Framework.Abstractions.Configuration;
using FlowWire.Framework.Abstractions.Model;
using FlowWire.Framework.Core.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FlowWire.Framework.Core.Execution;

public sealed class OrchestratorBackgroundService : BackgroundService
{
    private readonly IImpulseQueue _queue;
    private readonly IFlowExecutor _executor;
    private readonly OrchestratorOptions _options;
    private readonly ILogger<OrchestratorBackgroundService> _logger;

    private readonly Channel<Impulse>[] _shards;
    private readonly Task[] _shardWorkers;
    private readonly Channel<Impulse> _ackChannel;

    public OrchestratorBackgroundService(
        IImpulseQueue queue,
        IFlowExecutor executor,
        IOptions<FlowWireOptions> options,
        ILogger<OrchestratorBackgroundService> logger)
    {
        _queue = queue;
        _executor = executor;
        _options = options.Value.Orchestrator;
        _logger = logger;

        var shardCount = _options.Concurrency;
        _shards = new Channel<Impulse>[shardCount];
        _shardWorkers = new Task[shardCount];

        for (var i = 0; i < shardCount; i++)
        {
            _shards[i] = Channel.CreateBounded<Impulse>(new BoundedChannelOptions(capacity: 1000)
            {
                SingleWriter = true,
                SingleReader = true,
                FullMode = BoundedChannelFullMode.Wait
            });
        }

        _ackChannel = Channel.CreateUnbounded<Impulse>(new UnboundedChannelOptions
        {
            SingleWriter = false,
            SingleReader = true
        });
    }

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogOrchestratorStarted(_options.Concurrency);

        StartShardWorkers(stoppingToken);
        
        var ackerTask = Task.Run(() => RunAckerLoopAsync(stoppingToken), stoppingToken);
        
        await RunIngestionPumpAsync(stoppingToken);
        await Task.WhenAll(_shardWorkers);
        await ackerTask;
    }

    private void StartShardWorkers(CancellationToken stoppingToken)
    {
        for (var i = 0; i < _shards.Length; i++)
        {
            var shardId = i;
            _shardWorkers[i] = Task.Run(
                () => RunShardLoopAsync(shardId, _shards[shardId].Reader, stoppingToken),
                stoppingToken);
        }
    }

    private async Task RunIngestionPumpAsync(CancellationToken ct)
    {
        var batchSize = _options.MaxInboxBatchSize;

        while (!ct.IsCancellationRequested)
        {
            try
            {
                var batch = await _queue.DequeueBatchAsync("default", batchSize, ct);

                if (batch.Count == 0)
                {
                    await Task.Delay(_options.PollInterval, ct);
                    continue;
                }

                await RouteBatchToShardsAsync(batch, ct);
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ingestion Pump Fault");
                // CRITICAL: TODO? - Pause here?
            }
        }

        // Shutdown Signal
        foreach (var shard in _shards)
        {
            shard.Writer.TryComplete();
        }

        _ackChannel.Writer.TryComplete();
    }

    private ValueTask RouteBatchToShardsAsync(IReadOnlyList<Impulse> batch, CancellationToken ct)
    {
        for (var i = 0; i < batch.Count; i++)
        {
            var impulse = batch[i];
            var shardIndex = (uint)impulse.FlowId.GetHashCode() % (uint)_shards.Length;
            var writer = _shards[shardIndex].Writer;

            if (!writer.TryWrite(impulse))
            {
                // Fallback to async path if a channel is full
                return RouteBatchToShardsSlowAsync(batch, i, ct);
            }
        }

        return ValueTask.CompletedTask;
    }

    private async ValueTask RouteBatchToShardsSlowAsync(IReadOnlyList<Impulse> batch, int startIndex, CancellationToken ct)
    {
        for (var i = startIndex; i < batch.Count; i++)
        {
            var impulse = batch[i];
            var shardIndex = (uint)impulse.FlowId.GetHashCode() % (uint)_shards.Length;
            await _shards[shardIndex].Writer.WriteAsync(impulse, ct);
        }
    }

    private async Task RunShardLoopAsync(int shardId, ChannelReader<Impulse> reader, CancellationToken ct)
    {
        try
        {
            while (await reader.WaitToReadAsync(ct))
            {
                while (reader.TryRead(out var impulse))
                {
                    await ProcessImpulseAsync(impulse);
                }
            }
        }
        catch (OperationCanceledException) { }
    }

    private ValueTask ProcessImpulseAsync(Impulse impulse)
    {
        try
        {
            var task = _executor.ExecuteTickAsync(impulse);
            if (task.IsCompletedSuccessfully)
            {
                _ackChannel.Writer.TryWrite(impulse);
                return ValueTask.CompletedTask;
            }

            return ProcessImpulseSlowAsync(impulse, task);
        }
        catch (Exception ex)
        {
            return new ValueTask(HandleProcessErrorAsync(impulse, ex));
        }
    }

    private async Task HandleProcessErrorAsync(Impulse impulse, Exception ex)
    {
        await _queue.NackAsync("default", impulse, ex.Message, retryable: true);
    }

    private async ValueTask ProcessImpulseSlowAsync(Impulse impulse, ValueTask task)
    {
        try
        {
            await task;
            // Write to Ack Channel (Unbounded, so TryWrite always succeeds unless closed)
            _ackChannel.Writer.TryWrite(impulse);
        }
        catch (Exception ex)
        {
            await HandleProcessErrorAsync(impulse, ex);
        }
    }

    private async Task RunAckerLoopAsync(CancellationToken ct)
    {
        const int MaxAckBatch = 100;
        List<Impulse> buffer = new(MaxAckBatch);

        try
        {
            while (await _ackChannel.Reader.WaitToReadAsync(ct))
            {
                while (buffer.Count < MaxAckBatch && _ackChannel.Reader.TryRead(out var item))
                {
                    buffer.Add(item);
                }

                if (buffer.Count > 0)
                {
                    await FlushAckBufferAsync(buffer);
                }
            }
        }
        catch (OperationCanceledException) { }
    }

    /// <summary>
    /// Flushes the acknowledgment buffer to the queue with a safety fallback.
    /// </summary>
    private async ValueTask FlushAckBufferAsync(List<Impulse> buffer)
    {
        try
        {
            await _queue.AckBatchAsync("default", buffer);
        }
        catch (Exception)
        {
            // FALLBACK: Ack one-by-one
            foreach (var item in buffer)
            {
                try
                {
                    await _queue.AckAsync("default", item);
                }
                catch
                {
                    // Ignore individual failures to keep the pipe moving
                }
            }
        }
        finally
        {
            buffer.Clear();
        }
    }
}
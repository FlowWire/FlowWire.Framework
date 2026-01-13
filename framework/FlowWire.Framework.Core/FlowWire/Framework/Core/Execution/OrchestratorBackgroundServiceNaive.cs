using FlowWire.Framework.Abstractions.Configuration;
using FlowWire.Framework.Core.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FlowWire.Framework.Core.Execution;

public sealed class OrchestratorBackgroundServiceNaive(
    IImpulseQueue queue,
    IFlowExecutor executor,
    IOptions<FlowWireOptions> options,
    ILogger<OrchestratorBackgroundServiceNaive> logger) : BackgroundService
{
    private readonly IImpulseQueue _queue = queue;
    private readonly IFlowExecutor _executor = executor;
    private readonly OrchestratorOptions _options = options.Value.Orchestrator;
    private readonly ILogger<OrchestratorBackgroundServiceNaive> _logger = logger;

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogOrchestratorStarted(_options.Concurrency);

        var tasks = new Task[_options.Concurrency];

        for (var i = 0; i < _options.Concurrency; i++)
        {
            var context = new ConsumerContext(i, _queue, _executor, _options, _logger, stoppingToken);

            tasks[i] = Task.Factory.StartNew(
                static state => RunConsumerLoopAsync((ConsumerContext)state!),
                context,
                stoppingToken,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default
            ).Unwrap();
        }

        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Context object to pass all dependencies to the static loop, 
    /// avoiding implicit closure allocations.
    /// </summary>
    private record class ConsumerContext(
        int WorkerId,
        IImpulseQueue Queue,
        IFlowExecutor Executor,
        OrchestratorOptions Options,
        ILogger Logger,
        CancellationToken Token);

    private async static Task RunConsumerLoopAsync(ConsumerContext ctx)
    {
        var backoff = new PollingBackoff();

        while (!ctx.Token.IsCancellationRequested)
        {
            try
            {
                var impulse = await ctx.Queue.DequeueAsync("default", ctx.Token);

                if (impulse is null)
                {
                    await backoff.WaitAsync(ctx.Options.PollInterval, ctx.Token);
                    continue;
                }

                backoff.Reset();

                try
                {
                    await ctx.Executor.ExecuteTickAsync(impulse);
                    await ctx.Queue.AckAsync("default", impulse);
                }
                catch (Exception ex)
                {
                    ctx.Logger.LogImpulseProcessingFailed(impulse.Id, ex.Message);
                    await ctx.Queue.NackAsync("default", impulse, "Processing Failed", retryable: true);
                }
            }
            catch (OperationCanceledException)
            {
                // Graceful shutdown - do not log as error
                break;
            }
            catch (Exception ex)
            {
                ctx.Logger.LogFatalOrchestratorError(ctx.WorkerId, ex);
                await Task.Delay(1000, ctx.Token);
            }
        }
    }
}

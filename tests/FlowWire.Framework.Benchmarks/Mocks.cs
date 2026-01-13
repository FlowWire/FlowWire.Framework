using System.Collections.Concurrent;
using FlowWire.Framework.Abstractions.Model;
using FlowWire.Framework.Core.Execution;

namespace FlowWire.Framework.Benchmarks;

public class MockImpulseQueue : IImpulseQueue
{
    private readonly ConcurrentQueue<Impulse> _queue = new();
    private TaskCompletionSource<bool> _tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private int _remainingAcks;

    public void Setup(IEnumerable<Impulse> impulses, int count)
    {
        _queue.Clear();
        foreach (var impulse in impulses)
        {
            _queue.Enqueue(impulse);
        }
        _remainingAcks = count;
        _tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    public Task AllProcessed => _tcs.Task;

    public ValueTask<Impulse?> DequeueAsync(string group, CancellationToken ct)
    {
        if (_queue.TryDequeue(out var impulse))
        {
            return new ValueTask<Impulse?>(impulse);
        }
        return new ValueTask<Impulse?>(null as Impulse);
    }

    public ValueTask<IReadOnlyList<Impulse>> DequeueBatchAsync(string group, int batchSize, CancellationToken ct)
    {
        var list = new List<Impulse>();
        for (int i = 0; i < batchSize; i++)
        {
            if (_queue.TryDequeue(out var impulse))
            {
                list.Add(impulse);
            }
            else break;
        }
        return new ValueTask<IReadOnlyList<Impulse>>(list);
    }

    public ValueTask AckAsync(string group, Impulse impulse)
    {
        if (Interlocked.Decrement(ref _remainingAcks) <= 0)
        {
            _tcs.TrySetResult(true);
        }
        return ValueTask.CompletedTask;
    }

    public ValueTask AckBatchAsync(string group, IReadOnlyList<Impulse> impulses)
    {
        if (Interlocked.Add(ref _remainingAcks, -impulses.Count) <= 0)
        {
            _tcs.TrySetResult(true);
        }
        return ValueTask.CompletedTask;
    }

    public ValueTask NackAsync(string group, Impulse impulse, string reason, bool retryable) => ValueTask.CompletedTask;
}

public class MockFlowExecutor : IFlowExecutor
{
    private readonly ConcurrentDictionary<string, bool> _lockedFlows = new();
    private int _count = 0;
    public int ExecutedCount => _count;

    public async ValueTask ExecuteTickAsync(Impulse impulse)
    {
        // SIMULATE REDIS "SETNX" (Try Lock)
        // If we can't add it, it means someone else holds the lock.
        // We don't wait. We fail immediately.
        if (!_lockedFlows.TryAdd(impulse.FlowId, true))
        {
            // Simulate the network cost of the failed check
            await Task.Delay(1);
            throw new Exception("LockBusy"); // <--- FORCE FAILURE
        }

        try
        {
            // Simulate work
            Thread.SpinWait(500);
            Interlocked.Increment(ref _count);
        }
        finally
        {
            // Release Lock
            _lockedFlows.TryRemove(impulse.FlowId, out _);
        }
    }
}
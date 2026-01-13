namespace FlowWire.Framework.Core.Execution;

/// <summary>
/// A struct-based helper to manage polling intervals without allocating objects.
/// </summary>
internal struct PollingBackoff
{
    private int _missCount;

    public async ValueTask WaitAsync(TimeSpan pollInterval, CancellationToken ct)
    {
        if (_missCount < 20)
        {
            _missCount++;
        }

        var delay = Math.Min((int)(_missCount * pollInterval.TotalMilliseconds), 1000);

        await Task.Delay(delay, ct);
    }

    public void Reset()
    {
        _missCount = 0;
    }
}

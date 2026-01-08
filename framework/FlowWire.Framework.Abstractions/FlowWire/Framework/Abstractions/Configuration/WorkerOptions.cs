namespace FlowWire.Framework.Abstractions.Configuration;

public class WorkerOptions
{
    /// <summary>
    /// The maximum number of Flows this specific node will process in parallel.
    /// Adjust based on CPU/RAM resources.
    /// Default: 10
    /// </summary>
    public int MaxConcurrentFlows { get; set; } = 10;

    /// <summary>
    /// The maximum number of Drivers (Activities) this node will execute in parallel.
    /// Default: 20
    /// </summary>
    public int MaxConcurrentDrivers { get; set; } = 20;

    /// <summary>
    /// How long to wait for active tasks to finish when the application is stopping.
    /// Default: 30 seconds.
    /// </summary>
    public TimeSpan ShutdownGracePeriod { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// The polling interval for checking queues if no events are received.
    /// (Ideally we use BlockingPop, but a fallback poll is often needed).
    /// </summary>
    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromMilliseconds(500);
}
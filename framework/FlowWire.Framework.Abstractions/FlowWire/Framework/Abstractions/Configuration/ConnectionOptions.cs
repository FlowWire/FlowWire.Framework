namespace FlowWire.Framework.Abstractions.Configuration;

public class ConnectionOptions
{
    /// <summary>
    /// The Redis connection string (e.g., "localhost:6379,password=...").
    /// </summary>
    public string ConnectionString { get; set; } = "localhost:6379";

    /// <summary>
    /// The specific Redis Database index to use (0-15). 
    /// Useful for separating Test environments from Dev on the same server.
    /// Default: 0
    /// </summary>
    public int DatabaseIndex { get; set; } = 0;

    /// <summary>
    /// The prefix for all Redis keys (e.g., "fw").
    /// Example: "fw:flow:123"
    /// </summary>
    public string KeyPrefix { get; set; } = "fw";
}
using MemoryPack;

namespace FlowWire.Framework.Core.Storage;

/// <summary>
/// Represents a reference to data stored in an external backend (Cold Storage).
/// This object is serialized into the State instead of the actual heavy object.
/// </summary>
[MemoryPackable]
internal partial class BlobPointer
{
    /// <summary>
    /// Gets or sets the name of the storage resource associated with this instance.
    /// </summary>
    public string StorageName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the unique reference key used to locate the data in the storage backend.
    /// </summary>
    public string ReferenceKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the integrity check hash for the stored data.
    /// </summary>
    public string Hash { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the original size of the data before it was stored.
    /// </summary>
    public long OriginalSize { get; set; } = 0;
}

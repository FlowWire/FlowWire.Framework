namespace FlowWire.Framework.Abstractions.Storage;

public interface IStorage
{
    Task<string> WriteAsync(string blobName, byte[] data, CancellationToken ct = default);

    Task<byte[]> ReadAsync(string referenceKey, CancellationToken ct = default);
}
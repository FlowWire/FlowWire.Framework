namespace FlowWire.Framework.Abstractions.Storage;

internal record AutoStorageRule(long MaxBytes, StorageDecision Decision);

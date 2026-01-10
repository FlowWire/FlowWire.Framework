namespace FlowWire.Framework.Abstractions.Storage;

public interface IKeyStrategy
{
    string GetLockKey(string flowId, char separator);

    string GetStateKey(string flowId, char separator);

    string GetQueuePendingKey(string group, char separator);

    string GetQueueInflightKey(string group, char separator);

    string GetQueueDlqKey(string group, char separator);
}
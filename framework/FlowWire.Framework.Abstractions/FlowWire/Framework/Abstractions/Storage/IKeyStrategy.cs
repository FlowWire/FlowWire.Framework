namespace FlowWire.Framework.Abstractions.Storage;

public interface IKeyStrategy
{
    string GetLockKey(string flowId, char separator);

    string GetStateKey(string flowId, char separator);
}
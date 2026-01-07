namespace FlowWire.Framework.Abstractions;

public readonly struct DriverProxy(string driverName)
{

    public FlowCommand Drive(string method)
    {
        return new DriverCommand(driverName, method, []);
    }

    public FlowCommand Drive<T1>(string method, T1 arg1)
    {
        return new DriverCommand(driverName, method, [arg1]);
    }

    public FlowCommand Drive<T1, T2>(string method, T1 arg1, T2 arg2)
    {
        return new DriverCommand(driverName, method, [arg1, arg2]);
    }
    public FlowCommand Drive<T1, T2, T3>(string method, T1 arg1, T2 arg2, T3 arg3)
    {
        return new DriverCommand(driverName, method, [arg1, arg2, arg3]);
    }

    public FlowCommand Drive<T1, T2, T3, T4>(string method, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        return new DriverCommand(driverName, method, [arg1, arg2, arg3, arg4]);
    }

    public FlowCommand Drive<T1, T2, T3, T4, T5>(string method, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
    {
        return new DriverCommand(driverName, method, [arg1, arg2, arg3, arg4, arg5]);
    }

    public FlowCommand Drive<T1, T2, T3, T4, T5, T6>(string method, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
    {
        return new DriverCommand(driverName, method, [arg1, arg2, arg3, arg4, arg5, arg6]);
    }

    public FlowCommand Drive<T1, T2, T3, T4, T5, T6, T7>(string method, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
    {
        return new DriverCommand(driverName, method, [arg1, arg2, arg3, arg4, arg5, arg6, arg7]);
    }

    public FlowCommand Drive<T1, T2, T3, T4, T5, T6, T7, T8>(string method, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
    {
        return new DriverCommand(driverName, method, [arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8]);
    }

    public FlowCommand Drive(string method, params object?[] args)
    {
        return new DriverCommand(driverName, method, args);
    }

    public DriverCommand<TResult> Drive<TResult>(string method)
    {
        return new DriverCommand<TResult>(driverName, method, []);
    }

    public DriverCommand<TResult> Drive<TResult, T1>(string method, T1 arg1)
    {
        return new DriverCommand<TResult>(driverName, method, [arg1]);
    }

    public DriverCommand<TResult> Drive<TResult, T1, T2>(string method, T1 arg1, T2 arg2)
    {
        return new DriverCommand<TResult>(driverName, method, [arg1, arg2]);
    }

    public DriverCommand<TResult> Drive<TResult, T1, T2, T3>(string method, T1 arg1, T2 arg2, T3 arg3)
    {
        return new DriverCommand<TResult>(driverName, method, [arg1, arg2, arg3]);
    }

    public DriverCommand<TResult> Drive<TResult, T1, T2, T3, T4>(string method, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        return new DriverCommand<TResult>(driverName, method, [arg1, arg2, arg3, arg4]);
    }

    public DriverCommand<TResult> Drive<TResult, T1, T2, T3, T4, T5>(string method, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
    {
        return new DriverCommand<TResult>(driverName, method, [arg1, arg2, arg3, arg4, arg5]);
    }

    public DriverCommand<TResult> Drive<TResult, T1, T2, T3, T4, T5, T6>(string method, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
    {
        return new DriverCommand<TResult>(driverName, method, [arg1, arg2, arg3, arg4, arg5, arg6]);
    }

    public DriverCommand<TResult> Drive<TResult, T1, T2, T3, T4, T5, T6, T7>(string method, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
    {
        return new DriverCommand<TResult>(driverName, method, [arg1, arg2, arg3, arg4, arg5, arg6, arg7]);
    }

    public DriverCommand<TResult> Drive<TResult, T1, T2, T3, T4, T5, T6, T7, T8>(string method, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
    {
        return new DriverCommand<TResult>(driverName, method, [arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8]);
    }

    public DriverCommand<TResult> Drive<TResult>(string method, params object?[] args)
    {
        return new DriverCommand<TResult>(driverName, method, args);
    }
}

public readonly struct DriverProxy<T>
{
    private readonly string _driverName;

    public DriverProxy()
    {
        _driverName = typeof(T).Name;
    }

    public FlowCommand Drive(string method)
    {
        return new DriverCommand(_driverName, method, []);
    }

    public FlowCommand Drive<T1>(string method, T1 arg1)
    {
        return new DriverCommand(_driverName, method, [arg1]);
    }

    public FlowCommand Drive<T1, T2>(string method, T1 arg1, T2 arg2)
    {
        return new DriverCommand(_driverName, method, [arg1, arg2]);
    }

    public FlowCommand Drive<T1, T2, T3>(string method, T1 arg1, T2 arg2, T3 arg3)
    {
        return new DriverCommand(_driverName, method, [arg1, arg2, arg3]);
    }

    public FlowCommand Drive<T1, T2, T3, T4>(string method, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        return new DriverCommand(_driverName, method, [arg1, arg2, arg3, arg4]);
    }

    public FlowCommand Drive<T1, T2, T3, T4, T5>(string method, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
    {
        return new DriverCommand(_driverName, method, [arg1, arg2, arg3, arg4, arg5]);
    }

    public FlowCommand Drive<T1, T2, T3, T4, T5, T6>(string method, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
    {
        return new DriverCommand(_driverName, method, [arg1, arg2, arg3, arg4, arg5, arg6]);
    }

    public FlowCommand Drive<T1, T2, T3, T4, T5, T6, T7>(string method, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
    {
        return new DriverCommand(_driverName, method, [arg1, arg2, arg3, arg4, arg5, arg6, arg7]);
    }

    public FlowCommand Drive<T1, T2, T3, T4, T5, T6, T7, T8>(string method, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
    {
        return new DriverCommand(_driverName, method, [arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8]);
    }

    public FlowCommand Drive(string method, params object?[] args)
    {
        return new DriverCommand(_driverName, method, args);
    }

    public DriverCommand<TResult> Drive<TResult>(string method)
    {
        return new DriverCommand<TResult>(_driverName, method, []);
    }

    public DriverCommand<TResult> Drive<TResult, T1>(string method, T1 arg1)
    {
        return new DriverCommand<TResult>(_driverName, method, [arg1]);
    }

    public DriverCommand<TResult> Drive<TResult, T1, T2>(string method, T1 arg1, T2 arg2)
    {
        return new DriverCommand<TResult>(_driverName, method, [arg1, arg2]);
    }

    public DriverCommand<TResult> Drive<TResult, T1, T2, T3>(string method, T1 arg1, T2 arg2, T3 arg3)
    {
        return new DriverCommand<TResult>(_driverName, method, [arg1, arg2, arg3]);
    }

    public DriverCommand<TResult> Drive<TResult, T1, T2, T3, T4>(string method, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        return new DriverCommand<TResult>(_driverName, method, [arg1, arg2, arg3, arg4]);
    }

    public DriverCommand<TResult> Drive<TResult, T1, T2, T3, T4, T5>(string method, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
    {
        return new DriverCommand<TResult>(_driverName, method, [arg1, arg2, arg3, arg4, arg5]);
    }

    public DriverCommand<TResult> Drive<TResult, T1, T2, T3, T4, T5, T6>(string method, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
    {
        return new DriverCommand<TResult>(_driverName, method, [arg1, arg2, arg3, arg4, arg5, arg6]);
    }

    public DriverCommand<TResult> Drive<TResult, T1, T2, T3, T4, T5, T6, T7>(string method, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
    {
        return new DriverCommand<TResult>(_driverName, method, [arg1, arg2, arg3, arg4, arg5, arg6, arg7]);
    }

    public DriverCommand<TResult> Drive<TResult, T1, T2, T3, T4, T5, T6, T7, T8>(string method, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
    {
        return new DriverCommand<TResult>(_driverName, method, [arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8]);
    }

    public DriverCommand<TResult> Drive<TResult>(string method, params object?[] args)
    {
        return new DriverCommand<TResult>(_driverName, method, args);
    }
}

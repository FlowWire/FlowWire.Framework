namespace FlowWire.Framework.Abstractions;

/// <summary>
/// The static factory for creating commands. 
/// </summary>
public static class Command
{
    #region THE MUSCLE (Activity Execution) 

    // The "Magic String" fallback
    public static FlowCommand Run(string activityName, params object[] args)
    {
        return new Internal.ScheduleOperation(activityName, args);
    }

    public static FlowCommand Run(string activityName)
    {
        return new Internal.ScheduleOperation(activityName, []);
    }

    public static FlowCommand Run<T>(string activityName, T arg)
    {
        return new Internal.ScheduleOperation(activityName, [arg!]);
    }

    public static FlowCommand Run<T1, T2>(string activityName, T1 arg1, T2 arg2)
    {
        return new Internal.ScheduleOperation(activityName, [arg1!, arg2!]);
    }

    public static FlowCommand Run<T1, T2, T3>(string activityName, T1 arg1, T2 arg2, T3 arg3)
    {
        return new Internal.ScheduleOperation(activityName, [arg1!, arg2!, arg3!]);
    }

    public static FlowCommand Run<T1, T2, T3, T4>(string activityName, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        return new Internal.ScheduleOperation(activityName, [arg1!, arg2!, arg3!, arg4!]);
    }

    public static FlowCommand Run<T1, T2, T3, T4, T5>(string activityName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
    {
        return new Internal.ScheduleOperation(activityName, [arg1!, arg2!, arg3!, arg4!, arg5!]);
    }

    public static FlowCommand Run<T1, T2, T3, T4, T5, T6>(string activityName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
    {
        return new Internal.ScheduleOperation(activityName, [arg1!, arg2!, arg3!, arg4!, arg5!, arg6!]);
    }

    public static FlowCommand Run<T1, T2, T3, T4, T5, T6, T7>(string activityName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
    {
        return new Internal.ScheduleOperation(activityName, [arg1!, arg2!, arg3!, arg4!, arg5!, arg6!, arg7!]);
    }

    public static FlowCommand Run<T1, T2, T3, T4, T5, T6, T7, T8>(string activityName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
    {
        return new Internal.ScheduleOperation(activityName, [arg1!, arg2!, arg3!, arg4!, arg5!, arg6!, arg7!, arg8!]);
    }

    public static FlowCommand Run<T1, T2, T3, T4, T5, T6, T7, T8, T9>(string activityName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
    {
        return new Internal.ScheduleOperation(activityName, [arg1!, arg2!, arg3!, arg4!, arg5!, arg6!, arg7!, arg8!, arg9!]);
    }

    // Typed Return Overloads

    public static OperationCommand<TResult> Run<TResult>(string activityName, params object[] args)
    {
        return new OperationCommand<TResult>(activityName, args);
    }

    public static OperationCommand<TResult> Run<TResult>(string activityName)
    {
        return new OperationCommand<TResult>(activityName, []);
    }

    public static OperationCommand<TResult> Run<TResult, T1>(string activityName, T1 arg1)
    {
        return new OperationCommand<TResult>(activityName, [arg1!]);
    }

    public static OperationCommand<TResult> Run<TResult, T1, T2>(string activityName, T1 arg1, T2 arg2)
    {
        return new OperationCommand<TResult>(activityName, [arg1!, arg2!]);
    }

    public static OperationCommand<TResult> Run<TResult, T1, T2, T3>(string activityName, T1 arg1, T2 arg2, T3 arg3)
    {
        return new OperationCommand<TResult>(activityName, [arg1!, arg2!, arg3!]);
    }

    public static OperationCommand<TResult> Run<TResult, T1, T2, T3, T4>(string activityName, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        return new OperationCommand<TResult>(activityName, [arg1!, arg2!, arg3!, arg4!]);
    }

    public static OperationCommand<TResult> Run<TResult, T1, T2, T3, T4, T5>(string activityName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
    {
        return new OperationCommand<TResult>(activityName, [arg1!, arg2!, arg3!, arg4!, arg5!]);
    }

    public static OperationCommand<TResult> Run<TResult, T1, T2, T3, T4, T5, T6>(string activityName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
    {
        return new OperationCommand<TResult>(activityName, [arg1!, arg2!, arg3!, arg4!, arg5!, arg6!]);
    }

    public static OperationCommand<TResult> Run<TResult, T1, T2, T3, T4, T5, T6, T7>(string activityName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
    {
        return new OperationCommand<TResult>(activityName, [arg1!, arg2!, arg3!, arg4!, arg5!, arg6!, arg7!]);
    }

    public static OperationCommand<TResult> Run<TResult, T1, T2, T3, T4, T5, T6, T7, T8>(string activityName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
    {
        return new OperationCommand<TResult>(activityName, [arg1!, arg2!, arg3!, arg4!, arg5!, arg6!, arg7!, arg8!]);
    }

    public static OperationCommand<TResult> Run<TResult, T1, T2, T3, T4, T5, T6, T7, T8, T9>(string activityName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
    {
        return new OperationCommand<TResult>(activityName, [arg1!, arg2!, arg3!, arg4!, arg5!, arg6!, arg7!, arg8!, arg9!]);
    }

    #endregion THE MUSCLE (Activity Execution) 


    #region THE PROXY (Type-Safe Execution)

    public static OperationProxy<TActivity> Call<TActivity>()
    {
        return new OperationProxy<TActivity>();
    }

    #endregion THE PROXY (Type-Safe Execution)

    #region THE NERVOUS SYSTEM (Events & Time)

    public static FlowCommand WaitForSignal(string signalName, TimeSpan? timeout = null)
    {
        return new Internal.WaitForSignal(signalName, timeout);
    }

    public static FlowCommand Wait(TimeSpan duration)
    {
        return new Internal.WaitTimer(duration);
    }

    #endregion THE NERVOUS SYSTEM (Events & Time)

    #region FLOW CONTROL

    public static FlowCommand Race(params FlowCommand[] branches)
    {
        return new Internal.RaceGroup(branches);
    }

    public static FlowCommand Finish(object? output = null)
    {
        return new Internal.Complete(output);
    }

    public static FlowCommand Fail(string reason)
    {
        return new Internal.Fail(reason);
    }

    #endregion FLOW CONTROL
}

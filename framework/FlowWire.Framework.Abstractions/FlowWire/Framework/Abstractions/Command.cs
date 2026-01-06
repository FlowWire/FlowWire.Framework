namespace FlowWire.Framework.Abstractions;

/// <summary>
/// The static factory for creating commands. 
/// </summary>
public static class Command
{
    #region THE MUSCLE (Activity Execution) 

    // The "Magic String" fallback
    public static WorkflowCommand Run(string activityName, params object[] args)
    {
        return new Internal.ScheduleActivity(activityName, args);
    }

    public static WorkflowCommand Run(string activityName)
    {
        return new Internal.ScheduleActivity(activityName, []);
    }

    public static WorkflowCommand Run<T>(string activityName, T arg)
    {
        return new Internal.ScheduleActivity(activityName, [arg!]);
    }

    public static WorkflowCommand Run<T1, T2>(string activityName, T1 arg1, T2 arg2)
    {
        return new Internal.ScheduleActivity(activityName, [arg1!, arg2!]);
    }

    public static WorkflowCommand Run<T1, T2, T3>(string activityName, T1 arg1, T2 arg2, T3 arg3)
    {
        return new Internal.ScheduleActivity(activityName, [arg1!, arg2!, arg3!]);
    }

    public static WorkflowCommand Run<T1, T2, T3, T4>(string activityName, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        return new Internal.ScheduleActivity(activityName, [arg1!, arg2!, arg3!, arg4!]);
    }

    public static WorkflowCommand Run<T1, T2, T3, T4, T5>(string activityName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
    {
        return new Internal.ScheduleActivity(activityName, [arg1!, arg2!, arg3!, arg4!, arg5!]);
    }

    public static WorkflowCommand Run<T1, T2, T3, T4, T5, T6>(string activityName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
    {
        return new Internal.ScheduleActivity(activityName, [arg1!, arg2!, arg3!, arg4!, arg5!, arg6!]);
    }

    public static WorkflowCommand Run<T1, T2, T3, T4, T5, T6, T7>(string activityName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
    {
        return new Internal.ScheduleActivity(activityName, [arg1!, arg2!, arg3!, arg4!, arg5!, arg6!, arg7!]);
    }

    public static WorkflowCommand Run<T1, T2, T3, T4, T5, T6, T7, T8>(string activityName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
    {
        return new Internal.ScheduleActivity(activityName, [arg1!, arg2!, arg3!, arg4!, arg5!, arg6!, arg7!, arg8!]);
    }

    public static WorkflowCommand Run<T1, T2, T3, T4, T5, T6, T7, T8, T9>(string activityName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
    {
        return new Internal.ScheduleActivity(activityName, [arg1!, arg2!, arg3!, arg4!, arg5!, arg6!, arg7!, arg8!, arg9!]);
    }

    // Typed Return Overloads

    public static ActivityCommand<TResult> Run<TResult>(string activityName, params object[] args)
    {
        return new ActivityCommand<TResult>(activityName, args);
    }

    public static ActivityCommand<TResult> Run<TResult>(string activityName)
    {
        return new ActivityCommand<TResult>(activityName, []);
    }

    public static ActivityCommand<TResult> Run<TResult, T1>(string activityName, T1 arg1)
    {
        return new ActivityCommand<TResult>(activityName, [arg1!]);
    }

    public static ActivityCommand<TResult> Run<TResult, T1, T2>(string activityName, T1 arg1, T2 arg2)
    {
        return new ActivityCommand<TResult>(activityName, [arg1!, arg2!]);
    }

    public static ActivityCommand<TResult> Run<TResult, T1, T2, T3>(string activityName, T1 arg1, T2 arg2, T3 arg3)
    {
        return new ActivityCommand<TResult>(activityName, [arg1!, arg2!, arg3!]);
    }

    public static ActivityCommand<TResult> Run<TResult, T1, T2, T3, T4>(string activityName, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        return new ActivityCommand<TResult>(activityName, [arg1!, arg2!, arg3!, arg4!]);
    }

    public static ActivityCommand<TResult> Run<TResult, T1, T2, T3, T4, T5>(string activityName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
    {
        return new ActivityCommand<TResult>(activityName, [arg1!, arg2!, arg3!, arg4!, arg5!]);
    }

    public static ActivityCommand<TResult> Run<TResult, T1, T2, T3, T4, T5, T6>(string activityName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
    {
        return new ActivityCommand<TResult>(activityName, [arg1!, arg2!, arg3!, arg4!, arg5!, arg6!]);
    }

    public static ActivityCommand<TResult> Run<TResult, T1, T2, T3, T4, T5, T6, T7>(string activityName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
    {
        return new ActivityCommand<TResult>(activityName, [arg1!, arg2!, arg3!, arg4!, arg5!, arg6!, arg7!]);
    }

    public static ActivityCommand<TResult> Run<TResult, T1, T2, T3, T4, T5, T6, T7, T8>(string activityName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
    {
        return new ActivityCommand<TResult>(activityName, [arg1!, arg2!, arg3!, arg4!, arg5!, arg6!, arg7!, arg8!]);
    }

    public static ActivityCommand<TResult> Run<TResult, T1, T2, T3, T4, T5, T6, T7, T8, T9>(string activityName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
    {
        return new ActivityCommand<TResult>(activityName, [arg1!, arg2!, arg3!, arg4!, arg5!, arg6!, arg7!, arg8!, arg9!]);
    }

    #endregion THE MUSCLE (Activity Execution) 


    #region THE PROXY (Type-Safe Execution)

    public static ActivityProxy<TActivity> Call<TActivity>()
    {
        return new ActivityProxy<TActivity>();
    }

    #endregion THE PROXY (Type-Safe Execution)

    #region THE NERVOUS SYSTEM (Events & Time)

    public static WorkflowCommand WaitForSignal(string signalName, TimeSpan? timeout = null)
    {
        return new Internal.WaitForSignal(signalName, timeout);
    }

    public static WorkflowCommand Wait(TimeSpan duration)
    {
        return new Internal.WaitTimer(duration);
    }

    #endregion THE NERVOUS SYSTEM (Events & Time)

    #region FLOW CONTROL

    public static WorkflowCommand Race(params WorkflowCommand[] branches)
    {
        return new Internal.RaceGroup(branches);
    }

    public static WorkflowCommand Finish(object? output = null)
    {
        return new Internal.Complete(output);
    }

    public static WorkflowCommand Fail(string reason)
    {
        return new Internal.Fail(reason);
    }

    #endregion FLOW CONTROL
}

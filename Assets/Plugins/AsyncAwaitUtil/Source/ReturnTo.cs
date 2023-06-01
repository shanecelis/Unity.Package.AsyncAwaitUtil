using System;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityAsyncAwaitUtil;

public static class ReturnTo
{
    public abstract class ReturnToThread : INotifyCompletion
    {
        public ReturnToThread GetAwaiter() => this;
        public void GetResult() { }

        public abstract bool IsCompleted { get; }
        public abstract void OnCompleted(Action continuation);
    }

    public class ReturnToOnUnityThread : ReturnToThread
    {
        public override bool IsCompleted => true;

        public override void OnCompleted(Action continuation)
        {
            // This should never be called
            // Let's invoke continuation directly in case the unthinkable happened
            continuation?.Invoke();
        }
    }

    public class ReturnToNotOnUnityThread : ReturnToThread
    {
        public override bool IsCompleted => false;

        public override void OnCompleted(Action continuation)
        {
            if (continuation != null)
                SyncContextUtil.UnitySynchronizationContext.Post(_ => continuation(), null);
        }
    }

    // Pool objects to prevent memory allocations
    private static ReturnToThread onMainThreadTask = new ReturnToOnUnityThread();
    private static ReturnToThread notOnMainThreadTask = new ReturnToNotOnUnityThread();

    /// <summary>
    /// Ensures that the caller continues on the main thread when await finishes
    /// </summary>
    public static ReturnToThread MainThread
    {
        get
        {
            bool onMainThread = SynchronizationContext.Current == SyncContextUtil.UnitySynchronizationContext;
            return onMainThread ? onMainThreadTask : notOnMainThreadTask;
        }
    }
}

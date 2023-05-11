using System.Threading;
using UnityEngine;

namespace UnityAsyncAwaitUtil
{
    public static class SyncContextUtil
    {
#if UNITY_EDITOR
        static System.Reflection.MethodInfo executionMethod;

        /// <summary>
        /// HACK: makes Unity Editor execute continuations in edit mode.
        /// </summary>
        private static void ExecuteContinuations()
        {
            if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            var context = SynchronizationContext.Current;

            if (executionMethod == null)
            {
                executionMethod = context.GetType().GetMethod("Exec", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            }

            executionMethod?.Invoke(context, null);
        }
        
        [UnityEditor.InitializeOnLoadMethod]
#endif
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Initialize()
        {
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying) UnityEditor.EditorApplication.update += ExecuteContinuations;
            else UnityEditor.EditorApplication.update -= ExecuteContinuations;
#endif
            UnitySynchronizationContext = SynchronizationContext.Current;
            UnityThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        public static int UnityThreadId
        {
            get; private set;
        }
        public static SynchronizationContext UnitySynchronizationContext
        {
            get; private set;
        }
    }
}


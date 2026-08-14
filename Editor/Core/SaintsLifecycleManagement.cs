using UnityEngine.Events;
// ReSharper disable once RedundantUsingDirective
using UnityEditor;

namespace SaintsField.Editor.Core
{
    // ReSharper disable once PartialTypeWithSinglePart
    public partial class SaintsLifecycleManagement
    {
        public static readonly UnityEvent OnCodeUnloadingEvent = new UnityEvent();

        private static void OnCodeUnloading()
        {
            OnCodeUnloadingEvent.Invoke();
        }

#if UNITY_6000_5_OR_NEWER
        [Unity.Scripting.LifecycleManagement.OnCodeUnloading]
        private static void LifecycleManagementOnCodeUnloading() => OnCodeUnloading();
#else
        [InitializeOnLoadMethod]
        private static void RegisterAssemblyCacheCleanup()
        {
            AssemblyReloadEvents.beforeAssemblyReload -= OnCodeUnloading;
            AssemblyReloadEvents.beforeAssemblyReload += OnCodeUnloading;
        }
#endif
    }
}

using UnityEngine.Events;
// ReSharper disable once RedundantUsingDirective
using UnityEditor;

namespace SaintsField.Editor.Core
{
    public partial class SaintsLifecycleManagement
    {
        public static readonly UnityEvent OnCodeUnloadingEvent = new UnityEvent();

#if UNITY_6000_5_OR_NEWER
        [Unity.Scripting.LifecycleManagement.OnCodeUnloading]
        private static void OnCodeUnloading()
        {
            OnCodeUnloadingEvent.Invoke();
        }
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

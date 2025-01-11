using UnityEditor;
using UnityEngine.Events;

namespace SaintsField.Editor.Core
{
    public static class SaintsEditorApplicationChanged
    {
        public static readonly UnityEvent OnAnyEvent = new UnityEvent();

        [InitializeOnLoadMethod]
        private static void OnLoad()
        {
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
            EditorApplication.projectChanged += OnProjectChanged;
        }

        public static readonly UnityEvent OnHierarchyChangedEvent = new UnityEvent();
        private static void OnHierarchyChanged()
        {
            OnHierarchyChangedEvent.Invoke();
            OnAnyEvent.Invoke();
        }

        public static readonly UnityEvent OnProjectChangedEvent = new UnityEvent();
        private static void OnProjectChanged()
        {
            OnProjectChangedEvent.Invoke();
            OnAnyEvent.Invoke();
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.TroubleshootEditor
{
    public class TroubleshootEditorWindow: SaintsEditorWindow
    {
#if SAINTSFIELD_DEBUG
        [MenuItem("Saints/Troubleshoot Editor")]
#else
        [MenuItem("Window/Saints/Troubleshoot Editor")]
#endif
        public static void TestOpenWindow()
        {
            EditorWindow window = GetWindow<TroubleshootEditorWindow>(false, "Troubleshoot Editor");
            window.Show();
        }

        [ReadOnly, ProgressBar(maxCallback: nameof(_maxCount))] public int progress;

        private int _maxCount = 1;

        [Button("Check")]
        public IEnumerator Check()
        {
            int total = 0;
            progress = 0;
            _maxCount = 1;
            foreach (Assembly asb in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] allTypes = asb.GetTypes();
                List<Type> allEditors = allTypes
                    .Where(type => type.IsSubclassOf(typeof(UnityEditor.Editor)))
                    .ToList();
                total += allEditors.Count;
                if(total > 0)
                {
                    _maxCount = total;
                }
                yield return null;

                foreach (Type eachEditorType in allEditors)
                {
                    progress++;
                    yield return null;
                    foreach (CustomEditor customEditor in eachEditorType.GetCustomAttributes<CustomEditor>(true))
                    {
                        object v = typeof(CustomEditor)
                            .GetField("m_InspectedType",
                                BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
                                BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.FlattenHierarchy)
                            ?.GetValue(customEditor);
                        Debug.Log($"Found editor: {eachEditorType} -> {customEditor}: {v}");
                    }

                }
            }
        }
    }
}

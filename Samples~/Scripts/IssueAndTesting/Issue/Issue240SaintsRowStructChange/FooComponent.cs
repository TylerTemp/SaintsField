using System;
using System.Reflection;
using SaintsField.Playa;
using SaintsField.Samples.Scripts.SaintsEditor;
// using UnityEditor;

// using UnityEditor.SceneManagement;
// using UnityEngine.SceneManagement;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue240SaintsRowStructChange
{
    public class FooComponent : SaintsMonoBehaviour
    {
        public Foo1 foo1;

        [Serializable]
        public struct Foo1
        {
            public int foo;

            [Button]
            private void IncrementFoo()
            {
                // EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                // Undo.RecordObject(this, nameof(IncrementFoo));
                // FieldInfo field = typeof(FooComponent).GetField("foo1");
                // FooComponent[] components = FindObjectsOfType<FooComponent>();
                // foreach (FooComponent comp in components)
                // {
                //     object value = field.GetValue(comp);
                //     if (value is Foo1 foo1 && foo1.Equals(this))
                //     {
                //         // 'comp' is the FooComponent holding this Foo1 instance
                //         // You can use 'comp' here as needed
                //         SerializedObject so = new SerializedObject(comp);
                //         SerializedProperty prop = so.FindProperty("foo1.foo");
                //         prop.intValue++;
                //         so.ApplyModifiedProperties();
                //         // break;
                //     }
                // }

                foo++;
            }
        }
    }
}

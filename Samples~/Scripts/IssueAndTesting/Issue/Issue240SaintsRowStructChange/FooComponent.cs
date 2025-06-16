using System;
using SaintsField.Playa;
using SaintsField.Samples.Scripts.SaintsEditor;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue240SaintsRowStructChange
{
    public class FooComponent : SaintsMonoBehaviour
    {
        public Foo1 foo1;

        [Serializable]
        public struct Foo1
        {
            [BelowButton(nameof(FooInline))]
            public int fooInline;

            private void FooInline()
            {
#if UNITY_EDITOR
                SaintsContext.SerializedProperty.intValue++;
                SaintsContext.SerializedProperty.serializedObject.ApplyModifiedProperties();
#endif
            }

            public int foo;

            [Button]
            private void IncrementFoo()
            {
#if UNITY_EDITOR
                SaintsContext.SerializedProperty.FindPropertyRelative(nameof(foo)).intValue++;
                SaintsContext.SerializedProperty.serializedObject.ApplyModifiedProperties();
#endif
            }


        }
    }
}

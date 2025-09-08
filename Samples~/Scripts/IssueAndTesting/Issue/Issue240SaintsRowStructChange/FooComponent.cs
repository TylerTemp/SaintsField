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
                SaintsContext.SerializedProperty.intValue++;  // for inline buttons we get the decorated property
                SaintsContext.SerializedProperty.serializedObject.ApplyModifiedProperties();
#endif
            }

            public int foo;

            [Button]
            private void IncrementFoo()
            {
#if UNITY_EDITOR
                // for `Button` we get the property of the container (`foo1` field here)
                SaintsContext.FindPropertyRelateTo(nameof(foo)).intValue++;
                SaintsContext.SerializedProperty.serializedObject.ApplyModifiedProperties();
#endif
            }


        }

        [Serializable]
        public struct FloatWithBaseValue
        {
            [OnValueChanged(nameof(RevertFinalToBaseValue))] public float baseValue;
            public float finalValue;

            public void RevertFinalToBaseValue()
            {
                finalValue = baseValue;
            }

            [OnValueChanged(nameof(RevertFinalToBaseValue2))] public float baseValue2;
            public float finalValue2;

            public void RevertFinalToBaseValue2()
            {
#if UNITY_EDITOR
                SaintsContext.FindPropertyRelateTo(nameof(finalValue2)).floatValue = baseValue2;
                SaintsContext.SerializedProperty.serializedObject.ApplyModifiedProperties();
#endif
            }
        }

        public FloatWithBaseValue fwb;
    }
}

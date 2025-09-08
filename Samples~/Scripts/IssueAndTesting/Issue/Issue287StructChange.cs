using System;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue287StructChange : MonoBehaviour
    {
        [Serializable]
        public struct FloatWithBaseValue
        {
            [OnValueChanged((nameof(RevertFinalToBaseValue)))]
            public float baseValue;

            public float finalValue;

            public void RevertFinalToBaseValue()
            {
                finalValue = baseValue;
            }
        }

        public FloatWithBaseValue floatWithBaseValue;

        [Serializable]
        public struct FloatWithBaseValueFix
        {
            [OnValueChanged((nameof(RevertFinalToBaseValue)))]
            public float baseValue;

            public float finalValue;

            [field: SerializeField] public float AutoFinalValue { get; private set; }

            public void RevertFinalToBaseValue()
            {
#if UNITY_EDITOR
                SaintsContext.FindPropertyRelateTo(nameof(finalValue)).floatValue = baseValue;
                SaintsContext.SerializedProperty.serializedObject.ApplyModifiedProperties();

                SaintsContext.FindPropertyRelateTo(nameof(AutoFinalValue)).floatValue = baseValue;
                SaintsContext.SerializedProperty.serializedObject.ApplyModifiedProperties();
#endif
            }
        }

        public FloatWithBaseValueFix floatWithBaseValueFix;
    }
}

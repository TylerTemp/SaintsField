using System;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Samples.Scripts.Issue420
{
    [Serializable]
    public struct IntWithBaseValue
    {
        public IntWithBaseValue(int val)
        {
            baseValue = val;
            finalValue = val;
        }

        public int baseValue;
        public int finalValue;

        public void RevertFinalToBaseValue() => finalValue = baseValue;
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(IntWithBaseValue), true)]
    public class IntWithBaseValueDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var baseProp = property.FindPropertyRelative("baseValue");
            var finalProp = property.FindPropertyRelative("finalValue");

            float labelWidth = EditorGUIUtility.labelWidth;
            float totalWidth = position.width;
            float remainingWidth = totalWidth - labelWidth;

            float space = 6f;
            float inputWidth = Mathf.Clamp(remainingWidth * 0.5f, 40f, 110f);
            float finalWidth = Mathf.Max(0f, remainingWidth - inputWidth - space);

            Rect intFieldRect = new Rect(position.x, position.y, labelWidth + inputWidth, position.height);
            Rect finalRect = new Rect(position.x + labelWidth + inputWidth + space, position.y, finalWidth, position.height);

            if (baseProp != null)
            {
                EditorGUI.BeginChangeCheck();
                int newBaseVal = EditorGUI.IntField(intFieldRect, label, baseProp.intValue);
                if (EditorGUI.EndChangeCheck())
                {
                    baseProp.intValue = newBaseVal;
                    if (finalProp != null)
                    {
                        finalProp.intValue = newBaseVal;
                    }
                }
            }

            if (finalProp != null)
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUI.LabelField(finalRect, $"(Final: {finalProp.intValue})");
                }
            }

            EditorGUI.EndProperty();
        }
    }
#endif
}
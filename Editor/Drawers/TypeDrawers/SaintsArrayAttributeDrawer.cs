using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.TypeDrawers
{
    [CustomPropertyDrawer(typeof(SaintsArrayAttribute))]
    public class SaintsArrayAttributeDrawer: SaintsPropertyDrawer
    {
        public static (string propName, int index) GetSerName(SerializedProperty property, SaintsArrayAttribute saintsArrayAttribute, FieldInfo fieldInfo, object parent)
        {
            if(saintsArrayAttribute.PropertyName != null)
            {
                return (saintsArrayAttribute.PropertyName, SerializedUtils.PropertyPathIndex(property.propertyPath));
            }

            object rawValue = fieldInfo.GetValue(parent);
            int arrayIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);
            ISaintsArray curValue = (ISaintsArray)(arrayIndex == -1 ? rawValue : SerializedUtils.GetValueAtIndex(rawValue, arrayIndex));

            return (curValue.EditorArrayPropertyName, arrayIndex);
        }

        #region IMGUI

        private string _imGuiPropRawName = "";

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, FieldInfo info,
            bool hasLabelWidth, object parent)
        {
            if(_imGuiPropRawName == "")
            {
                _imGuiPropRawName = GetSerName(property, (SaintsArrayAttribute) saintsAttribute, info, parent).propName;
            }
            SerializedProperty arrProperty = property.FindPropertyRelative(_imGuiPropRawName) ?? SerializedUtils.FindPropertyByAutoPropertyName(property, _imGuiPropRawName);
            return EditorGUI.GetPropertyHeight(arrProperty, label, true);
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute,
            OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            if(_imGuiPropRawName == "")
            {
                _imGuiPropRawName = GetSerName(property, (SaintsArrayAttribute) saintsAttribute, info, parent).propName;
            }
            SerializedProperty arrProperty = property.FindPropertyRelative(_imGuiPropRawName) ?? SerializedUtils.FindPropertyByAutoPropertyName(property, _imGuiPropRawName);
            EditorGUI.PropertyField(position, arrProperty, label, true);
        }

        #endregion
    }
}

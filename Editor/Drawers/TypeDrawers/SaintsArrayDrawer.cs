using System.Reflection;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace SaintsField.Editor.Drawers.TypeDrawers
{
    [CustomPropertyDrawer(typeof(SaintsList<>))]
    [CustomPropertyDrawer(typeof(SaintsArray<>))]
    public class SaintsArrayDrawer: PropertyDrawer
    {
        private static (string error, string propName, int index) GetSerName(SerializedProperty property, FieldInfo fieldInfo)
        {
            (SerializedUtils.FieldOrProp _, object parent) = SerializedUtils.GetFieldInfoAndDirectParent(property);
            object rawValue = fieldInfo.GetValue(parent);
            int arrayIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);

            (string error, int index, object value) = Util.GetValue(property, fieldInfo, parent);
            if (error != "")
            {
                return (error, null, index);
            }

            IWrapProp curValue = (IWrapProp) value;
            return ("", curValue.EditorPropertyName, arrayIndex);
        }

        #region IMGUI

        private string _imGuiPropRawName = "";

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if(_imGuiPropRawName == "")
            {
                _imGuiPropRawName = GetSerName(property, fieldInfo).propName;
            }
            SerializedProperty arrProperty = property.FindPropertyRelative(_imGuiPropRawName) ?? SerializedUtils.FindPropertyByAutoPropertyName(property, _imGuiPropRawName);
            return EditorGUI.GetPropertyHeight(arrProperty, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if(_imGuiPropRawName == "")
            {
                _imGuiPropRawName = GetSerName(property, fieldInfo).propName;
            }
            SerializedProperty arrProperty = property.FindPropertyRelative(_imGuiPropRawName) ?? SerializedUtils.FindPropertyByAutoPropertyName(property, _imGuiPropRawName);
            EditorGUI.PropertyField(position, arrProperty, label, true);
        }
        #endregion

#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            (string error, string propRawName, int curInArrayIndex) = GetSerName(property, fieldInfo);
            if (error != "")
            {
                return new HelpBox(error, HelpBoxMessageType.Error);
            }
            SerializedProperty arrProperty = property.FindPropertyRelative(propRawName) ?? SerializedUtils.FindPropertyByAutoPropertyName(property, propRawName);
            // return new PropertyField(arrProperty);
            return new PropertyField(arrProperty, curInArrayIndex == -1? property.displayName : $"Element {curInArrayIndex}");
        }
#endif
    }
}

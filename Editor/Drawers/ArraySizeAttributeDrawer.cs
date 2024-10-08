using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(ArraySizeAttribute))]
    public class ArraySizeAttributeDrawer: SaintsPropertyDrawer
    {

        #region IMGUI

        private string _error;

        protected override bool WillDrawAbove(SerializedProperty property, ISaintsAttribute saintsAttribute,
            FieldInfo info, object parent) => true;

        protected override float GetAboveExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info, object parent) => 0f;

        protected override Rect DrawAboveImGui(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute,
            OnGUIPayload onGUIPayload,
            FieldInfo info, object parent)
        {
            ArraySizeAttribute arraySizeAttribute = (ArraySizeAttribute)saintsAttribute;
            // int size = ((ArraySizeAttribute)saintsAttribute).Size;

            // Debug.Log(property.propertyPath);
            // SerializedProperty arrProp = property.serializedObject.FindProperty("nests.Array.data[0].arr3");
            // Debug.Log(arrProp);
            // Debug.Log(property.propertyPath);
            (string error, SerializedProperty arrProp) = SerializedUtils.GetArrayProperty(property);
            _error = error;
            if (_error != "")
            {
                return position;
            }

            if (arraySizeAttribute.Min >= 0 && arrProp.arraySize < arraySizeAttribute.Min)
            {
                // Debug.Log(property.arraySize);
                // Debug.Log(property.propertyPath);
                arrProp.arraySize = arraySizeAttribute.Min;
                // arrProp.serializedObject.ApplyModifiedProperties();
            }
            if (arraySizeAttribute.Max >= 0 &&  arrProp.arraySize > arraySizeAttribute.Max)
            {
                arrProp.arraySize = arraySizeAttribute.Max;
            }

            return position;
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent)
        {
            return _error != "";
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            return _error == "" ? 0 : ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent) =>
            _error == ""
                ? position
                : ImGuiHelpBox.Draw(position, _error, MessageType.Error);
        #endregion

#if UNITY_2021_3_OR_NEWER

        #region UI Toolkit

        private static string NameHelpBox(SerializedProperty property) => $"{property.propertyPath}_ArraySize_HelpBox";

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container, FieldInfo info, object parent)
        {
            HelpBox helpBox = new HelpBox("", HelpBoxMessageType.Error)
            {
                style =
                {
                    display = DisplayStyle.None,
                },
                name = NameHelpBox(property),
            };

            helpBox.AddToClassList(ClassAllowDisable);

            return helpBox;
        }

        // protected override VisualElement CreateBelowUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
        //     VisualElement container, FieldInfo info, object parent)
        // {
        //     (string error, SerializedProperty arrProp) = GetArrayProperty(property);
        //     if (error != "")
        //     {
        //         return new HelpBox(error, HelpBoxMessageType.Error);
        //     }
        //
        //     int size = ((ArraySizeAttribute)saintsAttribute).Size;
        //     if (arrProp.arraySize != size)
        //     {
        //         arrProp.arraySize = size;
        //     }
        //
        //     return new VisualElement();
        // }

        // much error...
        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChanged, FieldInfo info)
        {
            // SerializedProperty targetProperty = property;
            object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
            if (parent == null)
            {
                Debug.LogWarning($"{property.propertyPath} parent disposed unexpectedly");
                return;
            }

            (SerializedProperty arrProp, int _, string error) = Util.GetArrayProperty(property, info, parent);

            SetHelpBox(error, property, container);

            ArraySizeAttribute arraySizeAttribute = (ArraySizeAttribute)saintsAttribute;

            // ReSharper disable once InvertIf
            // ReSharper disable once MergeIntoPattern
            if (error == "")
            {
                bool changed = false;
                int curSize = arrProp.arraySize;
                if(arraySizeAttribute.Min >= 0 && curSize < arraySizeAttribute.Min)
                {
                    // Debug.Log($"change array size {curSize} to min {arraySizeAttribute.Min}");
                    arrProp.arraySize = arraySizeAttribute.Min;
                    changed = true;
                }
                if(arraySizeAttribute.Max >= 0 && curSize > arraySizeAttribute.Max)
                {
                    // Debug.Log($"change array size {curSize} to max {arraySizeAttribute.Max}");
                    arrProp.arraySize = arraySizeAttribute.Max;
                    changed = true;
                }
                if(changed)
                {
                    arrProp.serializedObject.ApplyModifiedProperties();
                }
            }
        }

        private static void SetHelpBox(string error, SerializedProperty property, VisualElement container)
        {
            HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property));
            if (error != helpBox.text)
            {
                helpBox.text = error;
                helpBox.style.display = error == "" ? DisplayStyle.None : DisplayStyle.Flex;
            }
        }

        #endregion

#endif
    }
}

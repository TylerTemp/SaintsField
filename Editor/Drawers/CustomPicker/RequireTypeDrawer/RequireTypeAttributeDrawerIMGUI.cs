using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.CustomPicker.RequireTypeDrawer
{
    public partial class RequireTypeAttributeDrawer
    {
        #region IMGUI

        // ReSharper disable once InconsistentNaming
        protected string _error { private get; set; } = "";
        protected bool ImGuiFirstChecked { get; private set; }

        private UnityEngine.Object _previousValue;

        protected override float DrawPreLabelImGui(Rect position, SerializedProperty property,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            _previousValue = property.objectReferenceValue;
            return base.DrawPreLabelImGui(position, property, saintsAttribute, info, parent);
        }

        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, OnGUIPayload onGuiPayload, FieldInfo info, object parent)
        {
            RequireTypeAttribute requireTypeAttribute = (RequireTypeAttribute)saintsAttribute;
            return requireTypeAttribute.CustomPicker ? 20 : 0;
        }

        private GUIStyle _imGuiButtonStyle;

        protected override bool DrawPostFieldImGui(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            RequireTypeAttribute requireTypeAttribute = (RequireTypeAttribute)saintsAttribute;
            IReadOnlyList<Type> requiredTypes = requireTypeAttribute.RequiredTypes;

            bool customPicker = requireTypeAttribute.CustomPicker;
            if(customPicker)
            {
                // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
                if (_imGuiButtonStyle == null)
                {
                    _imGuiButtonStyle = new GUIStyle(GUI.skin.button)
                    {
                        // margin = new RectOffset(0, 0, 0, 0),
                        padding = new RectOffset(0, 0, 0, 0),
                    };
                }

                if (GUI.Button(position, "●", _imGuiButtonStyle))
                {
                    OpenSelectorWindow(property, requireTypeAttribute, info, onGUIPayload.SetValue, parent);
                }
            }

            if (!ImGuiFirstChecked || onGUIPayload.changed)
            {
                // Debug.Log($"onGUIPayload.changed={onGUIPayload.changed}/_imGuiFirstChecked={_imGuiFirstChecked}");
                _error = "";
                // bool isFirstCheck = !_imGuiFirstChecked;
                // Debug.Log($"_imGuiFirstChecked={_imGuiFirstChecked}/freeSign={fieldInterfaceAttribute.FreeSign}");


                UnityEngine.Object curValue = GetCurFieldValue(property, requireTypeAttribute);
                if (curValue is null)
                {
                    return customPicker;
                }

                IReadOnlyList<string> missingTypeNames = GetMissingTypeNames(curValue, requiredTypes);

                // Debug.Log($"missingTypeNames={string.Join(",", missingTypeNames)}, _imGuiFirstChecked={_imGuiFirstChecked}");

                if (missingTypeNames.Count > 0)  // if has errors
                {
                    string errorMessage = $"{curValue} has no component{(missingTypeNames.Count > 1? "s": "")} {string.Join(", ", missingTypeNames)}.";
                    // freeSign will always give error information
                    // but if you never passed the first check, then sign as you want and it'll always just show error
                    if (!ImGuiFirstChecked || requireTypeAttribute.FreeSign)
                    {
                        // Debug.Log($"isFirstCheck={isFirstCheck}/freeSign={fieldInterfaceAttribute.FreeSign}");
                        _error = errorMessage;
                    }
                    else  // it's not freeSign, and you've already got a correct answer. So revert to the old value.
                    {
                        // property.objectReferenceValue = _previousValue;
                        RestorePreviousValue(property, info, parent);
                        onGUIPayload.SetValue(GetPreviousValue());
                        Debug.LogWarning($"{errorMessage} Change reverted to {(_previousValue==null? "null": _previousValue.ToString())}.");
                    }
                }
                else
                {
                    ImGuiFirstChecked = true;
                }
            }

            return customPicker;
        }

        protected virtual UnityEngine.Object GetCurFieldValue(SerializedProperty property, RequireTypeAttribute _) => property.objectReferenceValue;

        protected virtual void OpenSelectorWindow(SerializedProperty property, RequireTypeAttribute requireTypeAttribute, FieldInfo info, Action<object> onChangeCallback, object parent)
        {
            FieldInterfaceSelectWindow.Open(property.objectReferenceValue, requireTypeAttribute.EditorPick,
                ReflectUtils.GetElementType(info.FieldType), requireTypeAttribute.RequiredTypes, fieldResult =>
            {
                UnityEngine.Object result = OnSelectWindowSelected(fieldResult, ReflectUtils.GetElementType(info.FieldType));
                property.objectReferenceValue = result;
                property.serializedObject.ApplyModifiedProperties();
                // onGUIPayload.SetValue(result);
                onChangeCallback(result);
            });
        }

        protected virtual void RestorePreviousValue(SerializedProperty property, FieldInfo info, object parent)
        {
            property.objectReferenceValue = _previousValue;
            ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, _previousValue);
        }

        protected virtual object GetPreviousValue() => _previousValue;

        private static UnityEngine.Object OnSelectWindowSelected(UnityEngine.Object fieldResult, Type fieldType)
        {
            UnityEngine.Object result = null;
            switch (fieldResult)
            {
                case null:
                    // property.objectReferenceValue = null;
                    break;
                case GameObject go:
                    // ReSharper disable once RedundantCast
                    result = fieldType == typeof(GameObject) ? (UnityEngine.Object)go : go.GetComponent(fieldType);
                    // Debug.Log($"isGo={fieldType == typeof(GameObject)},  fieldResult={fieldResult.GetType()} result={result.GetType()}");
                    break;
                case Component comp:
                    result = fieldType == typeof(GameObject)
                        // ReSharper disable once RedundantCast
                        ? (UnityEngine.Object)comp.gameObject
                        : comp.GetComponent(fieldType);
                    break;
            }

            return result;
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent) => _error == "" ? 0 : ImGuiHelpBox.GetHeight(_error, EditorGUIUtility.currentViewWidth, MessageType.Error);

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent) => ImGuiHelpBox.Draw(position, _error, MessageType.Error);
        #endregion
    }
}

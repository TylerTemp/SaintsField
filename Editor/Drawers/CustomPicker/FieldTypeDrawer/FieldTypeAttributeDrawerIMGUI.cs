using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.CustomPicker.FieldTypeDrawer
{
    public partial class FieldTypeAttributeDrawer
    {
        private string _error = "";

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            float width,
            ISaintsAttribute saintsAttribute, FieldInfo info, bool hasLabelWidth, object parent) => EditorGUIUtility.singleLineHeight;

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes, OnGUIPayload onGUIPayload,
            FieldInfo info, object parent)
        {
            FieldTypeAttribute fieldTypeAttribute = (FieldTypeAttribute)saintsAttribute;
            Type fieldType = SerializedUtils.IsArrayOrDirectlyInsideArray(property)? ReflectUtils.GetElementType(info.FieldType): info.FieldType;
            Type requiredComp = fieldTypeAttribute.CompType ?? fieldType;
            Object requiredValue;
            try
            {
                requiredValue = GetValue(property, fieldType, requiredComp);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                _error = e.Message;
                DefaultDrawer(position, property, label, info);
                return;
            }

            EPick editorPick = fieldTypeAttribute.EditorPick;
            bool customPicker = fieldTypeAttribute.CustomPicker;

            // ReSharper disable once ConvertToUsingDeclaration
            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                Rect fieldRect = customPicker
                    ? new Rect(position)
                    {
                        width = position.width - 20,
                    }
                    : position;

                Object fieldResult =
                    EditorGUI.ObjectField(fieldRect, label, requiredValue, requiredComp, editorPick.HasFlagFast(EPick.Scene));
                // ReSharper disable once InvertIf
                if (changed.changed)
                {
                    Object result = GetNewValue(fieldResult, fieldType, requiredComp);
                    property.objectReferenceValue = result;

                    if (fieldResult != null && result == null)
                    {
                        _error = $"{fieldResult} has no component {fieldType}";
                    }
                }
            }

            if(customPicker)
            {
                Rect overrideButtonRect = new Rect(position.x + position.width - 21, position.y, 21, position.height);
                if (GUI.Button(overrideButtonRect, "●"))
                {
                    // Type[] types = requiredComp  == fieldType
                    //     ? new []{requiredComp}
                    //     : new []{requiredComp, fieldType};
                    FieldTypeSelectWindow.Open(property.objectReferenceValue, editorPick, fieldType, requiredComp, fieldResult =>
                    {
                        Object result = OnSelectWindowSelected(fieldResult, fieldType);
                        property.objectReferenceValue = result;
                        property.serializedObject.ApplyModifiedProperties();
                        onGUIPayload.SetValue(result);
                    });
                }
            }
        }

        private static Object OnSelectWindowSelected(Object fieldResult, Type fieldType)
        {
            return Util.GetTypeFromObj(fieldResult, fieldType);
            // Object result = null;
            // switch (fieldResult)
            // {
            //     case null:
            //         // property.objectReferenceValue = null;
            //         break;
            //     case GameObject go:
            //         result = fieldType == typeof(GameObject) ? (Object)go : go.GetComponent(fieldType);
            //         // Debug.Log($"isGo={fieldType == typeof(GameObject)},  fieldResult={fieldResult.GetType()} result={result.GetType()}");
            //         break;
            //     case Component comp:
            //         result = fieldType == typeof(GameObject)
            //             ? (Object)comp.gameObject
            //             : comp.GetComponent(fieldType);
            //         break;
            // }
            //
            // return result;
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent) => _error == "" ? 0 : ImGuiHelpBox.GetHeight(_error, EditorGUIUtility.currentViewWidth, MessageType.Error);

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            OnGUIPayload onGuiPayload, FieldInfo info, object parent) => ImGuiHelpBox.Draw(position, _error, MessageType.Error);
    }
}

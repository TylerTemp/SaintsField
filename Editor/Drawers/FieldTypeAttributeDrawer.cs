using System;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(FieldTypeAttribute))]
    public class FieldTypeAttributeDrawer: SaintsPropertyDrawer
    {
        private string _error = "";

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, bool hasLabelWidth) => EditorGUIUtility.singleLineHeight;

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            FieldTypeAttribute fieldTypeAttribute = (FieldTypeAttribute)saintsAttribute;
            Type requiredComp = fieldTypeAttribute.CompType;
            Type fieldType = SerializedUtils.GetType(property);
            UnityEngine.Object requiredValue = null;
            try
            {
                // Debug.Log(property.objectReferenceValue);

                bool fieldTypeIsGameObject = fieldType == typeof(GameObject);
                bool requiredCompIsGameObject = requiredComp == typeof(GameObject);

                if (fieldTypeIsGameObject && requiredCompIsGameObject)
                {
                    requiredValue = property.objectReferenceValue;
                }
                else if (!fieldTypeIsGameObject && !requiredCompIsGameObject)
                {
                    requiredValue = ((Component)property.objectReferenceValue)?.GetComponent(requiredComp);
                }
                else if (fieldTypeIsGameObject && !requiredCompIsGameObject)
                {
                    requiredValue = ((GameObject)property.objectReferenceValue)?.GetComponent(requiredComp);
                }
                else if (!fieldTypeIsGameObject && requiredCompIsGameObject)
                {
                    requiredValue = ((Component)property.objectReferenceValue)?.gameObject;
                }


                // switch (fieldType == typeof(GameObject), requiredComp == typeof(GameObject))
                // {
                //     case (true, true):
                //         requiredValue = property.objectReferenceValue;
                //         break;
                //     case (false, false):
                //         requiredValue = ((Component)property.objectReferenceValue)?.GetComponent(requiredComp);
                //         break;
                //     case (true, false):
                //         requiredValue = ((GameObject)property.objectReferenceValue)?.GetComponent(requiredComp);
                //         break;
                //     case (false, true):
                //         requiredValue = ((Component)property.objectReferenceValue)?.gameObject;
                //         break;
                // }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                _error = e.Message;
                DefaultDrawer(position, property, label);
                return;
            }

            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                UnityEngine.Object fieldResult =
                    EditorGUI.ObjectField(position, label, requiredValue, requiredComp, true);
                if (changed.changed)
                {
                    // UnityEngine.Object result =
                    //     (requiredComp == typeof(GameObject), fieldType == typeof(GameObject)) switch
                    //     {
                    //         (true, true) => fieldResult,
                    //         (false, false) => ((Component)fieldResult)?.GetComponent(fieldType),
                    //         (true, false) => ((GameObject)fieldResult)?.GetComponent(fieldType),
                    //         (false, true) => ((Component)fieldResult)?.gameObject,
                    //     };
                    bool requiredCompIsGameObject = requiredComp == typeof(GameObject);
                    bool fieldTypeIsGameObject = fieldType == typeof(GameObject);

                    UnityEngine.Object result = null;

                    if (requiredCompIsGameObject && fieldTypeIsGameObject)
                    {
                        result = fieldResult;
                    }
                    else if (!requiredCompIsGameObject && !fieldTypeIsGameObject)
                    {
                        result = ((Component)fieldResult)?.GetComponent(fieldType);
                    }
                    else if (requiredCompIsGameObject && !fieldTypeIsGameObject)
                    {
                        result = ((GameObject)fieldResult)?.GetComponent(fieldType);
                    }
                    else if (!requiredCompIsGameObject && fieldTypeIsGameObject)
                    {
                        result = ((Component)fieldResult)?.gameObject;
                    }

                    property.objectReferenceValue = result;

                    if (fieldResult != null && result == null)
                    {
                        _error = $"{fieldResult} has no component {fieldType}";
                    }
                }
            }
        }

        protected override bool WillDrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute) => _error == "" ? 0 : HelpBox.GetHeight(_error, EditorGUIUtility.currentViewWidth, MessageType.Error);

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => HelpBox.Draw(position, _error, MessageType.Error);
    }
}

using System;
using System.Collections.Generic;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.SaintsInterfacePropertyDrawer
{
    public partial class SaintsInterfaceDrawer
    {

        private IWrapProp _imGuiPropInfo;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
            if (_imGuiPropInfo == null)
            {
                _imGuiPropInfo = GetSerName(property, fieldInfo).propInfo;
            }

            SerializedProperty valueProp =
                property.FindPropertyRelative(ReflectUtils.GetIWrapPropName(_imGuiPropInfo.GetType())) ??
                SerializedUtils.FindPropertyByAutoPropertyName(property,
                    ReflectUtils.GetIWrapPropName(_imGuiPropInfo.GetType()));
            return EditorGUI.GetPropertyHeight(valueProp, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (new EditorGUI.PropertyScope(position, label, property))
            {
                // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
                if (_imGuiPropInfo == null)
                {
                    _imGuiPropInfo = GetSerName(property, fieldInfo).propInfo;
                }

                SerializedProperty valueProp =
                    property.FindPropertyRelative(ReflectUtils.GetIWrapPropName(_imGuiPropInfo.GetType())) ??
                    SerializedUtils.FindPropertyByAutoPropertyName(property,
                        ReflectUtils.GetIWrapPropName(_imGuiPropInfo.GetType()));

                Type interfaceContainer = ReflectUtils.GetElementType(fieldInfo.FieldType);
                Type mostBaseType = ReflectUtils.GetMostBaseType(interfaceContainer);
                Debug.Assert(mostBaseType != null, interfaceContainer);
                Debug.Assert(mostBaseType.IsGenericType, $"{interfaceContainer}->{mostBaseType} is not generic type");
                IReadOnlyList<Type> genericArguments = mostBaseType.GetGenericArguments();
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_INTERFACE
                Debug.Log($"from {interfaceContainer} get types: {string.Join(",", genericArguments)}");
#endif

                Type valueType = genericArguments[0];
                Type interfaceType = genericArguments[1];

                const float buttonWidth = 21;
                (Rect fieldRect, Rect buttonRect) = RectUtils.SplitWidthRect(position, position.width - buttonWidth);

                if (GUI.Button(buttonRect, "●"))
                {
                    FieldInterfaceSelectWindow.Open(valueProp.objectReferenceValue, valueType, interfaceType,
                        fieldResult =>
                        {
                            if(valueProp.objectReferenceValue != fieldResult)
                            {
                                valueProp.objectReferenceValue = fieldResult;
                                valueProp.serializedObject.ApplyModifiedProperties();
                            }
                        });
                }

                // ReSharper disable once ConvertToUsingDeclaration
                using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                {
                    Object oldValue = valueProp.objectReferenceValue;
                    EditorGUI.PropertyField(fieldRect, valueProp, label, true);
                    // ReSharper disable once InvertIf
                    if (changed.changed)
                    {
                        Object newValue = valueProp.objectReferenceValue;
                        // if (newValue != null)
                        // {
                        //     (bool match, Object result) = GetSerializedObject(newValue, valueType, interfaceType);
                        //     // Debug.Log($"newValue={newValue}, match={match}, result={result}");
                        //     valueProp.objectReferenceValue = match
                        //         ? result
                        //         : oldValue;
                        // }

                        bool match = interfaceType.IsInstanceOfType(newValue);

                        if (!match)
                        {
                            (bool findMatch, Object findResult) = GetSerializedObject(newValue,
                                valueType, interfaceType);
                            valueProp.objectReferenceValue = findMatch
                                ? findResult
                                : oldValue;

                            valueProp.serializedObject.ApplyModifiedProperties();
                        }
                    }
                }
            }
        }

    }
}

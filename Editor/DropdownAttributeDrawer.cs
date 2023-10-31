using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ExtInspector.DropdownBase;
using ExtInspector.Editor.Standalone;
using ExtInspector.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace ExtInspector.Editor
{
    [CustomPropertyDrawer(typeof(DropdownAttribute))]
    public class DropdownAttributeDrawer: SaintsPropertyDrawer
    {
        private string _error = "";

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, bool hasLabel)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            DropdownAttribute dropdownAttribute = (DropdownAttribute) saintsAttribute;

            string funcName = dropdownAttribute.FuncName;
            UnityEngine.Object target = property.serializedObject.targetObject;
            Type targetType = target.GetType();
            (ReflectUil.GetPropType getPropType, object fieldOrMethodInfo) = ReflectUil.GetProp(targetType, funcName);
            IDropdownList dropdownListValue;
            switch (getPropType)
            {
                case ReflectUil.GetPropType.NotFound:
                {
                    _error = $"not found `{funcName}` on target `{target}`";
                    DefaultDrawer(position, property);
                }
                    return;
                case ReflectUil.GetPropType.Property:
                {
                    PropertyInfo foundPropertyInfo = (PropertyInfo)fieldOrMethodInfo;
                    dropdownListValue = foundPropertyInfo.GetValue(target) as IDropdownList;
                    if (dropdownListValue == null)
                    {
                        _error = $"dropdownListValue is null from `{funcName}` on target `{target}`";
                        DefaultDrawer(position, property);
                        return;
                    }
                }
                    break;
                case ReflectUil.GetPropType.Field:
                {
                    FieldInfo foundFieldInfo = (FieldInfo)fieldOrMethodInfo;
                    dropdownListValue = foundFieldInfo.GetValue(target) as IDropdownList;
                    if (dropdownListValue == null)
                    {
                        _error = $"dropdownListValue is null from `{funcName}` on target `{target}`";
                        DefaultDrawer(position, property);
                        return;
                    }
                }
                    break;
                case ReflectUil.GetPropType.Method:
                {
                    MethodInfo methodInfo = (MethodInfo)fieldOrMethodInfo;
                    ParameterInfo[] methodParams = methodInfo.GetParameters();
                    Debug.Assert(methodParams.All(p => p.IsOptional));
                    // Debug.Assert(methodInfo.ReturnType == typeof(string));
                    // if (methodInfo.ReturnType != typeof(string))
                    // {
                    //     _error =
                    //         $"Return type of callback method `{decButtonAttribute.ButtonLabel}` should be string";
                    //     return decButtonAttribute.ButtonLabel;
                    // }

                    _error = "";
                    try
                    {
                        dropdownListValue = methodInfo.Invoke(target, methodParams.Select(p => p.DefaultValue).ToArray()) as IDropdownList;
                    }
                    catch (TargetInvocationException e)
                    {
                        _error = e.InnerException!.Message;
                        Debug.LogException(e);
                        return;
                    }
                    catch (Exception e)
                    {
                        _error = e.Message;
                        Debug.LogException(e);
                        return;
                    }

                    if (dropdownListValue == null)
                    {
                        _error = $"dropdownListValue is null from `{funcName}()` on target `{target}`";
                        DefaultDrawer(position, property);
                        return;
                    }
                }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(getPropType), getPropType, null);
            }

            int selectedIndex = -1;
            // Debug.Log(property.propertyPath);

            const BindingFlags bindAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
                                              BindingFlags.Public | BindingFlags.DeclaredOnly;
            FieldInfo field = targetType.GetField(property.propertyPath, bindAttr);
            Debug.Assert(field != null, $"{property.propertyPath}/{target}");
            object curValue = field!.GetValue(target);
            List<string> options = new List<string>();
            List<object> values = new List<object>();

            foreach ((KeyValuePair<string, object> keyValuePair, int index) in dropdownListValue!.Select((keyValuePair, index) => (keyValuePair, index)))
            {
                // Debug.Log($"{keyValuePair.Key} -> {keyValuePair.Value}");
                // bool bothNull = curValue == null && keyValuePair.Value == null;

                // Debug.Log(keyValuePair.Value);
                // Debug.Log(curValue);

                // ReSharper disable once ConvertIfStatementToSwitchStatement
                if (curValue == null && keyValuePair.Value == null)
                {
                    selectedIndex = index;
                }
                else if (curValue is UnityEngine.Object curValueObj
                          && curValueObj == keyValuePair.Value as UnityEngine.Object)
                {
                    selectedIndex = index;
                }
                else if (keyValuePair.Value == null)
                {
                    // nothing
                }
                else if (keyValuePair.Value.Equals(curValue))
                {
                    selectedIndex = index;
                }
                options.Add(keyValuePair.Key);
                values.Add(keyValuePair.Value);
            }

            using EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope();
            int newIndex = EditorGUI.Popup(position, selectedIndex, options.ToArray());
            // ReSharper disable once InvertIf
            if (changed.changed)
            {
                Undo.RecordObject(target, "Dropdown");
                object newValue = values[newIndex];
                field.SetValue(target, newValue);
                // try
                // {
                //     field.SetValue(target, newValue);
                // }
                // catch (ArgumentException)
                // {
                //     property.objectReferenceValue = (UnityEngine.GameObject)newValue;
                // }
            }
        }

        protected override bool WillDrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute) => _error == "" ? 0 : HelpBox.GetHeight(_error, width);

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => _error == "" ? position : HelpBox.Draw(position, _error, MessageType.Error);
    }
}

using System;
using System.Linq;
using System.Reflection;
using SaintsField.DropdownBase;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(DropdownAttribute))]
    public class DropdownAttributeDrawer: SaintsPropertyDrawer
    {
        private string _error = "";

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, bool hasLabelWidth)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            DropdownAttribute dropdownAttribute = (DropdownAttribute) saintsAttribute;

            string funcName = dropdownAttribute.FuncName;
            UnityEngine.Object target = property.serializedObject.targetObject;
            Type targetType = target.GetType();
            (ReflectUtils.GetPropType getPropType, object fieldOrMethodInfo) = ReflectUtils.GetProp(targetType, funcName);
            IDropdownList dropdownListValue;
            switch (getPropType)
            {
                case ReflectUtils.GetPropType.NotFound:
                {
                    _error = $"not found `{funcName}` on target `{target}`";
                    DefaultDrawer(position, property, label);
                }
                    return;
                case ReflectUtils.GetPropType.Property:
                {
                    PropertyInfo foundPropertyInfo = (PropertyInfo)fieldOrMethodInfo;
                    dropdownListValue = foundPropertyInfo.GetValue(target) as IDropdownList;
                    if (dropdownListValue == null)
                    {
                        _error = $"dropdownListValue is null from `{funcName}` on target `{target}`";
                        DefaultDrawer(position, property, label);
                        return;
                    }
                }
                    break;
                case ReflectUtils.GetPropType.Field:
                {
                    FieldInfo foundFieldInfo = (FieldInfo)fieldOrMethodInfo;
                    dropdownListValue = foundFieldInfo.GetValue(target) as IDropdownList;
                    if (dropdownListValue == null)
                    {
                        _error = $"dropdownListValue is null from `{funcName}` on target `{target}`";
                        DefaultDrawer(position, property, label);
                        return;
                    }
                }
                    break;
                case ReflectUtils.GetPropType.Method:
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
                        DefaultDrawer(position, property, label);
                        return;
                    }
                }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(getPropType), getPropType, null);
            }

            // int selectedIndex = -1;
            // Debug.Log(property.propertyPath);

            const BindingFlags bindAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
                                              BindingFlags.Public | BindingFlags.DeclaredOnly;
            FieldInfo field = targetType.GetField(property.propertyPath, bindAttr);
            Debug.Assert(field != null, $"{property.propertyPath}/{target}");
            object curValue = field!.GetValue(target);
            string curDisplay = "";
            foreach (ValueTuple<string, object, bool, bool> itemInfos in dropdownListValue!.Where(each => !each.Item4))
            {
                string name = itemInfos.Item1;
                object itemValue = itemInfos.Item2;

                if (curValue == null && itemValue == null)
                {
                    curDisplay = name;
                    break;
                }
                if (curValue is UnityEngine.Object curValueObj
                          && curValueObj == itemValue as UnityEngine.Object)
                {
                    curDisplay = name;
                    break;
                }
                if (itemValue == null)
                {
                    // nothing
                }
                else if (itemValue.Equals(curValue))
                {
                    curDisplay = name;
                    break;
                }
            }

            // List<string> options = new List<string>();
            // List<object> values = new List<object>();

            // foreach ((ValueTuple<string, object, bool> keyValuePair, int index) in dropdownListValue!.Select((keyValuePair, index) => (keyValuePair, index)))
            // {
            //     string displayName = keyValuePair.Item1;
            //     object value = keyValuePair.Item2;
            //     bool isSeparator = keyValuePair.Item3;
            //
            //     // Debug.Log($"{keyValuePair.Key} -> {keyValuePair.Value}");
            //     // bool bothNull = curValue == null && keyValuePair.Value == null;
            //
            //     // Debug.Log(keyValuePair.Value);
            //     // Debug.Log(curValue);
            //
            //     // ReSharper disable once ConvertIfStatementToSwitchStatement
            //     if (curValue == null && keyValuePair.Value == null)
            //     {
            //         selectedIndex = index;
            //     }
            //     else if (curValue is UnityEngine.Object curValueObj
            //               && curValueObj == keyValuePair.Value as UnityEngine.Object)
            //     {
            //         selectedIndex = index;
            //     }
            //     else if (keyValuePair.Value == null)
            //     {
            //         // nothing
            //     }
            //     else if (keyValuePair.Value.Equals(curValue))
            //     {
            //         selectedIndex = index;
            //     }
            //     options.Add(keyValuePair.Key);
            //     values.Add(keyValuePair.Value);
            // }

            // using EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope();

            bool hasLabel = label.text != "";
            float labelWidth = hasLabel? EditorGUIUtility.labelWidth: 0;
            Rect labelRect = new Rect(position)
            {
                width = labelWidth,
            };
            // (Rect labelRect, Rect fieldRect) = RectUtils.SplitWidthRect(position, labelWidth);
            //
            // EditorGUI.LabelField(labelRect, label);

            Rect fieldRect = EditorGUI.PrefixLabel(position, label);

            // int newIndex = EditorGUI.Popup(position, label, selectedIndex, options.Select(each => new GUIContent(each)).ToArray());
            GUI.SetNextControlName(FieldControlName);
            // string display = selectedIndex == -1 ? "" : options[selectedIndex];
            if (EditorGUI.DropdownButton(fieldRect, new GUIContent(curDisplay), FocusType.Keyboard))
            {
                // create the menu and add items to it
                GenericMenu menu = new GenericMenu();

                foreach (ValueTuple<string, object, bool, bool> itemInfo in dropdownListValue!)
                {
                    string curName = itemInfo.Item1;
                    object curItem = itemInfo.Item2;
                    bool disabled = itemInfo.Item3;
                    bool curIsSeparator = itemInfo.Item4;
                    if (curIsSeparator)
                    {
                        menu.AddSeparator(curName);
                    }
                    else if (disabled)
                    {
                        // Debug.Log($"disabled: {curName}");
                        menu.AddDisabledItem(new GUIContent(curName), curName == curDisplay);
                    }
                    else
                    {
                        menu.AddItem(new GUIContent(curName), curName == curDisplay, () =>
                        {
                            // selectedIndex = options.IndexOf(option);
                            Undo.RecordObject(target, "Dropdown");
                            // object newValue = curItem;
                            field.SetValue(target, curItem);
                        });
                    }
                }

                // for (int index = 0; index < options.Count; index++)
                // {
                //     int curIndex = index;
                //     string option = options[curIndex];
                //     menu.AddItem(new GUIContent(option), curIndex == selectedIndex, () =>
                //     {
                //         // selectedIndex = options.IndexOf(option);
                //         Undo.RecordObject(target, "Dropdown");
                //         object newValue = values[curIndex];
                //         field.SetValue(target, newValue);
                //     });
                // }

                // display the menu
                // menu.ShowAsContext();
                menu.DropDown(fieldRect);
            }

            if(hasLabel)
            {
                ClickFocus(labelRect, FieldControlName);
            }

            // int newIndex = selectedIndex;
            // // ReSharper disable once InvertIf
            // if (changed.changed)
            // {
            //     Undo.RecordObject(target, "Dropdown");
            //     object newValue = values[newIndex];
            //     field.SetValue(target, newValue);
            // }
        }

        protected override bool WillDrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute) => _error == "" ? 0 : HelpBox.GetHeight(_error, width, MessageType.Error);

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => _error == "" ? position : HelpBox.Draw(position, _error, MessageType.Error);
    }
}

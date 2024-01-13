using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.DropdownBase;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(DropdownAttribute))]
    public class DropdownAttributeDrawer: SaintsPropertyDrawer
    {
        private string _error = "";
        private const BindingFlags BindAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
                                      BindingFlags.Public | BindingFlags.DeclaredOnly;

        #region IMGUI

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, bool hasLabelWidth)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, object parent)
        {
            DropdownAttribute dropdownAttribute = (DropdownAttribute) saintsAttribute;
            // Object target = property.serializedObject.targetObject;
            Type parentType = parent.GetType();
            FieldInfo field = parentType.GetField(property.name, BindAttr);
            MetaInfo metaInfo = GetMetaInfo(property, dropdownAttribute, field, parent);

            if (metaInfo.Error != "")
            {
                _error = metaInfo.Error;
                DefaultDrawer(position, property, label);
                return;
            }

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
            string curDisplay = metaInfo.SelectedIndex == -1 ? "-" : metaInfo.DropdownListValue[metaInfo.SelectedIndex].Item1;
            if (EditorGUI.DropdownButton(fieldRect, new GUIContent(curDisplay), FocusType.Keyboard))
            {
                // create the menu and add items to it
                GenericMenu menu = new GenericMenu();

                Debug.Assert(metaInfo.DropdownListValue != null);
                foreach ((string curName, object curItem, bool disabled, bool curIsSeparator) in metaInfo.DropdownListValue)
                {
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
                            Util.SetValue(property, curItem, parent, parentType, field);
                            SetValueChanged(property);
                        });
                    }
                }

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

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute) => _error == "" ? 0 : ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => _error == "" ? position : ImGuiHelpBox.Draw(position, _error, MessageType.Error);

        #endregion

        private struct MetaInfo
        {
            public string Error;
            public IReadOnlyList<ValueTuple<string, object, bool, bool>> DropdownListValue;
            public int SelectedIndex;
        }

        private static MetaInfo GetMetaInfo(SerializedProperty property, ISaintsAttribute saintsAttribute,
            FieldInfo field,
            object parentObj)
        {
            DropdownAttribute dropdownAttribute = (DropdownAttribute) saintsAttribute;
            IDropdownList dropdownListValue;

            string funcName = dropdownAttribute.FuncName;
            // object parentObj = GetParentTarget(property);
            Debug.Assert(parentObj != null);
            Type parentType = parentObj.GetType();
            (ReflectUtils.GetPropType getPropType, object fieldOrMethodInfo) =
                ReflectUtils.GetProp(parentType, funcName);
            switch (getPropType)
            {
                case ReflectUtils.GetPropType.NotFound:
                    return new MetaInfo
                    {
                        Error = $"not found `{funcName}` on target `{parentObj}`",
                        SelectedIndex = -1,
                        DropdownListValue = Array.Empty<ValueTuple<string, object, bool, bool>>(),
                    };
                case ReflectUtils.GetPropType.Property:
                {
                    PropertyInfo foundPropertyInfo = (PropertyInfo)fieldOrMethodInfo;
                    dropdownListValue = foundPropertyInfo.GetValue(parentObj) as IDropdownList;
                    if (dropdownListValue == null)
                    {
                        return new MetaInfo
                        {
                            Error = $"dropdownListValue is null from `{funcName}` on target `{parentObj}`",
                            SelectedIndex = -1,
                            DropdownListValue = Array.Empty<ValueTuple<string, object, bool, bool>>(),
                        };
                    }
                }
                    break;
                case ReflectUtils.GetPropType.Field:
                {
                    FieldInfo foundFieldInfo = (FieldInfo)fieldOrMethodInfo;
                    dropdownListValue = foundFieldInfo.GetValue(parentObj) as IDropdownList;
                    if (dropdownListValue == null)
                    {
                        return new MetaInfo
                        {
                            Error = $"dropdownListValue is null from `{funcName}` on target `{parentObj}`",
                            SelectedIndex = -1,
                            DropdownListValue = Array.Empty<ValueTuple<string, object, bool, bool>>(),
                        };
                    }
                }
                    break;
                case ReflectUtils.GetPropType.Method:
                {
                    MethodInfo methodInfo = (MethodInfo)fieldOrMethodInfo;
                    ParameterInfo[] methodParams = methodInfo.GetParameters();
                    Debug.Assert(methodParams.All(p => p.IsOptional));

                    try
                    {
                        dropdownListValue =
                            methodInfo.Invoke(parentObj, methodParams.Select(p => p.DefaultValue).ToArray()) as
                                IDropdownList;
                    }
                    catch (TargetInvocationException e)
                    {
                        Debug.LogException(e);
                        Debug.Assert(e.InnerException != null);
                        return new MetaInfo
                        {
                            Error = e.InnerException.Message,
                            SelectedIndex = -1,
                            DropdownListValue = Array.Empty<ValueTuple<string, object, bool, bool>>(),
                        };
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        return new MetaInfo
                        {
                            Error = e.Message,
                            SelectedIndex = -1,
                            DropdownListValue = Array.Empty<ValueTuple<string, object, bool, bool>>(),
                        };
                    }

                    if (dropdownListValue == null)
                    {
                        return new MetaInfo
                        {
                            Error = $"dropdownListValue is null from `{funcName}()` on target `{parentObj}`",
                            SelectedIndex = -1,
                            DropdownListValue = Array.Empty<ValueTuple<string, object, bool, bool>>(),
                        };
                    }
                }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(getPropType), getPropType, null);
            }

            Debug.Assert(field != null, $"{property.name}/{parentObj}");
            object curValue = field.GetValue(parentObj);
            Debug.Log($"get cur value {curValue}, {parentObj}->{field}");
            // string curDisplay = "";
            int selectedIndex = -1;
            Debug.Assert(dropdownListValue != null);

            IReadOnlyList<(string, object, bool, bool)> dropdownActualList = dropdownListValue.ToArray();

            foreach (int dropdownIndex in Enumerable.Range(0, dropdownActualList.Count))
            {
                (string _, object itemValue, bool _, bool isSeparator) = dropdownActualList[dropdownIndex];
                if (isSeparator)
                {
                    continue;
                }

                if (curValue == null && itemValue == null)
                {
                    // Debug.Log($"both null selected index = {dropdownIndex}");
                    selectedIndex = dropdownIndex;
                    break;
                }
                if (curValue is Object curValueObj
                    && curValueObj == itemValue as Object)
                {
                    // Debug.Log($"Object equal selected index = {dropdownIndex}");
                    selectedIndex = dropdownIndex;
                    break;
                }
                if (itemValue == null)
                {
                    // nothing
                }
                else if (itemValue.Equals(curValue))
                {
                    // Debug.Log($"Equal selected index = {dropdownIndex}");
                    selectedIndex = dropdownIndex;
                    break;
                }
            }

            return new MetaInfo
            {
                Error = "",
                DropdownListValue = dropdownActualList,
                SelectedIndex = selectedIndex,
            };
        }

        #region UIToolkit

        private static string NameButtonField(SerializedProperty property) => $"{property.propertyPath}__Dropdown_Button";
        private static string NameButtonLabelField(SerializedProperty property) => $"{property.propertyPath}__Dropdown_ButtonLabel";
        private static string NameHelpBox(SerializedProperty property) => $"{property.propertyPath}__Dropdown_HelpBox";
        private static string NameLabel(SerializedProperty property) => $"{property.propertyPath}__Dropdown_Label";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, VisualElement container, Label fakeLabel, object parent)
        {
            DropdownAttribute dropdownAttribute = (DropdownAttribute) saintsAttribute;
            Type parentType = parent.GetType();
            FieldInfo field = parentType.GetField(property.name, BindAttr);
            MetaInfo metaInfo = GetMetaInfo(property, dropdownAttribute, field, parent);

            string buttonLabel = metaInfo.SelectedIndex == -1? "-": metaInfo.DropdownListValue[metaInfo.SelectedIndex].Item1;

            Button button = new Button
            {
                style =
                {
                    height = EditorGUIUtility.singleLineHeight,
                    flexGrow = 1,
                },
                name = NameButtonField(property),
            };

            VisualElement buttonLabelContainer = new VisualElement
            {
                style =
                {
                    width = Length.Percent(100),
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    justifyContent = Justify.SpaceBetween,
                },
            };

            buttonLabelContainer.Add(new Label(buttonLabel)
            {
                name = NameButtonLabelField(property),
                userData = metaInfo.SelectedIndex,
            });
            buttonLabelContainer.Add(new Label("▼"));

            VisualElement root = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                },
            };

            button.Add(buttonLabelContainer);

            Label label = Util.PrefixLabelUIToolKit(new string(' ', property.displayName.Length), 1);
            label.name = NameLabel(property);
            root.Add(label);
            root.Add(button);

            return root;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container, object parent)
        {
            return new HelpBox("", HelpBoxMessageType.Error)
            {
                style =
                {
                    display = DisplayStyle.None,
                },
                name = NameHelpBox(property),
            };
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index, VisualElement container,
            Action<object> onValueChangedCallback, object parent)
        {
            container.Q<Button>(NameButtonField(property)).clicked += () =>
                ShowDropdown(property, saintsAttribute, container, parent, onValueChangedCallback);
        }

        protected override void ChangeFieldLabelToUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, string labelOrNull)
        {
            Label label = container.Q<Label>(NameLabel(property));
            label.text = labelOrNull ?? "";
            label.style.display = labelOrNull == null ? DisplayStyle.None : DisplayStyle.Flex;
        }

        private static void ShowDropdown(SerializedProperty property, ISaintsAttribute saintsAttribute,
            VisualElement container, object parent, Action<object> onChange)
        {
            DropdownAttribute dropdownAttribute = (DropdownAttribute) saintsAttribute;
            Type parentType = parent.GetType();
            FieldInfo field = parentType.GetField(property.name, BindAttr);
            MetaInfo metaInfo = GetMetaInfo(property, dropdownAttribute, field, parent);

            HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property));
            helpBox.style.display = metaInfo.Error == "" ? DisplayStyle.None : DisplayStyle.Flex;
            helpBox.text = metaInfo.Error;

            if (metaInfo.Error != "")
            {
                return;
            }

            // Button button = container.Q<Button>(NameButtonField(property));
            Label buttonLabel = container.Q<Label>(NameButtonLabelField(property));
            GenericDropdownMenu genericDropdownMenu = new GenericDropdownMenu();
            // this is a bug that can not get correct index
            int selectedIndex = (int)buttonLabel.userData;
            // Debug.Log($"metaInfo.SelectedIndex={metaInfo.SelectedIndex}");
            foreach (int index in Enumerable.Range(0, metaInfo.DropdownListValue.Count))
            {
                int curIndex = index;
                (string curName, object curItem, bool disabled, bool curIsSeparator) =
                    metaInfo.DropdownListValue[index];
                if (curIsSeparator)
                {
                    genericDropdownMenu.AddSeparator(curName);
                }
                else if (disabled)
                {
                    // Debug.Log($"disabled: {curName}");
                    genericDropdownMenu.AddDisabledItem(curName, index == selectedIndex);
                }
                else
                {
                    genericDropdownMenu.AddItem(curName, index == selectedIndex, () =>
                    {
                        Util.SetValue(property, curItem, parent, parentType, field);
                        onChange(curItem);
                        buttonLabel.text = curName;
                        buttonLabel.userData = curIndex;
                        // property.serializedObject.ApplyModifiedProperties();
                    });
                }
            }

            Button button = container.Q<Button>(NameButtonField(property));
            genericDropdownMenu.DropDown(button.worldBound, button, true);
        }

        #endregion
    }
}

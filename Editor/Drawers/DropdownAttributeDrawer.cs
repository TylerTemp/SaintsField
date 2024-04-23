using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.DropdownBase;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
#if UNITY_2021_3_OR_NEWER
using UnityEngine.UIElements;
#endif
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(DropdownAttribute))]
    public class DropdownAttributeDrawer: SaintsPropertyDrawer
    {
        private string _error = "";
        private const BindingFlags BindAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
                                      BindingFlags.Public | BindingFlags.DeclaredOnly;

        private static void ShowGenericMenu(MetaInfo metaInfo, string curDisplay, Rect fieldRect, Action<string, object> onSelect, bool hackSlashNoSub)
        {
            // create the menu and add items to it
            GenericMenu menu = new GenericMenu();

            Debug.Assert(metaInfo.DropdownListValue != null);
            foreach ((string curName, object curItem, bool disabled, bool curIsSeparator) in metaInfo.DropdownListValue)
            {
                string replacedCurName = curName.Replace('/', '\u2215');
                if (curIsSeparator)
                {
                    menu.AddSeparator(hackSlashNoSub? "": curName);
                }
                else if (disabled)
                {
                    // Debug.Log($"disabled: {curName}");
                    menu.AddDisabledItem(new GUIContent(hackSlashNoSub? replacedCurName: curName), curName == curDisplay);
                }
                else
                {
                    menu.AddItem(new GUIContent(hackSlashNoSub? replacedCurName: curName), curName == curDisplay, () => onSelect(curName, curItem));
                }
            }

            // display the menu
            // menu.ShowAsContext();
            menu.DropDown(fieldRect);
        }

        #region IMGUI

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, FieldInfo info, bool hasLabelWidth, object parent)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            DropdownAttribute dropdownAttribute = (DropdownAttribute) saintsAttribute;
            // Object target = property.serializedObject.targetObject;
            // Type parentType = parent.GetType();
            MetaInfo metaInfo = GetMetaInfo(property, dropdownAttribute, info, parent);

            if (metaInfo.Error != "")
            {
                _error = metaInfo.Error;
                DefaultDrawer(position, property, label, info);
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
                ShowGenericMenu(metaInfo, curDisplay, fieldRect, (_, item) =>
                {
                    Util.SignFieldValue(property.serializedObject.targetObject, item, parent, info);
                    Util.SignPropertyValue(property, item);
                    property.serializedObject.ApplyModifiedProperties();
                    onGUIPayload.SetValue(item);

                }, !dropdownAttribute.SlashAsSub);
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

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            FieldInfo info,
            object parent) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent) => _error == "" ? 0 : ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent) => _error == "" ? position : ImGuiHelpBox.Draw(position, _error, MessageType.Error);

        #endregion

        private struct MetaInfo
        {
            // ReSharper disable InconsistentNaming
            public string Error;
            public IReadOnlyList<ValueTuple<string, object, bool, bool>> DropdownListValue;
            public int SelectedIndex;
            // ReSharper enable InconsistentNaming

            public override string ToString() =>
                $"MetaInfo(index={SelectedIndex}, items={string.Join(",", DropdownListValue.Select(each => each.Item1))}";
        }

        private static MetaInfo GetMetaInfo(SerializedProperty property, ISaintsAttribute saintsAttribute,
            FieldInfo field,
            object parentObj)
        {
            Debug.Assert(field != null);
            DropdownAttribute dropdownAttribute = (DropdownAttribute) saintsAttribute;

            (string error, IDropdownList dropdownListValue) =
                Util.GetOf<IDropdownList>(dropdownAttribute.FuncName, null, property, field, parentObj);
            if(dropdownListValue == null || error != "")
            {
                return new MetaInfo
                {
                    Error = error == ""? $"dropdownList is null from `{dropdownAttribute.FuncName}` on target `{parentObj}`": error,
                    SelectedIndex = -1,
                    DropdownListValue = Array.Empty<ValueTuple<string, object, bool, bool>>(),
                };
            }

            // IDropdownList dropdownListValue;
            //
            // string funcName = dropdownAttribute.FuncName;
            // Debug.Assert(parentObj != null);
            // Type parentType = parentObj.GetType();
            // (ReflectUtils.GetPropType getPropType, object fieldOrMethodInfo) =
            //     ReflectUtils.GetProp(parentType, funcName);
            // switch (getPropType)
            // {
            //     case ReflectUtils.GetPropType.NotFound:
            //         return new MetaInfo
            //         {
            //             Error = $"not found `{funcName}` on target `{parentObj}`",
            //             SelectedIndex = -1,
            //             DropdownListValue = Array.Empty<ValueTuple<string, object, bool, bool>>(),
            //         };
            //     case ReflectUtils.GetPropType.Property:
            //     {
            //         PropertyInfo foundPropertyInfo = (PropertyInfo)fieldOrMethodInfo;
            //         dropdownListValue = foundPropertyInfo.GetValue(parentObj) as IDropdownList;
            //         if (dropdownListValue == null)
            //         {
            //             return new MetaInfo
            //             {
            //                 Error = $"dropdownListValue is null from `{funcName}` on target `{parentObj}`",
            //                 SelectedIndex = -1,
            //                 DropdownListValue = Array.Empty<ValueTuple<string, object, bool, bool>>(),
            //             };
            //         }
            //     }
            //         break;
            //     case ReflectUtils.GetPropType.Field:
            //     {
            //         FieldInfo foundFieldInfo = (FieldInfo)fieldOrMethodInfo;
            //         dropdownListValue = foundFieldInfo.GetValue(parentObj) as IDropdownList;
            //         if (dropdownListValue == null)
            //         {
            //             return new MetaInfo
            //             {
            //                 Error = $"dropdownListValue is null from `{funcName}` on target `{parentObj}`",
            //                 SelectedIndex = -1,
            //                 DropdownListValue = Array.Empty<ValueTuple<string, object, bool, bool>>(),
            //             };
            //         }
            //     }
            //         break;
            //     case ReflectUtils.GetPropType.Method:
            //     {
            //         MethodInfo methodInfo = (MethodInfo)fieldOrMethodInfo;
            //         ParameterInfo[] methodParams = methodInfo.GetParameters();
            //         Debug.Assert(methodParams.All(p => p.IsOptional));
            //
            //         try
            //         {
            //             dropdownListValue =
            //                 methodInfo.Invoke(parentObj, methodParams.Select(p => p.DefaultValue).ToArray()) as
            //                     IDropdownList;
            //         }
            //         catch (TargetInvocationException e)
            //         {
            //             Debug.LogException(e);
            //             Debug.Assert(e.InnerException != null);
            //             return new MetaInfo
            //             {
            //                 Error = e.InnerException.Message,
            //                 SelectedIndex = -1,
            //                 DropdownListValue = Array.Empty<ValueTuple<string, object, bool, bool>>(),
            //             };
            //         }
            //         catch (Exception e)
            //         {
            //             Debug.LogException(e);
            //             return new MetaInfo
            //             {
            //                 Error = e.Message,
            //                 SelectedIndex = -1,
            //                 DropdownListValue = Array.Empty<ValueTuple<string, object, bool, bool>>(),
            //             };
            //         }
            //
            //         if (dropdownListValue == null)
            //         {
            //             return new MetaInfo
            //             {
            //                 Error = $"dropdownListValue is null from `{funcName}()` on target `{parentObj}`",
            //                 SelectedIndex = -1,
            //                 DropdownListValue = Array.Empty<ValueTuple<string, object, bool, bool>>(),
            //             };
            //         }
            //     }
            //         break;
            //     default:
            //         throw new ArgumentOutOfRangeException(nameof(getPropType), getPropType, null);
            // }

            Debug.Assert(field != null, $"{property.name}/{parentObj}");
            object curValue = field.GetValue(parentObj);
            // Debug.Log($"get cur value {curValue}, {parentObj}->{field}");
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

#if UNITY_2021_3_OR_NEWER

        #region UIToolkit

        private static string NameButtonField(SerializedProperty property) => $"{property.propertyPath}__Dropdown_Button";
        private static string NameButtonLabelField(SerializedProperty property) => $"{property.propertyPath}__Dropdown_ButtonLabel";
        private static string NameHelpBox(SerializedProperty property) => $"{property.propertyPath}__Dropdown_HelpBox";
        private static string NameLabel(SerializedProperty property) => $"{property.propertyPath}__Dropdown_Label";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, VisualElement container, FieldInfo info, object parent)
        {
            DropdownAttribute dropdownAttribute = (DropdownAttribute) saintsAttribute;
            MetaInfo metaInfo = GetMetaInfo(property, dropdownAttribute, info, parent);

            string buttonLabel = metaInfo.SelectedIndex == -1? "-": metaInfo.DropdownListValue[metaInfo.SelectedIndex].Item1;

            UIToolkitUtils.DropdownButtonUIToolkit dropdownButton = UIToolkitUtils.MakeDropdownButtonUIToolkit();
            dropdownButton.Button.style.flexGrow = 1;
            dropdownButton.Button.name = NameButtonField(property);
            dropdownButton.Button.userData = metaInfo.SelectedIndex == -1
                ? null
                : metaInfo.DropdownListValue[metaInfo.SelectedIndex].Item2;
            dropdownButton.Label.text = buttonLabel;
            dropdownButton.Label.name = NameButtonLabelField(property);

            VisualElement root = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                },
            };

            Label label = Util.PrefixLabelUIToolKit(property.displayName, 0);
            label.name = NameLabel(property);
            label.AddToClassList("unity-label");
            root.Add(label);
            root.Add(dropdownButton.Button);

            root.AddToClassList(ClassAllowDisable);

            return root;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
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

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            DropdownAttribute dropdownAttribute = (DropdownAttribute)saintsAttribute;
            container.Q<Button>(NameButtonField(property)).clicked += () =>
                ShowDropdown(property, saintsAttribute, container, dropdownAttribute.SlashAsSub, info, parent, onValueChangedCallback);
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChanged, FieldInfo info, object parent)
        {
            Type parentType = parent.GetType();
            FieldInfo field = parentType.GetField(property.name, BindAttr);
            if (field == null)
            {
                return;
            }

            object newValue = field.GetValue(parent);
            object curValue = container.Q<Button>(NameButtonField(property)).userData;
            if (Util.GetIsEqual(curValue, newValue))
            {
                return;
            }

            MetaInfo metaInfo = GetMetaInfo(property, saintsAttribute, field, parent);
            string display = metaInfo.SelectedIndex == -1 ? "-" : metaInfo.DropdownListValue[metaInfo.SelectedIndex].Item1;
            container.Q<Label>(NameButtonLabelField(property)).text = display;
        }

        private static void ShowDropdown(SerializedProperty property, ISaintsAttribute saintsAttribute,
            VisualElement container, bool slashAsSub, FieldInfo info, object parent, Action<object> onChange)
        {
            DropdownAttribute dropdownAttribute = (DropdownAttribute) saintsAttribute;
            MetaInfo metaInfo = GetMetaInfo(property, dropdownAttribute, info, parent);

            HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property));
            if(helpBox.text != metaInfo.Error)
            {
                helpBox.style.display = metaInfo.Error == "" ? DisplayStyle.None : DisplayStyle.Flex;
                helpBox.text = metaInfo.Error;
            }

            if (metaInfo.Error != "")
            {
                return;
            }

            // Button button = container.Q<Button>(NameButtonField(property));
            Label buttonLabel = container.Q<Label>(NameButtonLabelField(property));
            Button button = container.Q<Button>(NameButtonField(property));

            if (slashAsSub)
            {
                string curDisplay = metaInfo.SelectedIndex == -1 ? "-" : metaInfo.DropdownListValue[metaInfo.SelectedIndex].Item1;
                ShowGenericMenu(metaInfo, curDisplay, button.worldBound, (newName, item) =>
                {
                    Util.SignFieldValue(property.serializedObject.targetObject, item, parent, info);
                    Util.SignPropertyValue(property, item);
                    property.serializedObject.ApplyModifiedProperties();
                    onChange(item);
                    buttonLabel.text = newName;
                    // property.serializedObject.ApplyModifiedProperties();
                }, false);
            }
            else
            {
                GenericDropdownMenu genericDropdownMenu = new GenericDropdownMenu();
                // int selectedIndex = (int)buttonLabel.userData;
                // FIXED: can not get correct index
                int selectedIndex = metaInfo.SelectedIndex;
                // Debug.Log($"metaInfo.SelectedIndex={metaInfo.SelectedIndex}");
                foreach (int index in Enumerable.Range(0, metaInfo.DropdownListValue.Count))
                {
                    // int curIndex = index;
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
                            Util.SignFieldValue(property.serializedObject.targetObject, curItem, parent, info);
                            Util.SignPropertyValue(property, curItem);
                            property.serializedObject.ApplyModifiedProperties();
                            onChange(curItem);
                            buttonLabel.text = curName;
                            // property.serializedObject.ApplyModifiedProperties();
                        });
                    }
                }

                genericDropdownMenu.DropDown(button.worldBound, button, true);
            }
        }

        #endregion

#endif
    }
}

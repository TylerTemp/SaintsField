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
                    Util.SignPropertyValue(property, info, parent, item);
                    property.serializedObject.ApplyModifiedProperties();
                    onGUIPayload.SetValue(item);
                    if(ExpandableIMGUIScoop.IsInScoop)
                    {
                        property.serializedObject.ApplyModifiedProperties();
                    }

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

            Debug.Assert(field != null, $"{property.name}/{parentObj}");
            object curValue = field.GetValue(parentObj);
            if (curValue is IWrapProp wrapProp)
            {
                curValue = Util.GetWrapValue(wrapProp);
            }
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

                if (Util.GetIsEqual(curValue, itemValue))
                {
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

        private static string NameDropdownButtonField(SerializedProperty property) => $"{property.propertyPath}__Dropdown_Button";
        private static string NameHelpBox(SerializedProperty property) => $"{property.propertyPath}__Dropdown_HelpBox";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, VisualElement container, FieldInfo info, object parent)
        {
            DropdownAttribute dropdownAttribute = (DropdownAttribute) saintsAttribute;
            MetaInfo metaInfo = GetMetaInfo(property, dropdownAttribute, info, parent);

            string buttonLabel = metaInfo.SelectedIndex == -1? "-": metaInfo.DropdownListValue[metaInfo.SelectedIndex].Item1;

            UIToolkitUtils.DropdownButtonField dropdownButton = UIToolkitUtils.MakeDropdownButtonUIToolkit(property.displayName);
            dropdownButton.style.flexGrow = 1;
            dropdownButton.buttonLabelElement.text = buttonLabel;
            dropdownButton.name = NameDropdownButtonField(property);
            dropdownButton.userData = metaInfo.SelectedIndex == -1
                ? null
                : metaInfo.DropdownListValue[metaInfo.SelectedIndex].Item2;

            dropdownButton.AddToClassList(ClassAllowDisable);

            return dropdownButton;
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
            container.Q<UIToolkitUtils.DropdownButtonField>(NameDropdownButtonField(property)).buttonElement.clicked += () =>
                ShowDropdown(property, saintsAttribute, container, dropdownAttribute.SlashAsSub, info, parent, onValueChangedCallback);
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChanged, FieldInfo info)
        {
            object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
            if (parent == null)
            {
                Debug.LogWarning($"{property.propertyPath} parent disposed unexpectly");
                return;
            }

            Type parentType = parent.GetType();
            FieldInfo field = parentType.GetField(property.name, BindAttr);
            if (field == null)
            {
                return;
            }

            object newValue = field.GetValue(parent);
            object curValue = container.Q<UIToolkitUtils.DropdownButtonField>(NameDropdownButtonField(property)).userData;
            if (Util.GetIsEqual(curValue, newValue))
            {
                return;
            }

            MetaInfo metaInfo = GetMetaInfo(property, saintsAttribute, field, parent);
            string display = metaInfo.SelectedIndex == -1 ? "-" : metaInfo.DropdownListValue[metaInfo.SelectedIndex].Item1;
            // Debug.Log($"change label to {display}");
            Label buttonLabelElement = container.Q<UIToolkitUtils.DropdownButtonField>(NameDropdownButtonField(property)).buttonLabelElement;
            if(buttonLabelElement.text != display)
            {
                buttonLabelElement.text = display;
            }
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
            UIToolkitUtils.DropdownButtonField dropdownButtonField = container.Q<UIToolkitUtils.DropdownButtonField>(NameDropdownButtonField(property));

            if (slashAsSub)
            {
                string curDisplay = metaInfo.SelectedIndex == -1 ? "-" : metaInfo.DropdownListValue[metaInfo.SelectedIndex].Item1;
                ShowGenericMenu(metaInfo, curDisplay, dropdownButtonField.buttonElement.worldBound, (newName, item) =>
                {
                    Util.SignFieldValue(property.serializedObject.targetObject, item, parent, info);
                    Util.SignPropertyValue(property, info, parent, item);
                    property.serializedObject.ApplyModifiedProperties();
                    onChange(item);
                    dropdownButtonField.buttonLabelElement.text = newName;
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
                            Util.SignPropertyValue(property, info, parent, curItem);
                            property.serializedObject.ApplyModifiedProperties();
                            onChange(curItem);
                            dropdownButtonField.buttonLabelElement.text = curName;
                            // property.serializedObject.ApplyModifiedProperties();
                        });
                    }
                }

                genericDropdownMenu.DropDown(dropdownButtonField.buttonElement.worldBound, dropdownButtonField, true);
            }
        }

        #endregion

#endif
    }
}

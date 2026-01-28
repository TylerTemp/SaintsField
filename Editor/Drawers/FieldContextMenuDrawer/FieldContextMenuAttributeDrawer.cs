using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.ButtonDrawers.DecButtonDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.FieldContextMenuDrawer
{
    [CustomPropertyDrawer(typeof(FieldCustomContextMenuAttribute))]
    public class FieldContextMenuAttributeDrawer: SaintsPropertyDrawer
    {
        private static string NamePlaceholder(SerializedProperty property, int index) => $"{property.propertyType}_{index}__ContextMenu";
        private static string ClassPlaceholder(SerializedProperty property) => $"{property.propertyType}__ContextMenu";

        private static string NameHelpBox(SerializedProperty property, int index) => $"{property.propertyType}_{index}__ContextMenu_HelpBox";

        private readonly HashSet<IEnumerator> _enumerators = new HashSet<IEnumerator>();
        private IVisualElementScheduledItem _buttonTask;

        protected override VisualElement CreateAboveUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            VisualElement container, FieldInfo info, object parent)
        {
            VisualElement root = new VisualElement
            {
                name = NamePlaceholder(property, index),
                userData = saintsAttribute,
            };
            root.AddToClassList(ClassPlaceholder(property));
            return root;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            HelpBox helpBox = new HelpBox("", HelpBoxMessageType.Error)
            {
                name = NameHelpBox(property, index),
                style =
                {
                    display = DisplayStyle.None,
                },
            };

            helpBox.AddToClassList(ClassAllowDisable);

            return helpBox;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            FieldCustomContextMenuAttribute fieldCustomContextMenuAttribute = (FieldCustomContextMenuAttribute)saintsAttribute;
            string callback = fieldCustomContextMenuAttribute.FuncName;
            string menuName = string.IsNullOrEmpty(fieldCustomContextMenuAttribute.MenuName) ? ObjectNames.NicifyVariableName(callback) : fieldCustomContextMenuAttribute.MenuName;

            IReadOnlyList<VisualElement> visibilityElements = container.Query<VisualElement>(className: ClassPlaceholder(property)).ToList();
            VisualElement topElement = visibilityElements[0];
            bool isFirst = topElement.name == NamePlaceholder(property, index);

            HelpBox helpBox = container.Q<HelpBox>(name: NameHelpBox(property, index));

            container.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                if (callback == null)
                {
                    evt.menu.AppendSeparator(fieldCustomContextMenuAttribute.MenuName);
                    return;
                }

                if (isFirst)
                {
                    evt.menu.AppendSeparator();
                }

                evt.menu.AppendAction(menuName, _ =>
                {
                    string buttonError = "";
                    // ReSharper disable once PossibleNullReferenceException
                    // ReSharper disable once AccessToModifiedClosure
                    // HashSet<IEnumerator> enumerators = (HashSet<IEnumerator>)buttonElement.userData;
                    foreach ((string eachError, object buttonResult) in DecButtonAttributeDrawer.CallButtonFunc(property, callback, info, parent))
                    {
                        // Debug.Log($"{eachError}/{buttonResult}");
                        if (eachError == "")
                        {
                            // Debug.Log(buttonResult is IEnumerator);
                            if (buttonResult is IEnumerator enumerator)
                            {
                                _enumerators.Add(enumerator);
                            }
                        }
                        else
                        {
                            buttonError += eachError;
                        }
                    }

                    _buttonTask?.Pause();

                    if (_enumerators.Count > 0)
                    {
                        // ReSharper disable once PossibleNullReferenceException
                        // ReSharper disable once AccessToModifiedClosure
                        _buttonTask = container.schedule.Execute(() =>
                        {
                            HashSet<IEnumerator> completedEnumerators = new HashSet<IEnumerator>();

                            foreach (IEnumerator enumerator in _enumerators)
                            {
                                if (!enumerator.MoveNext())
                                {
                                    completedEnumerators.Add(enumerator);
                                }
                            }

                            _enumerators.ExceptWith(completedEnumerators);
                            // bool show = enumerators.Count > 0;

                            // DisplayStyle style = show? DisplayStyle.Flex : DisplayStyle.None;
                            // if(buttonRotator.style.display != style)
                            // {
                            //     buttonRotator.style.display = style;
                            // }
                        }).Every(1);
                    }

                    UIToolkitUtils.SetHelpBox(helpBox, buttonError);
                });
            }));
        }
    }
}

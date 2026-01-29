using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.FieldContextMenuDrawer
{
    [CustomPropertyDrawer(typeof(FieldCustomContextMenuAttribute))]
    public class FieldContextMenuAttributeDrawer: SaintsPropertyDrawer, CustomContextMenuUtils.IManipulatorHandler
    {
        private static string NamePlaceholder(SerializedProperty property, int index) => $"{property.propertyType}_{index}__ContextMenu";
        private static string ClassPlaceholder(SerializedProperty property) => $"{property.propertyType}__ContextMenu";

        private static string NameHelpBox(SerializedProperty property, int index) => $"{property.propertyType}_{index}__ContextMenu_HelpBox";

        private readonly List<IEnumerator> _enumerators = new List<IEnumerator>();
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

        private HelpBox _helpBox;
        private VisualElement _container;
        private SerializedProperty _property;
        private object _parent;

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            _container = container;
            _property = property;
            _parent = parent;

            FieldCustomContextMenuAttribute fieldCustomContextMenuAttribute = (FieldCustomContextMenuAttribute)saintsAttribute;
            string callback = fieldCustomContextMenuAttribute.FuncName;
            string menuName = string.IsNullOrEmpty(fieldCustomContextMenuAttribute.MenuName) ? ObjectNames.NicifyVariableName(callback) : fieldCustomContextMenuAttribute.MenuName;

            IReadOnlyList<VisualElement> placeholderElements = container.Query<VisualElement>(className: ClassPlaceholder(property)).ToList();
            VisualElement topElement = placeholderElements[0];
            bool isFirst = topElement.name == NamePlaceholder(property, index);

            _helpBox = container.Q<HelpBox>(name: NameHelpBox(property, index));

            CustomContextMenuUtils.AddManipulator(container, callback, menuName, fieldCustomContextMenuAttribute.MenuNameIsCallback, isFirst, property, info, this);
        }

        public void SetHelpBox(string error)
        {
            UIToolkitUtils.SetHelpBox(_helpBox, error);
        }

        public void SetIEnumerators(IReadOnlyCollection<IEnumerator> enumerators)
        {
            _buttonTask?.Pause();
            _enumerators.Clear();
            _enumerators.AddRange(enumerators);


            if (_enumerators.Count > 0)
            {
                _buttonTask = _container.schedule.Execute(() =>
                {
                    HashSet<IEnumerator> completedEnumerators = new HashSet<IEnumerator>();

                    // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
                    foreach (IEnumerator enumerator in _enumerators)
                    {
                        if (!enumerator.MoveNext())
                        {
                            completedEnumerators.Add(enumerator);
                        }
                    }

                    _enumerators.RemoveAll(each  => completedEnumerators.Contains(each));
                }).Every(1);
            }
        }

        public object GetParent()
        {
            // ReSharper disable once InvertIf
            if(_parent != null && ReflectUtils.TypeIsStruct(_parent.GetType()))
            {
                (SerializedUtils.FieldOrProp _, object refreshedParent) =
                    SerializedUtils.GetFieldInfoAndDirectParent(_property);
                if (refreshedParent != null)
                {
                    return refreshedParent;
                }
            }
            return _parent;
        }
    }
}

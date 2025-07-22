#if SAINTSFIELD_SERIALIZATION && SAINTSFIELD_SERIALIZATION_ENABLE && UNITY_2022_2_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Events;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.SaintsEventBaseTypeDrawer
{
    public partial class SaintsEventBaseDrawer
    {
        protected override bool UseCreateFieldUIToolKit => true;

        private static string NameSaintsEventView(SerializedProperty property) => $"{property.propertyPath}__SaintsEventBase";
        private static string NameListView(SerializedProperty property) => $"{property.propertyPath}__SaintsEventBase_ListView";

        public class SaintsEventView : VisualElement
        {
            private static VisualTreeAsset _containerTree;

            private readonly VisualElement _container;
            public readonly Label Label;

            public readonly Button AddInstanceButton;
            public readonly Button AddStaticButton;
            public readonly Button RemoveButton;

            public SaintsEventView(string label)
            {
                if (_containerTree == null)
                {
                    _containerTree = Util.LoadResource<VisualTreeAsset>("UIToolkit/SaintsEvent/SaintsEventContainer.uxml");
                }

                TemplateContainer element = _containerTree.CloneTree();

                _container = element.Q<VisualElement>("saints-event-container");
                Label = _container.Q<Label>("saints-event-label");
                AddInstanceButton = _container.Q<Button>("saints-event-add-instance-button");
                AddStaticButton = _container.Q<Button>("saints-event-add-static-button");
                RemoveButton = _container.Q<Button>("saints-event-remove-button");

                SetLabel(label);

                Add(element);
            }

            public void SetLabel(string label)
            {
                if (string.IsNullOrEmpty(label))
                {
                    Label.style.display = DisplayStyle.None;
                }
                else
                {
                    Label.style.display = DisplayStyle.Flex;
                    Label.text = label;
                }
            }

            public void AddListView(ListView listView)
            {
                _container.Insert(_container.IndexOf(Label) + 1, listView);
            }
        }

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, FieldInfo info, object parent)
        {
            return new SaintsEventView(GetPreferredLabel(property))
            {
                name = NameSaintsEventView(property),
            };
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            SaintsEventView saintsEventView = container.Q<SaintsEventView>(NameSaintsEventView(property));
            SerializedProperty persistentCallProp = property.FindPropertyRelative(PropNamePersistentCalls);
            ListView listView = new ListView
            {
                showBoundCollectionSize = false,
                // showAlternatingRowBackgrounds = AlternatingRowBackground.All,
                showFoldoutHeader = false,
                showAddRemoveFooter = false,
                reorderable = true,
                selectionType = SelectionType.Multiple,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                reorderMode = ListViewReorderMode.Animated,

                makeItem = () => new PropertyField(),
                bindItem = (element, i) =>
                {
                    SerializedProperty itemProp = persistentCallProp.GetArrayElementAtIndex(i);
                    ((PropertyField)element).BindProperty(itemProp);
                },
                unbindItem = (element, _) =>
                {
                    PropertyField propField = (PropertyField)element;
                    UIToolkitUtils.Unbind(propField);
                },
            };

            listView.BindProperty(persistentCallProp);
            listView.bindingPath = persistentCallProp.propertyPath;
            listView.Bind(persistentCallProp.serializedObject);

            ScrollView sv = listView.Q<ScrollView>();
            sv.AddToClassList("unity-collection-view--with-border");
            sv.AddToClassList("unity-list-view__scroll-view--with-footer");
            sv.AddToClassList("unity-event__list-view-scroll-view");

            saintsEventView.AddListView(listView);

            saintsEventView.AddInstanceButton.clicked += () => PersistentCallAdd(persistentCallProp, false);
            saintsEventView.AddStaticButton.clicked += () => PersistentCallAdd(persistentCallProp, true);
            saintsEventView.RemoveButton.clicked += () =>
            {
                int[] selected = listView.selectedIndices.OrderByDescending(each => each).ToArray();
                if (selected.Length == 0)
                {
                    selected = new[] { persistentCallProp.arraySize - 1 };
                }

                foreach (int deleteIndex in selected)
                {
                    persistentCallProp.DeleteArrayElementAtIndex(deleteIndex);
                }

                persistentCallProp.serializedObject.ApplyModifiedProperties();
            };
        }

        private static void PersistentCallAdd(SerializedProperty persistentCallProp, bool isStatic)
        {
            int index = persistentCallProp.arraySize;
            persistentCallProp.arraySize = index + 1;
            SerializedProperty persistentCallElement = persistentCallProp.GetArrayElementAtIndex(index);
            persistentCallElement.FindPropertyRelative("_isStatic").boolValue = isStatic;
            persistentCallElement.FindPropertyRelative(nameof(PersistentCall.callState)).intValue =
                (int)UnityEventCallState.RuntimeOnly;
            persistentCallElement.serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif

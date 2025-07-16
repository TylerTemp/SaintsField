#if SAINTSFIELD_SERIALIZATION && SAINTSFIELD_SERIALIZATION_ENABLE
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
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

            public SaintsEventView(string label)
            {
                if (_containerTree == null)
                {
                    _containerTree = Util.LoadResource<VisualTreeAsset>("UIToolkit/SaintsEvent/SaintsEventContainer.uxml");
                }

                TemplateContainer element = _containerTree.CloneTree();

                _container = element.Q<VisualElement>("saints-event-container");
                Label = _container.Q<Label>("saints-event-label");

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
            };

            SerializedProperty persistentCallProp = property.FindPropertyRelative(PropNamePersistentCalls);
            listView.bindingPath = persistentCallProp.propertyPath;
            listView.Bind(property.serializedObject);

            ScrollView sv = listView.Q<ScrollView>();
            sv.AddToClassList("unity-collection-view--with-border");
            sv.AddToClassList("unity-list-view__scroll-view--with-footer");
            sv.AddToClassList("unity-event__list-view-scroll-view");

            saintsEventView.AddListView(listView);
        }
    }
}
#endif

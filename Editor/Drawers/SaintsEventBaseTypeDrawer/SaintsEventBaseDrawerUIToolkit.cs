#if SAINTSFIELD_SERIALIZATION && !SAINTSFIELD_SERIALIZATION_DISABLED && UNITY_2022_2_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
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
        // private static string NameListView(SerializedProperty property) => $"{property.propertyPath}__SaintsEventBase_ListView";

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

            private void SetLabel(string label)
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
            IReadOnlyList<Type> genericTypes = GetTypes(property);
            string rawPreferredLabel = GetPreferredLabel(property);
            string useLabel;
            if (string.IsNullOrEmpty(rawPreferredLabel))
            {
                useLabel = null;
            }
            else if(genericTypes.Count == 0)
            {
                useLabel = rawPreferredLabel;
            }
            else
            {
                useLabel = $"{rawPreferredLabel} ({string.Join(", ", genericTypes.Select(SaintsEventUtils.StringifyType))})";
            }

            return new SaintsEventView(useLabel)
            {
                name = NameSaintsEventView(property),
            };
        }

        private Type[] _cachedEventParamTypes;

        private IReadOnlyList<Type> GetTypes(SerializedProperty property)
        {
            if (_cachedEventParamTypes != null)
            {
                return _cachedEventParamTypes;
            }

            int propIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);
            (SerializedUtils.FieldOrProp rootFieldOrProp, object _) = SerializedUtils.GetFieldInfoAndDirectParent(property);
            Type rawType = rootFieldOrProp.IsField
                ? rootFieldOrProp.FieldInfo.FieldType
                : rootFieldOrProp.PropertyInfo.PropertyType;
            if (propIndex >= 0)
            {
                rawType = ReflectUtils.GetElementType(rawType);
            }
            return _cachedEventParamTypes = rawType.GetGenericArguments();
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            SaintsEventView saintsEventView = container.Q<SaintsEventView>(NameSaintsEventView(property));

            UIToolkitUtils.AddContextualMenuManipulator(saintsEventView.Label, property, () => Util.PropertyChangedCallback(property, info, onValueChangedCallback));

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
                    if (i >= persistentCallProp.arraySize)
                    {
                        return;
                    }
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
                listView.Rebuild();
            };
        }

        private static void PersistentCallAdd(SerializedProperty persistentCallProp, bool isStatic)
        {
            int index = persistentCallProp.arraySize;
            persistentCallProp.arraySize = index + 1;
            SerializedProperty persistentCallElement = persistentCallProp.GetArrayElementAtIndex(index);
            persistentCallElement.FindPropertyRelative(nameof(PersistentCall.isStatic)).boolValue = isStatic;
            persistentCallElement.FindPropertyRelative(nameof(PersistentCall.callState)).intValue =
                (int)UnityEventCallState.RuntimeOnly;
            persistentCallElement.FindPropertyRelative(nameof(PersistentCall.methodName)).stringValue = "";
            persistentCallElement.FindPropertyRelative(nameof(PersistentCall.target)).objectReferenceValue = null;
            persistentCallElement.FindPropertyRelative(nameof(PersistentCall.persistentArguments)).arraySize = 0;
            persistentCallElement.FindPropertyRelative(nameof(PersistentCall.staticType) + SubPropNameTypeNameAndAssmble).stringValue = "";
            persistentCallElement.FindPropertyRelative(nameof(PersistentCall.staticType) + SubPropMonoScriptGuid).stringValue = "";
            persistentCallElement.FindPropertyRelative(nameof(PersistentCall.returnType) + SubPropNameTypeNameAndAssmble).stringValue = "";
            persistentCallElement.FindPropertyRelative(nameof(PersistentCall.returnType) + SubPropMonoScriptGuid).stringValue = "";
            persistentCallElement.serializedObject.ApplyModifiedProperties();
        }

        protected override void ChangeFieldLabelToUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container, string labelOrNull, IReadOnlyList<RichTextDrawer.RichTextChunk> richTextChunks, bool tried,
            RichTextDrawer richTextDrawer)
        {
            SaintsEventView saintsEventView = container.Q<SaintsEventView>(NameSaintsEventView(property));
            IReadOnlyList<RichTextDrawer.RichTextChunk> useChunks = richTextChunks;
            if (useChunks is { Count: > 0 })
            {
                IReadOnlyList<Type> types = GetTypes(property);
                if (types.Count > 0)
                {
                    useChunks = useChunks.Append(new RichTextDrawer.RichTextChunk
                    {
                        Content = $" ({string.Join(", ", types.Select(SaintsEventUtils.StringifyType))})",
                    }).ToList();
                }
            }
            UIToolkitUtils.SetLabel(saintsEventView.Label, useChunks, richTextDrawer);
        }
    }
}
#endif

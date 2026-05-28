#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System;
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.SaintsEventBaseTypeDrawer.UIToolkitElements;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Editor.Utils;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.UIToolkitElements
{
#if UNITY_6000_0_OR_NEWER
    [UxmlElement]
#endif
    public partial class OnEventElement: VisualElement
    {
#if !UNITY_6000_0_OR_NEWER
        public new class UxmlTraits : VisualElement.UxmlTraits { }
        public new class UxmlFactory : UxmlFactory<OnEventElement, UxmlTraits> { }
#endif

        private static VisualTreeAsset _treeRowTemplate;

        // private readonly Label _title;

        private readonly UnityEventCallStateSelector _unityEventCallStateSelector;
        private readonly ObjectField _objectField;
        // private readonly Label _eventName;
        private readonly VisualElement _valueContainer;

        // private string _v
        //
        // public new string viewDataKey
        // {
        //     get =>
        // }

        // ReSharper disable once MemberCanBePrivate.Global
        public OnEventElement() : this("", ""){}

        public OnEventElement(string title, string targetEventName)
        {
            _treeRowTemplate ??= Util.LoadResource<VisualTreeAsset>("UIToolkit/OnEventFunction/OnEvent.uxml");
            TemplateContainer root = _treeRowTemplate.CloneTree();
            hierarchy.Add(root);

            root.Q<Label>("title").text = title;

            _unityEventCallStateSelector = root.Q<UnityEventCallStateSelector>();
            _objectField = root.Q<ObjectField>();
            root.Q<Label>("eventName").text = targetEventName;
            _valueContainer = root.Q<VisualElement>("valueContainer");
        }

        public void Refresh(Object unityEventContainerObject, UnityEventCallState unityEventCallState, bool hasValue, Type valueType, object value)
        {
            _unityEventCallStateSelector.SetValueWithoutNotify((int)unityEventCallState);
            _objectField.SetValueWithoutNotify(unityEventContainerObject);

            if (hasValue)
            {
                (VisualElement result, bool _) = UIToolkitEdit.UIToolkitValueEdit(
                    _valueContainer.Children().FirstOrDefault(),
                    "",
                    valueType,
                    value,
                    null,
                    null,
                    true,
                    false,
                    Array.Empty<Attribute>(),
                    Array.Empty<object>(),
                    new RichTextDrawer.EmptyRichTextTagProvider(),
                    viewDataKey
                );
                if (result != null)
                {
                    _valueContainer.Clear();
                    _valueContainer.Add(result);
                }
            }
        }
    }
}
#endif

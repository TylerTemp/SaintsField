using SaintsField.Editor.Utils;
using UnityEngine.UIElements;

namespace SaintsField.Editor.UIToolkitElements
{

#if UNITY_6000_0_OR_NEWER
    [UxmlElement]
#endif
    public partial class CollectionFoldout: BindableElement, INotifyValueChanged<bool>
    {

#if !UNITY_6000_0_OR_NEWER
        // public new class UxmlTraits : BindableElement.UxmlTraits { }
        public new class UxmlFactory : UxmlFactory<CollectionFoldout, UxmlTraits> { }
#endif
        // ReSharper disable once MemberCanBePrivate.Global
        public CollectionFoldout() : this(null)
        {
        }

        private static VisualTreeAsset _template;
        private readonly Foldout _foldout;
        public readonly Button MenuButton;
        public readonly IntegerField ArraySizeField;

        public CollectionFoldout(string label)
        {
            _template ??= Util.LoadResource<VisualTreeAsset>("UIToolkit/CollectionFoldout/CollectionFoldout.uxml");
            TemplateContainer element = _template.CloneTree();
            hierarchy.Add(element);

            _foldout = element.Q<Foldout>();
            _foldout.RegisterValueChangedCallback(evt =>
            {
                evt.StopPropagation();
                using ChangeEvent<bool> forwardedEvent = ChangeEvent<bool>.GetPooled(evt.previousValue, evt.newValue);
                forwardedEvent.target = this;
                SendEvent(forwardedEvent);
            });
            if (!string.IsNullOrEmpty(label))
            {
                _foldout.text = label;
            }
            contentContainer = _foldout.contentContainer;

            MenuButton = element.Q<Button>(name: "menuButton");
            ArraySizeField = element.Q<IntegerField>(name: "arraySizeField");
        }

        public override VisualElement contentContainer { get; }

        // ReSharper disable once InconsistentNaming
        public new string viewDataKey
        {
            // ReSharper disable once UnusedMember.Global
            get => _foldout.viewDataKey;
            set => _foldout.viewDataKey = value;
        }

        public void SetValueWithoutNotify(bool newValue)
        {
            _foldout.SetValueWithoutNotify(newValue);
        }

        public bool value
        {
            get => _foldout.value;
            set
            {
                if (_foldout.value == value)
                {
                    return;
                }

                bool previous = _foldout.value;
                SetValueWithoutNotify(value);

                using ChangeEvent<bool> evt = ChangeEvent<bool>.GetPooled(previous, value);
                evt.target = this;
                SendEvent(evt);
            }
        }
    }
}

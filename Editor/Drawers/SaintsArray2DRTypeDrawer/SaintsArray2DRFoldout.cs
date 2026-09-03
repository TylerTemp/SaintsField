using SaintsField.Editor.Utils;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.SaintsArray2DRTypeDrawer
{

#if UNITY_6000_0_OR_NEWER
    [UxmlElement]
#endif
    public partial class SaintsArray2DRFoldout: BindableElement, INotifyValueChanged<bool>
    {

#if !UNITY_6000_0_OR_NEWER
        // public new class UxmlTraits : BindableElement.UxmlTraits { }
        public new class UxmlFactory : UxmlFactory<SaintsArray2DRFoldout, UxmlTraits> { }
#endif
        // ReSharper disable once MemberCanBePrivate.Global
        public SaintsArray2DRFoldout() : this(null)
        {
        }

        private static VisualTreeAsset _template;
        private readonly VisualElement _topRightContainer;
        public readonly Foldout Foldout;
        public readonly Button ColReduceButton;
        public readonly Button ColAddButton;
        public readonly Button RowReduceButton;
        public readonly Button RowAddButton;
        public readonly IntegerField ColSizeField;
        public readonly IntegerField RowSizeField;

        public SaintsArray2DRFoldout(string label)
        {
            _template ??= Util.LoadResource<VisualTreeAsset>("UIToolkit/SaintsArray2DR/SaintsArray2DRFoldout.uxml");
            TemplateContainer element = _template.CloneTree();
            hierarchy.Add(element);

            _topRightContainer = element.Q<VisualElement>(name: "topRightContainer");
            Foldout = element.Q<Foldout>();
            Foldout.RegisterValueChangedCallback(evt =>
            {
                evt.StopPropagation();
                using ChangeEvent<bool> forwardedEvent = ChangeEvent<bool>.GetPooled(evt.previousValue, evt.newValue);
                forwardedEvent.target = this;
                SendEvent(forwardedEvent);
            });
            if (!string.IsNullOrEmpty(label))
            {
                Foldout.text = label;
            }
            contentContainer = Foldout.contentContainer;

            ColReduceButton = element.Q<Button>(name: "colReduce");
            ColAddButton = element.Q<Button>(name: "colAdd");
            RowReduceButton = element.Q<Button>(name: "rowReduce");
            RowAddButton = element.Q<Button>(name: "rowAdd");
            ColSizeField = element.Q<IntegerField>(name: "colSizeField");
            RowSizeField = element.Q<IntegerField>(name: "rowSizeField");
        }

        public void SetTranspose(bool transpose)
        {
            _topRightContainer.style.flexDirection = transpose
                ? FlexDirection.RowReverse
                : FlexDirection.Row;
        }

        public override VisualElement contentContainer { get; }

        // ReSharper disable once InconsistentNaming
        public new string viewDataKey
        {
            // ReSharper disable once UnusedMember.Global
            get => Foldout.viewDataKey;
            set => Foldout.viewDataKey = value;
        }

        public void SetValueWithoutNotify(bool newValue)
        {
            Foldout.SetValueWithoutNotify(newValue);
        }

        public bool value
        {
            get => Foldout.value;
            set
            {
                if (Foldout.value == value)
                {
                    return;
                }

                bool previous = Foldout.value;
                SetValueWithoutNotify(value);

                using ChangeEvent<bool> evt = ChangeEvent<bool>.GetPooled(previous, value);
                evt.target = this;
                SendEvent(evt);
            }
        }
    }
}

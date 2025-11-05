#if UNITY_2021_3_OR_NEWER
using SaintsField.Editor.Linq;
using SaintsField.Editor.UIToolkitElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.SortingLayerDrawer
{
    public class SortingLayerStringElement: StringDropdownElement
    {
        private VisualElement _boundTarget;

        public SortingLayerStringElement()
        {
            Button.clicked += () =>
                SortingLayerUtils.MakeDropdown(true, value, _boundTarget ?? this, v => value = (string)v);
        }

        public void BindBound(VisualElement target) => _boundTarget = target;


        public override void SetValueWithoutNotify(string newValue)
        {
            CachedValue = newValue;

            foreach ((SortingLayer layer, int index) in SortingLayer.layers.WithIndex())
            {
                // ReSharper disable once InvertIf
                if (layer.name == newValue)
                {
                    SetLabelString($"{layer.name} <color=#808080>({index})</color>");
                    return;
                }
            }

            SetLabelString($"<color=red>?</color> {(string.IsNullOrEmpty(newValue)? "": $"({newValue})")}");
        }
    }

    public class SortingLayerStringField : BaseField<string>
    {
        private readonly SortingLayerStringElement _sortingLayerStringElement;
        public SortingLayerStringField(string label, SortingLayerStringElement visualInput) : base(label, visualInput)
        {
            _sortingLayerStringElement = visualInput;
            visualInput.BindBound(this);
        }

        public override void SetValueWithoutNotify(string newValue)
        {
            _sortingLayerStringElement.SetValueWithoutNotify(newValue);
        }

        public override string value
        {
            get => _sortingLayerStringElement.value;
            set => _sortingLayerStringElement.value = value;
        }
    }
}
#endif

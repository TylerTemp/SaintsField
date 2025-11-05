#if UNITY_2021_3_OR_NEWER
using SaintsField.Editor.Linq;
using SaintsField.Editor.UIToolkitElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.SortingLayerDrawer
{
    public class SortingLayerIntElement: IntDropdownElement
    {
        private VisualElement _boundTarget;

        public void BindBound(VisualElement target) => _boundTarget = target;

        public SortingLayerIntElement()
        {
            Button.clicked += () =>
                SortingLayerUtils.MakeDropdown(false, value, _boundTarget ?? this, v => value = (int)v);
        }


        public override void SetValueWithoutNotify(int newValue)
        {
            CachedValue = newValue;

            foreach ((SortingLayer layer, int index) in SortingLayer.layers.WithIndex())
            {
                if (layer.value == newValue)
                {
                    Label.text = $"{layer.name} <color=#808080>({index})</color>";
                    return;
                }
            }

            Label.text = $"<color=red>?</color> ({newValue})";
        }
    }

    public class SortingLayerIntField : BaseField<int>
    {
        private readonly SortingLayerIntElement _sortingLayerIntElement;
        public SortingLayerIntField(string label, SortingLayerIntElement visualInput) : base(label, visualInput)
        {
            _sortingLayerIntElement = visualInput;
            visualInput.BindBound(this);
        }

        public override void SetValueWithoutNotify(int newValue)
        {
            _sortingLayerIntElement.SetValueWithoutNotify(newValue);
        }

        public override int value
        {
            get => _sortingLayerIntElement.value;
            set => _sortingLayerIntElement.value = value;
        }
    }
}
#endif

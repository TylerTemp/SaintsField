#if UNITY_2021_3_OR_NEWER
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

            // foreach ((SortingLayer layer, int index) in SortingLayer.layers.WithIndex())
            foreach (SortingLayer layer in SortingLayer.layers)
            {
                if (layer.id == newValue)
                {
                    Label.text = $"{layer.name} <color=#808080>({layer.id})</color>";
                    return;
                }
            }

            Label.text = $"<color=red>?</color> ({newValue})";
        }
    }

    public class SortingLayerIntField : IntDropdownField
    {
        private SortingLayerIntField(string label, SortingLayerIntElement visualInput) : base(label, visualInput)
        {
            visualInput.BindBound(this);
            visualInput.RegisterValueChangedCallback(evt => evt.StopPropagation());
        }

        public SortingLayerIntField(string label) : this(label, new SortingLayerIntElement())
        {
        }
    }
}
#endif

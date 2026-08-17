#if UNITY_2021_3_OR_NEWER
using SaintsField.Editor.UIToolkitElements;
using UnityEngine;
using UnityEngine.UIElements;


namespace SaintsField.Editor.Drawers.LayerDrawer
{
    public class LayerIntDropdownElement: IntDropdownElement
    {
        public void BindDrop(VisualElement root)
        {
            Button.clicked += () =>
                LayerUtils.MakeDropdown(false, 1 << CachedValue, root, newValue => value = newValue.Value);
        }

        public override void SetValueWithoutNotify(int newValue)
        {
            CachedValue = newValue;

            foreach (LayerUtils.LayerInfo layerInfo in LayerUtils.GetAllLayers())
            {
                // ReSharper disable once InvertIf
                if (layerInfo.Value == newValue)
                {
                    Label.text = LayerUtils.LayerInfoLabelUIToolkit(layerInfo);
                    Button.tooltip = layerInfo.Name;
                    return;
                }
            }

            Label.text =
                LayerUtils.LayerInfoLabelUIToolkit(new LayerUtils.LayerInfo("<color=red>?</color>", newValue));
            Button.tooltip = "Invalid layer";
        }
    }

    public class LayerIntDropdownField: IntDropdownField
    {
        private LayerIntDropdownField(string label, LayerIntDropdownElement visualInput) : base(label, visualInput)
        {
            visualInput.BindDrop(this);
        }

        public LayerIntDropdownField(string label) : this(label, new LayerIntDropdownElement())
        {
        }
    }
}
#endif

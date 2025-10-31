using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.UIToolkitElements;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_2021_3_OR_NEWER
namespace SaintsField.Editor.Drawers.LayerDrawer
{
    public class LayerMaskDropdownElement: IntDropdownElement
    {
        public void BindDrop(VisualElement root)
        {
            Button.clicked += () =>
            {
                if (CachedValue == null)
                {
                    return;
                }

                LayerUtils.MakeDropdown(false, (int)CachedValue, root, newValue => value = newValue.Mask);
            };
        }

        public override void SetValueWithoutNotify(int newValue)
        {
            CachedValue = newValue;

            bool hasInvalid = false;
            List<LayerUtils.LayerInfo> selected = new List<LayerUtils.LayerInfo>();
            foreach (LayerUtils.LayerInfo layerInfo in LayerUtils.GetAllLayers())
            {
                if (layerInfo.Mask == newValue)
                {
                    Label.text = LayerUtils.LayerInfoLabelUIToolkit(layerInfo);
                    return;
                }

                int maskValue = layerInfo.Mask & newValue;

                if (maskValue != 0)
                {
                    if (maskValue == layerInfo.Mask)
                    {
                        selected.Add(layerInfo);
                    }
                    else
                    {
                        hasInvalid = true;
                    }
                }
            }

            if (hasInvalid)
            {
                Label.text = LayerUtils.LayerInfoLabelUIToolkit(new LayerUtils.LayerInfo("<color=red>?</color>", newValue));
                Button.tooltip = "Invalid layer value";
            }
            else if (selected.Count > 0)
            {
                Label.text =
                    $"<color=red>!</color> {string.Join(", ", selected.Select(LayerUtils.LayerInfoLabelUIToolkit))}";
                Button.tooltip = "Multiple layer selected";
            }
            else
            {
                Button.tooltip = "";
            }
        }
    }

    public class LayerMaskDropdownField : BaseField<int>
    {
        public LayerMaskDropdownField(string label, LayerMaskDropdownElement visualInput) : base(label, visualInput)
        {
            visualInput.BindDrop(this);
        }
    }
}
#endif

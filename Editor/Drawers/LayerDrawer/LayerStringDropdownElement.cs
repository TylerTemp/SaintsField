#if UNITY_2021_3_OR_NEWER
using SaintsField.Editor.UIToolkitElements;
using UnityEngine;
using UnityEngine.UIElements;


namespace SaintsField.Editor.Drawers.LayerDrawer
{
    public class LayerStringDropdownElement: StringDropdownElement
    {
        public void BindDrop(VisualElement root)
        {
            Button.clicked += () =>
            {
                if (CachedValue == null)
                {
                    return;
                }

                LayerUtils.MakeDropdown(false, _maskValue, root, newValue =>
                {
                    // Debug.Log($"dropdown set value to {newValue.Name}");
                    value = newValue.Name;
                });
            };
        }

        private int _maskValue;

        public override void SetValueWithoutNotify(string newValue)
        {
            // Debug.Log($"set value {newValue}");
            CachedValue = newValue;
            foreach (LayerUtils.LayerInfo layerInfo in LayerUtils.GetAllLayers())
            {
                if (layerInfo.Name == newValue)
                {
                    // Debug.Log($"set label to {LayerUtils.LayerInfoLabelUIToolkit(layerInfo)}");
                    // Label.text = LayerUtils.LayerInfoLabelUIToolkit(layerInfo);
                    // ((INotifyValueChanged<string>)Label).SetValueWithoutNotify(LayerUtils.LayerInfoLabelUIToolkit(layerInfo));
                    SetLabelString(LayerUtils.LayerInfoLabelUIToolkit(layerInfo));
                    _maskValue = layerInfo.Mask;
                    Button.tooltip = layerInfo.Name;
                    return;
                }
            }

            // Debug.Log($"set invalid label to question {newValue}");

            // Label.text = $"<color=red>?</color> {newValue}";
            // ((INotifyValueChanged<string>)Label).SetValueWithoutNotify($"<color=red>?</color> {newValue}");
            SetLabelString($"<color=red>?</color> {newValue}");

            _maskValue = -1;
            Button.tooltip = "Invalid layer";
        }

    }
}
#endif

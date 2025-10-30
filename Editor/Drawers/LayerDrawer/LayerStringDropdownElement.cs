#if UNITY_2021_3_OR_NEWER
using SaintsField.Editor.UIToolkitElements;
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

                LayerUtils.MakeDropdown(false, _maskValue, root, newValue => value = newValue.Name);
            };
        }

        private int _maskValue;

        public override void SetValueWithoutNotify(string newValue)
        {
            CachedValue = newValue;

            foreach (LayerUtils.LayerInfo layerInfo in LayerUtils.GetAllLayers())
            {
                if (layerInfo.Name == newValue)
                {
                    Label.text = LayerUtils.LayerInfoLabelUIToolkit(layerInfo);
                    _maskValue = layerInfo.Mask;
                    return;
                }
            }

            Label.text = $"<color=red>?</color> {newValue}";
            _maskValue = -1;
        }

    }
}
#endif

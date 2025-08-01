#if UNITY_2021_3_OR_NEWER
using SaintsField.Editor.UIToolkitElements;


namespace SaintsField.Editor.Drawers.LayerDrawer
{
    public class LayerStringDropdownElement: StringDropdownElement
    {
        public override void SetValueWithoutNotify(string newValue)
        {
            CachedValue = newValue;

            foreach (LayerUtils.LayerInfo layerInfo in LayerUtils.GetAllLayers())
            {
                if (layerInfo.Name == newValue)
                {
                    Label.text = LayerUtils.LayerInfoLabelUIToolkit(layerInfo);
                    return;
                }

                Label.text = $"<color=red>?</color> {newValue}";
            }
        }

    }
}
#endif

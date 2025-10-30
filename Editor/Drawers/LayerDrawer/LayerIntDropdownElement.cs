#if UNITY_2021_3_OR_NEWER
using SaintsField.Editor.UIToolkitElements;
using UnityEngine.UIElements;


namespace SaintsField.Editor.Drawers.LayerDrawer
{
    public class LayerIntDropdownElement: IntDropdownElement
    {
        public void BindDrop(VisualElement root)
        {
            Button.clicked += () =>
            {
                if (CachedValue == null)
                {
                    return;
                }

                LayerUtils.MakeDropdown(false, 1 << (int)CachedValue, root, newValue => value = newValue.Value);
            };
        }

        public override void SetValueWithoutNotify(int newValue)
        {
            CachedValue = newValue;

            foreach (LayerUtils.LayerInfo layerInfo in LayerUtils.GetAllLayers())
            {
                if (layerInfo.Value == newValue)
                {
                    Label.text = LayerUtils.LayerInfoLabelUIToolkit(layerInfo);
                    return;
                }
            }

            Label.text =
                LayerUtils.LayerInfoLabelUIToolkit(new LayerUtils.LayerInfo("<color=red>?</color>", newValue));
        }
    }
}
#endif

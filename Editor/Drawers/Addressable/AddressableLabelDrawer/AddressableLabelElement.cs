#if UNITY_2021_3_OR_NEWER
using System.Collections.Generic;
using SaintsField.Editor.UIToolkitElements;
using UnityEditor.AddressableAssets;

namespace SaintsField.Editor.Drawers.Addressable.AddressableLabelDrawer
{
    public class AddressableLabelElement: StringDropdownElement
    {
        public override void SetValueWithoutNotify(string newValue)
        {
            CachedValue = newValue;

            List<string> labels = AddressableAssetSettingsDefaultObject.Settings?.GetLabels() ?? new List<string>();

            Label.text = AddressableLabelAttributeDrawer.GetAddressableLabelDisplay(CachedValue, labels, true);
        }
    }
}
#endif

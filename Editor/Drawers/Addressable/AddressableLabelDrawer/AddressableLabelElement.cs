#if UNITY_2021_3_OR_NEWER
using System.Collections.Generic;
using SaintsField.Editor.Drawers.Addressable.AddressableAddressDrawer;
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

    public class AddressableLabelField : StringDropdownField
    {
        private AddressableLabelField(string label, AddressableLabelElement addressableAddressAttribute) : base(label, addressableAddressAttribute)
        {
        }

        public AddressableLabelField(string label) : base(label, new AddressableLabelElement())
        {
        }
    }
}
#endif

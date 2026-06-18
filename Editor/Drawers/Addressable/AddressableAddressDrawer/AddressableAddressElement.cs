#if UNITY_2021_3_OR_NEWER
using System.Collections.Generic;
using System.Linq;
using SaintsField.Addressable;
using SaintsField.Editor.UIToolkitElements;
using UnityEditor.AddressableAssets.Settings;

namespace SaintsField.Editor.Drawers.Addressable.AddressableAddressDrawer
{
    public class AddressableAddressElement: StringDropdownElement
    {
        private readonly AddressableAddressAttribute _addressableAddressAttribute;

        public AddressableAddressElement(AddressableAddressAttribute addressableAddressAttribute)
        {
            _addressableAddressAttribute = addressableAddressAttribute;
        }

        public override void SetValueWithoutNotify(string newValue)
        {
            CachedValue = newValue;

            (string _, IEnumerable<AddressableAssetEntry> entries) =
                AddressableUtil.GetAllEntries(_addressableAddressAttribute.Group,
                    _addressableAddressAttribute.LabelFilters);
            string[] keys = entries.Select(each => each.address).ToArray();

            Label.text = AddressableAddressAttributeDrawer.GetAddressableAddressDisplay(CachedValue, keys, true);
        }
    }
}
#endif

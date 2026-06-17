#if UNITY_2021_3_OR_NEWER
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.TreeDropdownDrawer
{
    public class DropdownButtonLongElement: DropdownButtonGenElement<long>
    {
        public DropdownButtonLongElement(EnumMetaInfo metaInfo) : base(metaInfo)
        {
        }
    }

    public class DropdownFieldLong : BaseField<long>
    {
        public DropdownFieldLong(string label, DropdownButtonLongElement visualInput) : base(label, visualInput)
        {
        }
    }
}
#endif

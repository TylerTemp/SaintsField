#if UNITY_2021_3_OR_NEWER
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.TreeDropdownDrawer
{
    public class DropdownButtonULongElement: DropdownButtonGenElement<ulong>
    {
        public DropdownButtonULongElement(EnumMetaInfo metaInfo) : base(metaInfo)
        {
        }
    }

    public class DropdownFieldULong : BaseField<long>
    {
        public DropdownFieldULong(string label, DropdownButtonULongElement visualInput) : base(label, visualInput)
        {
        }
    }
}
#endif

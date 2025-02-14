using SaintsField.Addressable;
using SaintsField.Editor.Core;
using UnityEditor;

namespace SaintsField.Editor.Drawers.Addressable.AddressableAddressDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.SuperPriority)]
#endif
    [CustomPropertyDrawer(typeof(AddressableAddressAttribute), true)]
    public partial class AddressableAddressAttributeDrawer: SaintsPropertyDrawer
    {
        // private IReadOnlyList<string> _targetKeys;

    }
}

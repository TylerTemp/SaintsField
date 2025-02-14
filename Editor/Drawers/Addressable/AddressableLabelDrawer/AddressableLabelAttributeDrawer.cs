using SaintsField.Addressable;
using SaintsField.Editor.Core;
using UnityEditor;

namespace SaintsField.Editor.Drawers.Addressable.AddressableLabelDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.SuperPriority)]
#endif
    [CustomPropertyDrawer(typeof(AddressableLabelAttribute), true)]
    public partial class AddressableLabelAttributeDrawer: SaintsPropertyDrawer
    {
        // private static string ErrorNoSettings => "Addressable has no settings created yet.";
    }
}

using SaintsField.Addressable;
using SaintsField.Editor.Core;
using UnityEditor;

namespace SaintsField.Editor.Drawers.Addressable.AddressableLabelDrawer
{
    [CustomPropertyDrawer(typeof(AddressableLabelAttribute))]
    public partial class AddressableLabelAttributeDrawer: SaintsPropertyDrawer
    {
        // private static string ErrorNoSettings => "Addressable has no settings created yet.";
    }
}

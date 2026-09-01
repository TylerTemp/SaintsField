using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.UnitDrawer;
using UnityEditor;

namespace SaintsField.Editor.Drawers.AdaptDrawer
{
    [CustomPropertyDrawer(typeof(AdaptAttribute))]
    public partial class AdaptAttributeDrawer: UnitAttributeDrawer
    {
    }
}

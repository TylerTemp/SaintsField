#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using SaintsField.Editor.Core;
using UnityEditor;

namespace SaintsField.Editor.Drawers
{

    [CustomPropertyDrawer(typeof(UIToolkitAttribute))]
    public class UIToolkitAttributeDrawer: SaintsPropertyDrawer
    {
    }
}
#endif

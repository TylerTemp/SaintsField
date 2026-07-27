using SaintsField.Editor.Core;
using UnityEditor;

namespace SaintsField.Editor.Drawers.DropdownDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.AttributePriority)]
#endif
    [CustomPropertyDrawer(typeof(DropdownAttribute), true)]
    public partial class DropdownAttributeDrawer: SaintsPropertyDrawer
    {
        private readonly RichTextDrawer _richTextDrawer = new RichTextDrawer();
    }
}

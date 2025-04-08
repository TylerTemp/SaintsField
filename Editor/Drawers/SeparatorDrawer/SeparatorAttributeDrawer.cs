using SaintsField.Editor.Core;
using UnityEditor;

namespace SaintsField.Editor.Drawers.SeparatorDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.SuperPriority)]
#endif
    [CustomPropertyDrawer(typeof(SeparatorAttribute), true)]
    [CustomPropertyDrawer(typeof(BelowSeparatorAttribute), true)]
    public partial class SeparatorAttributeDrawer: SaintsPropertyDrawer
    {
        private readonly RichTextDrawer _richTextDrawer = new RichTextDrawer();
    }
}

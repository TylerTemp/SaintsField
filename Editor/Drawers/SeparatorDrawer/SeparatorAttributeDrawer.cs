using SaintsField.Editor.Core;
using UnityEditor;

namespace SaintsField.Editor.Drawers.SeparatorDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.WrapperPriority)]
#endif
    [CustomPropertyDrawer(typeof(FieldSeparatorAttribute), true)]
    [CustomPropertyDrawer(typeof(FieldBelowSeparatorAttribute), true)]
    public partial class SeparatorAttributeDrawer: SaintsPropertyDrawer
    {
        private readonly RichTextDrawer _richTextDrawer = new RichTextDrawer();
    }
}

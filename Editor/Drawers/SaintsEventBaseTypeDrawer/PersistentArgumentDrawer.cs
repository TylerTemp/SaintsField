#if SAINTSFIELD_SERIALIZATION && !SAINTSFIELD_SERIALIZATION_DISABLED && UNITY_2022_2_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using SaintsField.Editor.Core;
using SaintsField.Events;
using UnityEditor;

namespace SaintsField.Editor.Drawers.SaintsEventBaseTypeDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.ValuePriority)]
#endif
    [CustomPropertyDrawer(typeof(PersistentArgument), true)]
    public partial class PersistentArgumentDrawer: SaintsPropertyDrawer
    {

    }
}
#endif

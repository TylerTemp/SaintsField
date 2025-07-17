#if SAINTSFIELD_SERIALIZATION
using SaintsField.Editor.Core;
using SaintsField.Events;
using UnityEditor;

namespace SaintsField.Editor.Drawers.SaintsEventBaseTypeDrawer
{

#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.ValuePriority)]
#endif
    [CustomPropertyDrawer(typeof(SaintsEventBase), true)]
    public partial class SaintsEventBaseDrawer: SaintsPropertyDrawer
    {
        private const string PropNamePersistentCalls = "_persistentCalls";
    }
}
#endif

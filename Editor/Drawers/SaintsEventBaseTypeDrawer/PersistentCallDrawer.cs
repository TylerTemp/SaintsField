using SaintsField.Editor.Core;
using SaintsField.Events;
using UnityEditor;

namespace SaintsField.Editor.Drawers.SaintsEventBaseTypeDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.ValuePriority)]
#endif
    [CustomPropertyDrawer(typeof(PersistentCall), true)]
    public partial class PersistentCallDrawer: SaintsPropertyDrawer
    {
        private static string PropNameCallState() => nameof(PersistentCall.callState);
        private const string PropNameIsStatic = "_isStatic";
        private const string PropNameTarget = "_target";
    }
}

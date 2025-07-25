#if SAINTSFIELD_SERIALIZATION && !SAINTSFIELD_SERIALIZATION_DISABLED
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
        private const string SubPropNameTypeNameAndAssmble = "._typeNameAndAssembly";
        private const string SubPropMonoScriptGuid = "._monoScriptGuid";
    }
}
#endif

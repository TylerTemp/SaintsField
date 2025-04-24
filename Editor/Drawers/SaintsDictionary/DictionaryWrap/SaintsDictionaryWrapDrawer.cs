using SaintsField.Editor.Core;
using UnityEditor;

namespace SaintsField.Editor.Drawers.SaintsDictionary.DictionaryWrap
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.SuperPriority)]
#endif
    [CustomPropertyDrawer(typeof(SaintsDictionaryBase<,>.Wrap<>), true)]
    public partial class SaintsDictionaryWrapDrawer: SaintsPropertyDrawer
    {

    }
}

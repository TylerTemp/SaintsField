using UnityEditor;

namespace SaintsField.Editor.Drawers.SepTitleDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.WrapperPriority)]
#endif
    [CustomPropertyDrawer(typeof(SepTitleAttribute), true)]
    public partial class SepTitleAttributeDrawer : DecoratorDrawer
    {
        // public static int drawCounter = 0;
        //
        // // public override bool CanCacheInspectorGUI() => false;
        //
        // public SepTitleAttributeDrawer()
        // {
        //     Debug.Log($"Create {drawCounter}: {string.Join(",", SaintsPropertyDrawer.SubCounter.Select(each => $"{each.Key} {each.Value}"))}");
        // }
    }
}

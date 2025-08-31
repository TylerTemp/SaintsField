using SaintsField.Editor.Core;
using UnityEditor;

namespace SaintsField.Editor.Drawers.CurveRangeDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.AttributePriority)]
#endif
    [CustomPropertyDrawer(typeof(CurveRangeAttribute), true)]
    public partial class CurveRangeAttributeDrawer: SaintsPropertyDrawer
    {


        // protected override void ChangeFieldLabelToUIToolkit(SerializedProperty property,
        //     ISaintsAttribute saintsAttribute, int index, VisualElement container, string labelOrNull,
        //     IReadOnlyList<RichTextDrawer.RichTextChunk> richTextChunks, bool tried, RichTextDrawer richTextDrawer)
        // {
        //     CurveField target = container.Q<CurveField>(NameCurveField(property));
        //     target.label = labelOrNull;
        // }

    }
}

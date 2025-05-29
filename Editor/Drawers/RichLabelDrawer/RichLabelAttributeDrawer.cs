using SaintsField.Editor.Core;
using UnityEditor;

namespace SaintsField.Editor.Drawers.RichLabelDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.WrapperPriority)]
#endif
    [CustomPropertyDrawer(typeof(RichLabelAttribute), true)]
    public partial class RichLabelAttributeDrawer: SaintsPropertyDrawer
    {
        private readonly RichTextDrawer _richTextDrawer = new RichTextDrawer();

        // private readonly Color _backgroundColor;
        //
        // public RichLabelAttributeDrawer()
        // {
        //     _backgroundColor = EditorGUIUtility.isProSkin
        //         ? new Color32(56, 56, 56, 255)
        //         : new Color32(194, 194, 194, 255);
        // }

        // ~RichLabelAttributeDrawer()
        // {
        //     _richTextDrawer.Dispose();
        // }

        // protected override float GetLabelHeight(SerializedProperty property, GUIContent label,
        //     ISaintsAttribute saintsAttribute) =>
        //     EditorGUIUtility.singleLineHeight;

        // protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
        //     FieldInfo info,
        //     object parent)
        // {
        //     return _error != "";
        // }
    }
}

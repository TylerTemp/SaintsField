using SaintsField.Editor.Core;
using UnityEditor;

namespace SaintsField.Editor.Drawers.ResizableTextAreaDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.AttributePriority)]
#endif
    [CustomPropertyDrawer(typeof(ResizableTextAreaAttribute), true)]
    public partial class ResizableTextAreaAttributeDrawer: SaintsPropertyDrawer
    {
        // private float _width = -1;

        // private bool _hasLabel = true;

        // private static float GetTextAreaHeight(string text) => (EditorGUIUtility.singleLineHeight - 3.0f) * GetNumberOfLines(text) + 3.0f;

        // private static int GetNumberOfLines(string text)
        // {
        //     if (text == null)
        //     {
        //         return 1;
        //     }
        //
        //     string content = Regex.Replace(text, @"\r\n|\n\r|\r|\n", Environment.NewLine);
        //     string[] lines = content.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
        //     return lines.Length;
        // }
    }
}

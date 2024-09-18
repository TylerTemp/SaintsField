using System;
using UnityEditor;

namespace SaintsField.Editor.Utils
{
    public class ImGuiLabelStyleRichTextScoop: IDisposable
    {
        private readonly bool _richText;

        public ImGuiLabelStyleRichTextScoop()
        {
            _richText = EditorStyles.label.richText;
            EditorStyles.label.richText = true;
        }

        public void Dispose()
        {
            EditorStyles.label.richText = _richText;
        }
    }
}

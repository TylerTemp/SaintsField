using System;
using UnityEditor;

namespace SaintsField.Editor.Utils
{
    public class ImGuiFoldoutStyleRichTextScoop: IDisposable
    {
        private readonly bool _richText;

        public ImGuiFoldoutStyleRichTextScoop()
        {
            _richText = EditorStyles.foldout.richText;
            EditorStyles.foldout.richText = true;
        }

        public void Dispose()
        {
            EditorStyles.foldout.richText = _richText;
        }
    }
}

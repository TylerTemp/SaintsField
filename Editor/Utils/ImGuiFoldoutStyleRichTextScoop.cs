using System;
using UnityEditor;

namespace SaintsField.Editor.Utils
{
    public class ImGuiFoldoutStyleRichTextScoop: IDisposable
    {
        private readonly bool _applied;
        private readonly bool _richText;

        public ImGuiFoldoutStyleRichTextScoop()
        {
            try
            {
                _richText = EditorStyles.foldout.richText;
                EditorStyles.foldout.richText = true;
            }
            catch (NullReferenceException)
            {
                return;
            }

            _applied = true;
        }

        public void Dispose()
        {
            if(_applied)
            {
                EditorStyles.foldout.richText = _richText;
            }
        }
    }
}

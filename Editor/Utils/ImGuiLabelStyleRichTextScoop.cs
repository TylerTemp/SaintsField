using System;
using UnityEditor;

namespace SaintsField.Editor.Utils
{
    public class ImGuiLabelStyleRichTextScoop: IDisposable
    {
        private readonly bool _applied;
        private readonly bool _richText;

        public ImGuiLabelStyleRichTextScoop()
        {
            try
            {
                _richText = EditorStyles.label.richText;
                EditorStyles.label.richText = true;
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
                EditorStyles.label.richText = _richText;
            }
        }
    }
}

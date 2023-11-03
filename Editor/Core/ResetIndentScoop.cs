using System;
using UnityEditor;

namespace SaintsField.Editor.Core
{
    public class ResetIndentScoop: IDisposable
    {
        private readonly int _curLevel;

        public ResetIndentScoop()
        {
            _curLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
        }

        public void Dispose()
        {
            EditorGUI.indentLevel = _curLevel;
        }
    }
}

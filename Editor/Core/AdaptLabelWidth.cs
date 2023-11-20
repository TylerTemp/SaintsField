using System;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Core
{
    public class AdaptLabelWidth: IDisposable
    {
        private readonly float _labelWidth;

        public AdaptLabelWidth()
        {
            _labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = ProperLabelWidth();
        }

        public void Dispose()
        {
            EditorGUIUtility.labelWidth = _labelWidth;
        }

        private static float ProperLabelWidth()
        {
            const float fakeWidth = 1000f;
            Rect indented = EditorGUI.IndentedRect(new Rect(0, 0, fakeWidth, fakeWidth));
            return EditorGUIUtility.labelWidth - indented.x;
        }
    }
}

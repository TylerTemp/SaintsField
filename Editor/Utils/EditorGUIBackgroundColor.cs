using System;
using UnityEngine;

namespace SaintsField.Editor.Utils
{
    public class EditorGUIBackgroundColor: IDisposable
    {
        private static readonly Color ToggledColor = new Color(0.65f, 0.65f, 0.65f);

        private readonly bool _colorSwapped;

        private readonly Color _oldColor;

        private EditorGUIBackgroundColor(Color newColor)
        {
            _oldColor = GUI.backgroundColor;
            GUI.backgroundColor = newColor;
            _colorSwapped = true;
        }

        private EditorGUIBackgroundColor()
        {
            _colorSwapped = false;
        }

        public static EditorGUIBackgroundColor ToggleButton(bool on) => on
            ? new EditorGUIBackgroundColor(ToggledColor)
            : new EditorGUIBackgroundColor();

        public void Dispose()
        {
            if(_colorSwapped)
            {
                GUI.backgroundColor = _oldColor;
            }
        }
    }
}

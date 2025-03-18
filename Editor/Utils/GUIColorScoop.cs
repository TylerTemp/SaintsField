using System;
using UnityEngine;

namespace SaintsField.Editor.Utils
{
    public readonly struct GUIColorScoop: IDisposable
    {
        private readonly Color _color;

        public GUIColorScoop(Color newColor)
        {
            _color = GUI.color;
            GUI.color = newColor;
        }

        public void Dispose()
        {
            GUI.color = _color;
        }
    }
}

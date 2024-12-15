using System;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.HandleDrawers
{
    public class HandleColorScoop: IDisposable
    {
        private readonly Color _oldColor;

        public HandleColorScoop(Color newColor)
        {
            _oldColor = Handles.color;
            Handles.color = newColor;
        }

        public void Dispose()
        {
            Handles.color = _oldColor;
        }
    }
}

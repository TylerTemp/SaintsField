using System;
using UnityEngine;

namespace SaintsField.Editor.Utils
{
    public class GUIContentColor: IDisposable
    {
        private readonly Color oldColor;

        public GUIContentColor(Color newColor)
        {
            oldColor = GUI.contentColor;
            GUI.contentColor = newColor;
        }

        public void Dispose()
        {
            GUI.contentColor = oldColor;
        }
    }
}

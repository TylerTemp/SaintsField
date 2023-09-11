using System;
using UnityEngine;

namespace ExtInspector.Editor.Utils
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

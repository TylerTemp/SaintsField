using System;
using UnityEngine;

namespace SaintsField.Editor.Drawers.SceneViewPickerDrawer
{
    public class GUIBeginScrollViewScoop: IDisposable
    {
        public Vector2 ScrollPosition { get; private set; }
        public GUIBeginScrollViewScoop(Rect position, Vector2 scrollPosition, Rect viewRect)
        {
            ScrollPosition = GUI.BeginScrollView(position, scrollPosition, viewRect);
        }

        public void Dispose()
        {
            GUI.EndScrollView();
        }
    }
}

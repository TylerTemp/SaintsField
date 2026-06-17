using System;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Utils
{
    public class IMGUILoading
    {
        private const float DegreesPerSecond = -360f;

        private Texture2D _icon;
        private bool _lastDrawTimeInit;
        private double _lastDrawTime;
        private float _rotation;

        private Texture2D GetIcon()
        {
            if (_icon == null)
            {
                _icon = EditorGUIUtility.IconContent("d_Loading").image as Texture2D;
            }

            return _icon;
        }

        public void Draw(Rect position)
        {
            if (!_lastDrawTimeInit)
            {
                _lastDrawTime = EditorApplication.timeSinceStartup;
                return;
            }

            if (Event.current == null || Event.current.type != EventType.Repaint)
            {
                return;
            }

            Texture2D icon = GetIcon();
            if (icon == null)
            {
                return;
            }

            double now = EditorApplication.timeSinceStartup;
            _rotation = Mathf.Repeat(_rotation + (float)((now - _lastDrawTime) * DegreesPerSecond), 360f);
            _lastDrawTime = now;

            Matrix4x4 oldMatrix = GUI.matrix;
            try
            {
                GUIUtility.RotateAroundPivot(_rotation, position.center);
                GUI.DrawTexture(position, icon, ScaleMode.ScaleToFit, true);
            }
            finally
            {
                GUI.matrix = oldMatrix;
            }
        }
    }
}

using System;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Utils.IMGUIPlainDrawer
{
    public readonly struct InHorizontalLayoutScoop: IDisposable
    {
        private readonly bool _inHorizontal;
        private readonly bool _oldWideMode;
        private readonly float _oldLabelWidth;

        public InHorizontalLayoutScoop(bool inHorizontal, Rect position)
        {
            _inHorizontal = inHorizontal;

            if (inHorizontal)
            {
                _oldWideMode = EditorGUIUtility.wideMode;
                _oldLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.wideMode = false;
                EditorGUIUtility.labelWidth = position.width;
            }
            else
            {
                _oldWideMode = false;
                _oldLabelWidth = 0;
            }
        }

        public void Dispose()
        {
            if (!_inHorizontal)
            {
                return;
            }

            EditorGUIUtility.wideMode = _oldWideMode;
            EditorGUIUtility.labelWidth = _oldLabelWidth;
        }
    }
}

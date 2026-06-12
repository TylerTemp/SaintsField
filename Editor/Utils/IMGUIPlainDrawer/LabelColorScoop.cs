using System;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Utils.IMGUIPlainDrawer
{
    public readonly struct LabelColorScoop: IDisposable
    {
        private readonly bool _changeColor;
        private readonly Color _prevColor;

        public LabelColorScoop(bool changeColor)
        {
            _changeColor = changeColor;
            if (!changeColor)
            {
                _prevColor = default;
                return;
            }

            _prevColor = EditorStyles.label.normal.textColor;
            EditorStyles.label.normal.textColor = IMGUIUtils.LabelGrayColor;
        }


        public void Dispose()
        {
            if(_changeColor)
            {
                EditorStyles.label.normal.textColor = _prevColor;
            }
        }
    }
}

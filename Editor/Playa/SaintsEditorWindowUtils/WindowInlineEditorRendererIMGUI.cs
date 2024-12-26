using System;
using SaintsField.Editor.Drawers;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Playa.SaintsEditorWindowUtils
{
    public partial class WindowInlineEditorRenderer
    {
        private UnityEditor.Editor _editor;
        private Object _curTarget;

        protected override void RenderTargetIMGUI(PreCheckResult preCheckResult)
        {
            // if (Event.current.type != EventType.Layout)
            // {
            //     return;
            // }
            Object target = SerializedUtils.GetSerObject(_fieldWithInfo.SerializedProperty,
                _fieldWithInfo.FieldInfo, _fieldWithInfo.Target);

            // Debug.Log(target);

            if (Util.IsNull(target))
            {
                return;
            }

            if (!ReferenceEquals(target, _curTarget))
            {
                if (_editor != null)
                {
                    Object.DestroyImmediate(_editor);
                    _editor = null;
                }
            }

            if (_editor == null)
            {
                _editor = UnityEditor.Editor.CreateEditor(target);
            }

            try
            {
                _editor.OnInspectorGUI();
            }
            catch (ArgumentException)
            {
                // ignored
            }
        }

        protected override float GetFieldHeightIMGUI(float width, PreCheckResult preCheckResult)
        {
            throw new System.NotSupportedException();
        }

        protected override void RenderPositionTarget(Rect position, PreCheckResult preCheckResult)
        {
            throw new System.NotSupportedException();
        }
    }
}

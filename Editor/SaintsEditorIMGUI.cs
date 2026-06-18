using System;
using SaintsField.Editor.HeaderGUI;
using SaintsField.Editor.Playa;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor
{
    public partial class SaintsEditor
    {
        public override bool RequiresConstantRepaint() => true;

        private void OnEnableIMGUI()
        {
            if (!_saintsEditorIMGUI)
            {
                return;
            }

            _coreEditor ??= new SaintsEditorCore(this, EditorShowMonoScript, this);

            _coreEditor.OnEnableIMGUI();
        }

        private void OnDestroyIMGUI()
        {
            _coreEditor?.OnDestroyIMGUI();
            _coreEditor = null;
        }

        public override void OnInspectorGUI()
        {
            _coreEditor ??= new SaintsEditorCore(this, EditorShowMonoScript, this);
            _coreEditor.OnInspectorGUI();

#if DOTWEEN && SAINTSFIELD_DOTWEEN_ENABLE
            AliveInstances.Add(this);
#endif
        }
    }
}

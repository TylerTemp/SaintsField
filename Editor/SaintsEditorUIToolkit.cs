#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System;
using SaintsField.Editor.Playa;
using UnityEditor.UIElements;
using UnityEngine.UIElements;


namespace SaintsField.Editor
{
    public partial class SaintsEditor
    {
        private void OnHeaderButtonClickUIToolkit()
        {
            _coreEditor.OnHeaderButtonClick();
            // _toolbarSearchField.style.display = _searchableShown ? DisplayStyle.Flex : DisplayStyle.None;
            // if(_searchableShown)
            // {
            //     _toolbarSearchField.Focus();
            // }
        }

        private ToolbarSearchField _toolbarSearchField;

        private SaintsEditorCore _coreEditor;

        public override VisualElement CreateInspectorGUI()
        {
            _saintsEditorIMGUI = false;
            _coreEditor = new SaintsEditorCore(this, EditorShowMonoScript, this);
            VisualElement root = _coreEditor.CreateInspectorGUI();

#if DOTWEEN && !SAINTSFIELD_DOTWEEN_DISABLED
            root.RegisterCallback<AttachToPanelEvent>(_ => AddInstance(this));
            root.RegisterCallback<DetachFromPanelEvent>(_ => RemoveInstance(this));
#endif
            return root;
        }

        private void OnDestroyUIToolkit()
        {
            foreach (ISaintsRenderer saintsRenderer in _coreEditor?.AllRenderersUIToolkit ?? Array.Empty<ISaintsRenderer>())
            {
                saintsRenderer.OnDestroy();
            }
        }

        private void ResetSearchUIToolkit()
        {
            _coreEditor.ResetSearchUIToolkit();
            // // ReSharper disable once InvertIf
            // if (_toolbarSearchField.parent != null)
            // {
            //     _toolbarSearchField.parent.Focus();
            //     _toolbarSearchField.value = "";
            // }
        }
    }
}
#endif

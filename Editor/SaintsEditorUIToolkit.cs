#if UNITY_2021_3_OR_NEWER
using System;
using SaintsField.Editor.Playa;
using SaintsField.Editor.Utils;
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

#if !SAINTSFIELD_UI_TOOLKIT_DISABLE

        public override VisualElement CreateInspectorGUI()
        {
            _saintsEditorIMGUI = false;
            _coreEditor = new SaintsEditorCore(this, EditorShowMonoScript, this);
            // VisualElement root = new VisualElement();
            VisualElement root = _coreEditor.CreateInspectorGUI();

#if DOTWEEN && SAINTSFIELD_DOTWEEN_ENABLE
            root.RegisterCallback<AttachToPanelEvent>(_ => AddInstance(this));
            root.RegisterCallback<DetachFromPanelEvent>(_ => RemoveInstance(this));
#endif
            UIToolkitUtils.OnAttachToPanelOnce(root, _ =>
            {
                root.schedule.Execute(() => HeaderGUI.DrawHeaderGUI.EnsureInitLoad()).StartingIn(500);
            });

            return root;
        }
#endif

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

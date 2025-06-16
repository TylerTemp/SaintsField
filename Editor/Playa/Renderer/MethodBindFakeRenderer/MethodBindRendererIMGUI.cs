using System.Collections.Generic;
using SaintsField.Editor.Core;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using UnityEngine;

namespace SaintsField.Editor.Playa.Renderer.MethodBindFakeRenderer
{
    public partial class MethodBindRenderer
    {
        private RichTextDrawer _richTextDrawer;

        private string _cachedCallbackLabelIMGUI;
        private IReadOnlyList<RichTextDrawer.RichTextChunk> _cachedRichTextChunksIMGUI;

        private bool _addListener;

        private void EnsureAddListener()
        {
            if (_addListener)
            {
                return;
            }

            _addListener = true;
            SaintsEditorApplicationChanged.OnAnyEvent.AddListener(OnApplicationChanged);
        }

        protected override void RenderTargetIMGUI(float width, AbsRenderer.PreCheckResult preCheckResult)
        {
            EnsureAddListener();
        }

        protected override float GetFieldHeightIMGUI(float width, AbsRenderer.PreCheckResult preCheckResult)
        {
            EnsureAddListener();
            return 0;
        }

        protected override void RenderPositionTargetIMGUI(Rect position, AbsRenderer.PreCheckResult preCheckResult)
        {
            EnsureAddListener();
        }
    }
}

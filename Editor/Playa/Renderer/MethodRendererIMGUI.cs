using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Playa.Renderer
{
    public partial class MethodRenderer
    {
        private RichTextDrawer _richTextDrawer;

        private string _cachedCallbackLabelIMGUI;
        private IReadOnlyList<RichTextDrawer.RichTextChunk> _cachedRichTextChunksIMGUI;

        private void OnDestroyIMGUI()
        {
            _richTextDrawer?.Dispose();
        }

        private IReadOnlyList<RichTextDrawer.RichTextChunk> GetRichIMGUI(ButtonAttribute buttonAttribute, MethodInfo methodInfo)
        {
            // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
            if(_richTextDrawer == null)
            {
                _richTextDrawer = new RichTextDrawer();
            }

            if (string.IsNullOrEmpty(buttonAttribute.Label))
            {
                return new[] {new RichTextDrawer.RichTextChunk
                {
                    Content = ObjectNames.NicifyVariableName(methodInfo.Name),
                }};
            }

            if (!buttonAttribute.IsCallback)
            {
                if (string.IsNullOrEmpty(buttonAttribute.Label))
                {
                    return new[] {new RichTextDrawer.RichTextChunk
                    {
                        Content = ObjectNames.NicifyVariableName(methodInfo.Name),
                    }};
                }

                if (_cachedRichTextChunksIMGUI == null)
                {
                    _cachedRichTextChunksIMGUI = RichTextDrawer.ParseRichXml(buttonAttribute.Label,
                        FieldWithInfo.MethodInfo.Name, FieldWithInfo.MethodInfo, FieldWithInfo.Target).ToArray();
                }

                return _cachedRichTextChunksIMGUI;
            }

            (string error, string result) = Util.GetOf<string>(buttonAttribute.Label, null, FieldWithInfo.SerializedProperty, FieldWithInfo.MethodInfo, FieldWithInfo.Target);

            if (error != "")
            {
#if SAINTSFIELD_DEBUG
                Debug.LogError(error);
#endif

                return new[] {new RichTextDrawer.RichTextChunk
                {
                    Content = ObjectNames.NicifyVariableName(methodInfo.Name),
                }};
            }

            if (result == _cachedCallbackLabelIMGUI)
            {
                return _cachedRichTextChunksIMGUI;
            }

            IEnumerable<RichTextDrawer.RichTextChunk> chunks = RichTextDrawer.ParseRichXml(result,
                FieldWithInfo.MethodInfo.Name, FieldWithInfo.MethodInfo, FieldWithInfo.Target);
            _cachedCallbackLabelIMGUI = result;
            _cachedRichTextChunksIMGUI = chunks.ToArray();

            return _cachedRichTextChunksIMGUI;
        }
    }
}

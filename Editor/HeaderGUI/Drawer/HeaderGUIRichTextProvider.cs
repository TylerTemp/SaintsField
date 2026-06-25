using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Utils;
using UnityEngine;

namespace SaintsField.Editor.HeaderGUI.Drawer
{
    public class HeaderGUIRichTextProvider : IRichTextTagProvider
    {
        private readonly string _label;
        private readonly DrawHeaderGUI.RenderTargetInfo _renderTargetInfo;
        private readonly object _parent;

        public HeaderGUIRichTextProvider(string label, DrawHeaderGUI.RenderTargetInfo renderTargetInfo, object target)
        {
            _label = label;
            _renderTargetInfo = renderTargetInfo;
            _parent = target;
        }

        public string GetLabel() => _label;

        public string GetContainerType()
        {
            if (RuntimeUtil.IsNull(_parent))
            {
                return "";
            }

            return _parent.GetType().Name;
        }

        public string GetContainerTypeBaseType()
        {
            if (RuntimeUtil.IsNull(_parent))
            {
                return "";
            }

            return _parent.GetType().BaseType?.Name ?? "";
        }

        public string GetIndex(string formatter)
        {
            return "";
        }

        public string GetField(string rawContent, string tagName, string tagValue)
        {
            object accParent = _parent;

            if (RuntimeUtil.IsNull(accParent))
            {
                return rawContent;
            }

            // Debug.Log(parsedResult.content);
            (string error, int index, object value) result =
                Util.GetValueAtIndex(-1, _renderTargetInfo.MemberInfo, accParent);
            if (result.error != "")
            {
#if SAINTSFIELD_DEBUG
                Debug.LogWarning(result.error);
#endif
                return rawContent;
            }

            if (tagName == "field")
            {
                return RichTextDrawer.TagStringFormatter(result.value, tagValue);
            }

            // ReSharper disable once ReplaceSubstringWithRangeIndexer
            string revName = tagName["field.".Length..];
            (string error, MemberInfo _, object result) getOfValue = Util.GetOf<object>(revName, null,
                null,
                _renderTargetInfo.MemberInfo, result.value, null);

            // ReSharper disable once InvertIf
            if (getOfValue.error != "")
            {
#if SAINTSFIELD_DEBUG
                Debug.LogWarning(getOfValue.error);
#endif
                return rawContent;
            }

            return RichTextDrawer.TagStringFormatter(getOfValue.result, tagValue);
        }
    }
}

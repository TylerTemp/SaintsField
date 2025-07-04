using System.Collections.Generic;
using SaintsField.Editor.Core;

namespace SaintsField.Editor.HeaderGUI
{
    public static class CacheAndUtil
    {
        private static RichTextDrawer _richTextDrawer;

        public static RichTextDrawer GetCachedRichTextDrawer()
        {
            // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
            if (_richTextDrawer is null)
            {
                _richTextDrawer = new RichTextDrawer();
            }

            return _richTextDrawer;
        }

        public static readonly Dictionary<string, IReadOnlyList<RichTextDrawer.RichTextChunk>> ParsedXmlCache = new Dictionary<string, IReadOnlyList<RichTextDrawer.RichTextChunk>>();
    }
}

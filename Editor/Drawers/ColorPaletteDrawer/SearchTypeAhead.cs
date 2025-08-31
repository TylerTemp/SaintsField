#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using SaintsField.Editor.UIToolkitElements;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.ColorPaletteDrawer
{
    public class SearchTypeAhead: CleanableTextInputTypeAhead
    {
        public Func<IReadOnlyList<string>> GetOptionsFunc;
        public Func<string, bool> OnInputOptionTypeAheadFunc;

        public SearchTypeAhead(VisualElement root) : base(root)
        {
        }

        protected override IReadOnlyList<string> GetOptions()
        {
            return GetOptionsFunc?.Invoke() ?? Array.Empty<string>();
        }

        protected override bool OnInputOptionReturn(string value)
        {
            return false;
        }

        protected override bool OnInputOptionTypeAhead(string value)
        {
            return OnInputOptionTypeAheadFunc?.Invoke(value) ?? false;
        }
    }
}
#endif

#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using SaintsField.Editor.UIToolkitElements;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.ColorPaletteDrawer
{
    public class SearchTypeAhead: CleanableTextInputTypeAhead
    {
        public Func<IEnumerable<string>> GetOptionsFunc;
        public Func<string, bool> OnInputOptionFunc;

        public SearchTypeAhead(VisualElement root): base(root){}

        protected override IEnumerable<string> GetOptions()
        {
            return GetOptionsFunc?.Invoke() ?? Array.Empty<string>();
        }

        protected override bool OnInputOption(string value)
        {
            return OnInputOptionFunc?.Invoke(value) ?? false;
        }
    }
}
#endif

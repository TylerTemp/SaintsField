using SaintsField.Editor.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.UIToolkitElements
{
    public static class TreeIndentUtil
    {
        private static VisualTreeAsset _treeRowIndentIconTemplate;

        public static TemplateContainer MakeIndentElement(int indent)
        {
            // root.style.paddingLeft = indent * SaintsPropertyDrawer.IndentWidth;
            _treeRowIndentIconTemplate ??= Util.LoadResource<VisualTreeAsset>("UIToolkit/TreeDropdown/TreeRowIndentIcon.uxml");
            TemplateContainer clone = _treeRowIndentIconTemplate.CloneTree();
            // range: 0.4, 0.3, 0.2, 0.1
            float alpha = (4 - indent % 4) / 10f;
            clone.Q<VisualElement>("saintsfield-tree-row-indent-icon").style.unityBackgroundImageTintColor =
                new StyleColor(new Color(1, 1, 1, alpha));
            return clone;
        }
    }
}

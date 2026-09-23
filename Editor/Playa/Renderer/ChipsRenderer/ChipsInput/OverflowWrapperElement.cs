using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.Renderer.ChipsRenderer.ChipsInput
{
#if UNITY_6000_0_OR_NEWER
    [UxmlElement]
#endif
    public partial class OverflowWrapperElement: VisualElement
    {
#if !UNITY_6000_0_OR_NEWER
        public new class UxmlFactory : UxmlFactory<OverflowWrapperElement, UxmlTraits> { }
#endif

        // ReSharper disable once MemberCanBePrivate.Global
        public OverflowWrapperElement()
        {
            // VisualTreeAsset tree = Util.LoadResource<VisualTreeAsset>("UIToolkit/ChipInput/OverflowWrapper.uxml");
            // tree.CloneTree(this);
            //
            // ScrollView scrollView = this.Q<ScrollView>("overflowContainer");
            // contentContainer = scrollView.contentContainer;
            style.position = Position.Absolute;
            style.backgroundColor = new StyleColor(EditorGUIUtility.isProSkin
                ? new Color32(56, 56, 56, 255)
                : new Color32(194, 194, 194, 255));
            style.color = EditorStyles.label.normal.textColor;
            style.borderTopWidth = 1;
            style.borderBottomWidth = 1;
            style.borderLeftWidth = 1;
            style.borderRightWidth = 1;
            style.borderTopColor = style.borderBottomColor = style.borderLeftColor = style.borderRightColor = Color.gray;
        }

        // public override VisualElement contentContainer { get; }

        public void AnchorTo(VisualElement element)
        {
            const float dropdownHeight = 300f;

            if (element == null || parent == null || element.panel == null || panel != element.panel)
            {
                return;
            }

            Rect anchorBound = element.worldBound;
            Rect viewportBound = element.panel.visualTree.worldBound;
            float spaceBelow = viewportBound.yMax - anchorBound.yMax - 10;  // add a little gap
            bool placeBelow = spaceBelow >= dropdownHeight;
            Vector2 localPosition = parent.WorldToLocal(new Vector2(
                anchorBound.xMin,
                placeBelow ? anchorBound.yMax : anchorBound.yMin - dropdownHeight));
            Rect localAnchorBound = parent.WorldToLocal(anchorBound);

            style.left = localPosition.x;
            style.top = localPosition.y;
            style.width = localAnchorBound.width;
            style.maxHeight = placeBelow ? spaceBelow : dropdownHeight;
            if (placeBelow)
            {
                style.height = StyleKeyword.Auto;
            }
            else
            {
                style.height = dropdownHeight;
            }
        }
    }
}

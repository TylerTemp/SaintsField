#if UNITY_2021_3_OR_NEWER
using SaintsField.Editor.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.Renderer.HeaderRenderer
{
    public partial class HeaderAttributeRenderer
    {
        protected override (VisualElement target, bool needUpdate) CreateTargetUIToolkit(VisualElement inspectorRoot, VisualElement container)
        {
            Label result = new Label(_headerAttribute.header)
            {
                style =
                {
                    marginLeft = 3,
                    marginTop = 13,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    unityTextAlign = TextAnchor.LowerLeft,
                },
            };
            result.AddToClassList("unity-header-drawer__label");

            _onSearchFieldUIToolkit.AddListener(Search);
            result.RegisterCallback<DetachFromPanelEvent>(_ => _onSearchFieldUIToolkit.RemoveListener(Search));

            return (result, false);

            void Search(string search)
            {
                DisplayStyle display = Util.UnityDefaultSimpleSearch(GetFriendlyName(FieldWithInfo), search)
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;

                if (result.style.display != display)
                {
                    result.style.display = display;
                }
            }
        }
        public override void OnDestroyUIToolkit()
        {
        }
    }
}
#endif

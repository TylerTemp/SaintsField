#if UNITY_2021_3_OR_NEWER
using SaintsField.Editor.Utils;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.Renderer.SpaceRenderer
{
    public partial class SpaceAttributeRenderer
    {
        protected override (VisualElement target, bool needUpdate) CreateTargetUIToolkit(
            VisualElement inspectorRoot, VisualElement container)
        {
            VisualElement result = new VisualElement
            {
                style =
                {
                    height = _spaceAttribute.height,
                },
            };

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

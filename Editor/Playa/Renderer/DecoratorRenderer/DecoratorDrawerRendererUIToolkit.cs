#if UNITY_2021_3_OR_NEWER
using SaintsField.Editor.Utils;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.Renderer.DecoratorRenderer
{
    public partial class DecoratorDrawerRenderer
    {
        public override void OnDestroyUIToolkit()
        {
        }

        protected override (VisualElement target, bool needUpdate) CreateTargetUIToolkit(VisualElement inspectorRoot,
            VisualElement container)
        {
            VisualElement result = UIToolkitCache.CreateDec(_propertyAttribute, _decoratorDrawerType);
            string labelName = GetFriendlyName(FieldWithInfo);

            _onSearchFieldUIToolkit.AddListener(Search);
            result.RegisterCallback<DetachFromPanelEvent>(_ => _onSearchFieldUIToolkit.RemoveListener(Search));

            return (result, false);

            void Search(string search)
            {
                DisplayStyle display = Util.UnityDefaultSimpleSearch(labelName, search)
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
                UIToolkitUtils.SetDisplayStyle(result, display);
            }
        }

    }
}
#endif

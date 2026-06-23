#if UNITY_2021_3_OR_NEWER
using SaintsField.Editor.Utils;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.Renderer.OnValueChangedCollectionFakeRenderer
{
    public partial class OnValueChangedCollectionRenderer
    {
        private HelpBox _helpBox;

        protected override (VisualElement target, bool needUpdate) CreateTargetUIToolkit(VisualElement inspectorRoot,
            VisualElement container)
        {
            return (_helpBox = new HelpBox
            {
                style =
                {
                    display = DisplayStyle.None,
                    flexGrow = 1,
                    flexShrink = 1,
                },
            }, true);
        }

        protected override PreCheckResult OnUpdateUIToolKit(VisualElement root)
        {
            PreCheckResult baseResult = base.OnUpdateUIToolKit(root);
            (bool changed, string error) = CheckCollectionLengthChanged();
            if (changed)
            {
                UIToolkitUtils.SetHelpBox(_helpBox, error);
            }

            return baseResult;
        }
    }
}
#endif

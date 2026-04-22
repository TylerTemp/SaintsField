using SaintsField.Editor.Utils;
using UnityEngine.UIElements;

namespace SaintsField.Editor.UIToolkitElements
{
#if UNITY_6000_0_OR_NEWER
    [UxmlElement]
#endif
    public partial class FancyButton: VisualElement
    {
#if !UNITY_6000_0_OR_NEWER
        public new class UxmlFactory : UxmlFactory<FancyButton, UxmlTraits> { }
#endif

        private static VisualTreeAsset _template;

        private readonly VisualElement _parameters;

        public readonly Button MainButton;
        public readonly VisualElement MainLabel;
        public readonly StatusIndicatorElement StatusIndicator;
        public readonly Button CloseButton;

        private readonly VisualElement _result;

        public FancyButton()
        {
            if (_template == null)
            {
                _template = Util.LoadResource<VisualTreeAsset>("UIToolkit/FancyButton/FancyButton.uxml");
            }

            TemplateContainer root = _template.CloneTree();
            hierarchy.Add(root);

            _parameters = root.Q<VisualElement>("parameters");

            MainButton = root.Q<Button>("mainButton");
            MainLabel = MainButton.Q<VisualElement>("mainLabel");

            CloseButton = root.Q<Button>("closeButton");

            StatusIndicator = root.Q<StatusIndicatorElement>("statusIndicatorElement");

            CloseButton.clicked += () => ShowResult(false);

            _result = root.Q<VisualElement>("result");

            ShowResult(false);
        }

        public void ShowCloseButton(bool show)
        {
            if (show)
            {
                CloseButton.style.display = DisplayStyle.Flex;
                MainButton.AddToClassList("mainButtonWithClose");
            }
            else
            {
                CloseButton.style.display = DisplayStyle.None;
                MainButton.RemoveFromClassList("mainButtonWithClose");
            }
        }

        public VisualElement HasParameters()
        {
            MainButton.AddToClassList("buttonWithParams");
            CloseButton.AddToClassList("buttonWithParams");
            return _parameters;
        }

        public VisualElement ShowResult(bool show)
        {
            if (show)
            {
                _result.style.display = DisplayStyle.Flex;
                // _result.Clear();
                MainButton.AddToClassList("buttonWithResult");
                CloseButton.AddToClassList("buttonWithResult");
            }
            else
            {
                _result.style.display = DisplayStyle.None;
                MainButton.RemoveFromClassList("buttonWithResult");
                CloseButton.RemoveFromClassList("buttonWithResult");
                _result.Clear();
            }
            ShowCloseButton(show);
            return _result;
        }

        public void ClearResult()
        {
            _result.Clear();
        }


    }
}

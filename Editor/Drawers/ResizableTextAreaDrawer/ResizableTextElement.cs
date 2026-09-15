using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.ResizableTextAreaDrawer
{
    public class ResizableTextElement: TextField
    {
        public ResizableTextElement(): this(0)
        {
        }

        public ResizableTextElement(float minHeight)
        {
            multiline = true;
            style.whiteSpace = WhiteSpace.Normal;
            // style.minHeight = MinHeight;
            style.minHeight = minHeight;
            style.marginRight = 0;

            VisualElement textInput = this.Q(name: "unity-text-input");
            if (textInput != null)
            {
                // textInput.style.minHeight = MinHeight;
                textInput.style.minHeight = minHeight;
            }
        }
    }

    public class ResizableTextField: BaseField<string>
    {
        private readonly ResizableTextElement _resizableTextElement;

        private ResizableTextField(string label, ResizableTextElement visualInput) : base(label, visualInput)
        {
            style.flexShrink = 1;

            _resizableTextElement = visualInput;
            visualInput.RegisterValueChangedCallback(evt =>
            {
                evt.StopPropagation();
                value = evt.newValue;
            });
        }

        public ResizableTextField(string label, float minHeight) : this(label, new ResizableTextElement(minHeight))
        {
        }

        public override void SetValueWithoutNotify(string newValue)
        {
            base.SetValueWithoutNotify(newValue);
            _resizableTextElement.SetValueWithoutNotify(newValue);
        }

        protected override void UpdateMixedValueContent()
        {
            _resizableTextElement.showMixedValue = showMixedValue;
        }

        public void ToHorizontal()
        {
            style.flexDirection = FlexDirection.Column;
        }
    }
}

using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class TextAreaEdit : SaintsMonoBehaviour
    {
        [ShowInInspector, ResizableTextArea] private string _textArea;

        [ShowInInspector, ResizableTextArea]
        // ReSharper disable once ConvertToAutoProperty
        private string TextArea
        {
            get => _textArea;
            set => _textArea = value;
        }

        [Button, ResizableTextArea]
        private string TestTextAreaBtn([ResizableTextArea] string text)
        {
            return "Button: " + text;
        }

        [ShowInInspector, ResizableTextArea]
        private string TestTextAreaShowInInspector([ResizableTextArea] string text)
        {
            return "ShowInInspector: " + text;
        }

    }
}

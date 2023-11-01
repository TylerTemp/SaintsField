using UnityEngine;

namespace SaintsField.Samples
{
    public class ResizableTextAreaExample : MonoBehaviour
    {
        [SerializeField, ResizableTextArea] private string _short;
        [SerializeField, ResizableTextArea] private string _long;
        [SerializeField, RichLabel(null), ResizableTextArea] private string _noLabel;

        [SerializeField, ResizableTextArea(false)] private string _inlineShort;
        [SerializeField, ResizableTextArea(false)] private string _inlineLong;
    }
}

using UnityEngine;

namespace SaintsField.Samples
{
    public class ResizableTextAreaExample : MonoBehaviour
    {
        [ResizableTextArea] public string _short;
        [ResizableTextArea] public string _long;
        [RichLabel(null), ResizableTextArea] public string _noLabel;

        // [ResizableTextArea(false)] public string _inlineShort;
        // [ResizableTextArea(false)] public string _inlineLong;
    }
}

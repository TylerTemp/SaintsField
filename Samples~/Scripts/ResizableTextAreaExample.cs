using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class ResizableTextAreaExample : MonoBehaviour
    {
        [ResizableTextArea, RichLabel("<icon=star.png /><label />")] public string _short;
        [ResizableTextArea] public string _long;
        [RichLabel(null), ResizableTextArea] public string _noLabel;

        // [ResizableTextArea(false)] public string _inlineShort;
        // [ResizableTextArea(false)] public string _inlineLong;

        [Serializable]
        public struct MyStruct
        {
            [ResizableTextArea] public string myString;
        }

        public MyStruct myStruct;
        [ResizableTextArea] public string[] arr;
        public MyStruct[] structArr;

        [ReadOnly]
        [ResizableTextArea, RichLabel("<icon=star.png /><label />")] public string shortDisabled;
    }
}

using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class ResizableTextAreaExample : MonoBehaviour
    {
        [OnValueChanged(":Debug.Log")]
        [ResizableTextArea, FieldLabelText("<icon=star.png /><label />"), BelowButton(nameof(ChangeValue))] public string shortValue;

        [ResizableTextArea(inline: true)] public string inline;

        [Tooltip("This is a long driver for people with nothing to think about")]
        [ResizableTextArea] public string _long;
        [FieldLabelText(null), ResizableTextArea] public string _noLabel;
        [FieldLabelText("long long long long long long long long long long long long long label"), ResizableTextArea] public string longLabel;

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

        [FieldReadOnly]
        [ResizableTextArea, FieldLabelText("<icon=star.png /><label />")] public string shortDisabled;

        private void ChangeValue(string oldValue)
        {
            shortValue = string.IsNullOrEmpty(oldValue) ? $"Random {UnityEngine.Random.Range(0, 10)}" : "";
        }

        [TextArea] public string defaultTextArea;

        [ResizableTextArea] public string defaultStyle;
        [ResizableTextArea(inline: true)] public string inlineStyle;
        [ResizableTextArea(minRow: 3)] public string default3;
        [ResizableTextArea(inline: true, minRow: 3)] public string inline3;
    }
}

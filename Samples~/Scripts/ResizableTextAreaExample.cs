using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class ResizableTextAreaExample : MonoBehaviour
    {
        [ResizableTextArea, FieldRichLabel("<icon=star.png /><label />"), BelowButton(nameof(ChangeValue))] public string shortValue;
        [ResizableTextArea] public string _long;
        [FieldRichLabel(null), ResizableTextArea] public string _noLabel;
        [FieldRichLabel("long long long long long long long long long long long long long label"), ResizableTextArea] public string longLabel;

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
        [ResizableTextArea, FieldRichLabel("<icon=star.png /><label />")] public string shortDisabled;

        private void ChangeValue(string oldValue)
        {
            shortValue = string.IsNullOrEmpty(oldValue) ? $"Randome {UnityEngine.Random.Range(0, 10)}" : "";
        }
    }
}

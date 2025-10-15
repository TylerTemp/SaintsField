using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class ResizableTextAreaExample : MonoBehaviour
    {
        [ResizableTextArea, FieldLabelText("<icon=star.png /><label />"), BelowButton(nameof(ChangeValue))] public string shortValue;
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

        [ReadOnly]
        [ResizableTextArea, FieldLabelText("<icon=star.png /><label />")] public string shortDisabled;

        private void ChangeValue(string oldValue)
        {
            shortValue = string.IsNullOrEmpty(oldValue) ? $"Randome {UnityEngine.Random.Range(0, 10)}" : "";
        }
    }
}

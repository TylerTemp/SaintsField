using System.Diagnostics;
using SaintsField.Utils;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false)]
    public class DrawLabelAttribute : PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy => "";

        public readonly EColor EColor;
        public readonly string Content;
        public bool IsCallback;

        public DrawLabelAttribute(EColor eColor, string content, bool isCallback = false)
        {
            EColor = eColor;
            (string parsedContent, bool parsedIsCallback) = RuntimeUtil.ParseCallback(content, isCallback);
            Content = parsedContent;
            IsCallback = parsedIsCallback;
        }

        public DrawLabelAttribute(string content, bool isCallback = false): this(EColor.White, content, isCallback)
        {
        }
    }
}

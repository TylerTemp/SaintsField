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
        public readonly bool IsCallback;

        public readonly Space Space;

        public DrawLabelAttribute(EColor eColor, string content, bool isCallback = false, Space space = Space.World)
        {
            EColor = eColor;
            (string parsedContent, bool parsedIsCallback) = RuntimeUtil.ParseCallback(content, isCallback);
            Content = parsedContent;
            IsCallback = parsedIsCallback;
            Space = space;
        }

        public DrawLabelAttribute(string content, bool isCallback = false, Space space = Space.World): this(EColor.White, content, isCallback, space)
        {
        }
    }
}

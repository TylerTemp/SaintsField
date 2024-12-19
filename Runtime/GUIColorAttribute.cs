using System;
using SaintsField.Utils;
using UnityEngine;

namespace SaintsField
{
    public class GUIColorAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy => "";

        public readonly bool IsCallback;
        public readonly string Callback;

        public readonly Color Color;

        public GUIColorAttribute(float r, float g, float b, float a = 1f)
        {
            Color = new Color(r, g, b, a);
        }

        public GUIColorAttribute(string hexColor)
        {
            if (hexColor.StartsWith("$"))
            {
                IsCallback = true;
                Callback = RuntimeUtil.ParseCallback(hexColor).content;
            }
            else
            {
                if (!ColorUtility.TryParseHtmlString(hexColor, out Color color))
                {
                    throw new ArgumentOutOfRangeException(nameof(hexColor), hexColor, $"Not a html hex color string");
                }

                Color = color;
            }
        }

        public GUIColorAttribute(EColor eColor)
        {
            Color = eColor.GetColor();
        }
    }
}

using System;
using System.Diagnostics;
using SaintsField.Interfaces;
using SaintsField.Playa;
using SaintsField.Utils;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    public class GUIColorAttribute: PropertyAttribute, ISaintsAttribute, IPlayaAttribute
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
            if (hexColor.StartsWith("#"))
            {
                if (!ColorUtility.TryParseHtmlString(hexColor, out Color color))
                {
                    throw new ArgumentOutOfRangeException(nameof(hexColor), hexColor, "Not a html hex color string");
                }

                Color = color;
            }
            else
            {
                IsCallback = true;
                Callback = RuntimeUtil.ParseCallback(hexColor).content;
            }
        }

        public GUIColorAttribute(EColor eColor, float alpha = 1f)
        {
            Color c = eColor.GetColor();
            c.a = alpha;
            Color = c;
        }
    }
}

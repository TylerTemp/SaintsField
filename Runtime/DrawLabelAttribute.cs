using System;
using System.Diagnostics;
using SaintsField.Interfaces;
using SaintsField.Utils;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class DrawLabelAttribute : PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy => "";

        public readonly Color Color;
        public readonly string ColorCallback;
        public readonly string Content;
        public readonly bool IsCallback;

        public readonly string Space;

        public DrawLabelAttribute(EColor eColor, string content = null, string space = "this", float alpha = 1f, string color = null)
        {
            (string parsedContent, bool parsedIsCallback) = RuntimeUtil.ParseCallback(content);
            Content = parsedContent;
            IsCallback = parsedIsCallback;
            Space = space;

            Color = eColor.GetColor();
            if (alpha < 1f)
            {
                Color.a = alpha;
            }
            ColorCallback = null;

            bool colorIsString = !string.IsNullOrEmpty(color);
            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if(colorIsString && color.StartsWith("#"))
            {
                bool isColor = ColorUtility.TryParseHtmlString(color, out Color colorObj);
                if (!isColor)
                {
                    throw new Exception($"Color {color} is not a valid color");
                }
                Color = colorObj;
            }
            else if(colorIsString)
            {
                (string colorContent, bool _) = RuntimeUtil.ParseCallback(color);
                ColorCallback = colorContent;
            }
        }

        public DrawLabelAttribute(string content = null, string space = "this", float alpha=1f, string color = null): this(EColor.White, content, space, alpha, color)
        {
        }
    }
}

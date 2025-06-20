using System;
using SaintsField.Interfaces;
using SaintsField.Utils;
using UnityEngine;

namespace SaintsField
{
    public abstract class OneDirectionBaseAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy => "";

        public readonly string Start;
        public readonly int StartIndex;
        public readonly string StartSpace;
        public readonly string End;
        public readonly int EndIndex;
        public readonly string EndSpace;
        public readonly Color Color;
        public readonly string ColorCallback;
        public readonly float Dotted;

        protected OneDirectionBaseAttribute(
            string start = null, int startIndex = 0, string startSpace = "this",
            string end = null, int endIndex = 0, string endSpace = "this",
            EColor eColor = EColor.White, float alpha = 1f, string color = null,
            float dotted = -1f
        )
        {
            Start = start;
            StartIndex = startIndex;
            StartSpace = startSpace;
            End = end;
            EndIndex = endIndex;
            EndSpace = endSpace;

            Dotted = dotted;

            Color = eColor.GetColor();
            if (alpha < 1f)
            {
                Color.a = alpha;
            }

            bool colorIsString = !string.IsNullOrEmpty(color);
            ColorCallback = null;

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
                // ColorIsCallback = true;
                ColorCallback = colorContent;
            }
        }
    }
}

using System;
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

        protected OneDirectionBaseAttribute(
            string start = null, int startIndex = 0, string startSpace = "this",
            string end = null, int endIndex = 0, string endSpace = "this",
            EColor eColor = EColor.White, string colorCallback = null
        )
        {
            Start = start;
            StartIndex = startIndex;
            StartSpace = startSpace;
            End = end;
            EndIndex = endIndex;
            EndSpace = endSpace;

            Color = eColor.GetColor();

            bool colorIsString = !string.IsNullOrEmpty(colorCallback);
            ColorCallback = null;

            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if(colorIsString && colorCallback.StartsWith("#"))
            {
                bool isColor = ColorUtility.TryParseHtmlString(colorCallback, out Color color);
                if (!isColor)
                {
                    throw new Exception($"Color {colorCallback} is not a valid color");
                }
                Color = color;
            }
            else if(colorIsString)
            {
                (string colorContent, bool _) = RuntimeUtil.ParseCallback(colorCallback);
                // ColorIsCallback = true;
                ColorCallback = colorContent;
            }
        }
    }
}

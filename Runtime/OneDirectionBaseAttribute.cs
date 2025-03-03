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

        protected OneDirectionBaseAttribute(
            string start = null, int startIndex = 0, string startSpace = "this",
            string end = null, int endIndex = 0, string endSpace = "this",
            EColor eColor = EColor.White, string color = null
        )
        {
            Start = start;
            StartIndex = startIndex;
            StartSpace = startSpace;
            End = end;
            EndIndex = endIndex;
            EndSpace = endSpace;

            Color = eColor.GetColor();

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

using System;
using System.Diagnostics;
using SaintsField.Interfaces;
using SaintsField.Utils;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class SliderHandleAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy => string.Empty;

        public readonly string Space;

        public readonly Vector3 Direction;
        public readonly string DirectionCallback;

        public readonly float Size;
        public readonly float Snap;

        public readonly Vector3 PosOffset;
        public readonly string PosOffsetCallback;

        public readonly Color Color;
        public readonly string ColorCallback;

        public SliderHandleAttribute(
            string space = "this",
            float directionX = 1f, float directionY = 0f, float directionZ = 0f, string directionCallback = null,
            float size = -1f,
            float snap = -1f,
            float posXOffset = 0f, float posYOffset = 0f, float posZOffset = 0f, string posOffsetCallback = null,
            EColor eColor = EColor.White, float alpha = 1f, string color = null
        )
        {
            Space = space;

            Direction = new Vector3(directionX, directionY, directionZ);
            DirectionCallback = RuntimeUtil.ParseCallback(directionCallback).content;

            Size = size;
            Snap = snap;

            PosOffset = new Vector3(posXOffset, posYOffset, posZOffset);
            PosOffsetCallback = RuntimeUtil.ParseCallback(posOffsetCallback).content;

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

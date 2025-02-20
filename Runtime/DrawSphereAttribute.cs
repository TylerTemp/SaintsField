using System;
using System.Diagnostics;
using SaintsField.Utils;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class DrawSphereAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy => "";

        public readonly float Radius;
        public readonly string RadiusCallback;

        public readonly string Space;

        public readonly Vector3 PosOffset;
        public readonly string PosOffsetCallback;

        public readonly Color Color;
        public readonly string ColorCallback;

        public DrawSphereAttribute(
            float radius = 1f, string radisCallback = null,
            string space = "this",
            float posXOffset = 0f, float posYOffset = 0f, float posZOffset = 0f, string posOffsetCallback = null,
            EColor eColor = EColor.White, string colorCallback = null
        )
        {
            Radius = radius;
            RadiusCallback = radisCallback;
            Space = space;
            PosOffset = new Vector3(posXOffset, posYOffset, posZOffset);
            PosOffsetCallback = posOffsetCallback;

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

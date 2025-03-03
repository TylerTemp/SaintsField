using System;
using System.Diagnostics;
using SaintsField.Interfaces;
using SaintsField.Utils;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class SphereHandleCapAttribute: PropertyAttribute, ISaintsAttribute
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

        public SphereHandleCapAttribute(
            float radius = 1f, string radiusCallback = null,
            string space = "this",
            float posXOffset = 0f, float posYOffset = 0f, float posZOffset = 0f, string posOffsetCallback = null,
            EColor eColor = EColor.White, string color = null
        )
        {
            Radius = radius;
            RadiusCallback = radiusCallback;
            Space = space;
            PosOffset = new Vector3(posXOffset, posYOffset, posZOffset);
            PosOffsetCallback = posOffsetCallback;

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

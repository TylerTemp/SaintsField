using System;
using ExtInspector.Utils;
using UnityEditor;
using UnityEngine;

namespace ExtInspector.Standalone
{
    [AttributeUsage(AttributeTargets.Field)]
    public class LabelTextAttribute: PropertyAttribute
    {
        public readonly string text;
        public readonly EColor textColor;
        public readonly string icon;
        public readonly EColor iconColor;
        public readonly int iconWidth;

        // iconWidth=-2: fixed default width
        // iconWidth=-1: dynamic auto width
        // other: width
        public LabelTextAttribute(string text, EColor textColor = EColor.Default, string icon = null, EColor iconColor = EColor.White, int iconWidth=-2)
        {
            this.text = text;
            this.textColor = textColor;
            this.icon = icon;
            this.iconColor = iconColor;
            this.iconWidth = iconWidth == -2
                ? Mathf.FloorToInt(EditorGUIUtility.singleLineHeight)
                : iconWidth;
        }

        // public LabelTextAttribute(string text, EColor textColor = EColor.Default, string icon = null,
        //     EColor iconColor = EColor.White, bool dynamicIconWidth = false)
        // {
        //     this.text = text;
        //     this.textColor = textColor;
        //     this.icon = icon;
        //     this.iconColor = iconColor;
        //     this.iconWidth = dynamicIconWidth;
        // }
    }
}

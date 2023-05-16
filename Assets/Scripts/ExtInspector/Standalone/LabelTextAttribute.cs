using System;
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

        public LabelTextAttribute(string text, EColor textColor = EColor.Default, string icon = null, EColor iconColor = EColor.White)
        {
            this.text = text;
            this.textColor = textColor;
            this.icon = icon;
            this.iconColor = iconColor;
        }
    }
}

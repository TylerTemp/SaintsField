using ExtInspector.Standalone;
using ExtInspector.Utils;
using UnityEngine;

namespace ExtInspector
{
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = true)]
    public class LabelSuffixAttribute : PropertyAttribute, IPostDecorator
    {
        public string DrawerClassName => "ExtInspector.Editor.LabelSuffixAttributeDrawer";

        public readonly string text;
        public readonly EColor textColor;
        public readonly string icon;
        public readonly EColor iconColor;

        public LabelSuffixAttribute(string text, EColor textColor = EColor.Default, string icon = null, EColor iconColor = EColor.White)
        {
            this.text = text;
            this.textColor = textColor;
            this.icon = icon;
            this.iconColor = iconColor;
        }
    }
}

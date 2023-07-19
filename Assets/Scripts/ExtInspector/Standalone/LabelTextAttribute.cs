using System;
using ExtInspector.Utils;
using UnityEditor;
using UnityEngine;

namespace ExtInspector.Standalone
{
    [AttributeUsage(AttributeTargets.Field)]
    public class LabelTextAttribute: PropertyAttribute
    {
        public readonly bool useOldLabel;
        public readonly string text;
        public readonly EColor textColor;
        public readonly string icon;
        public readonly EColor iconColor;
        public readonly int iconWidth;

        // iconWidth=-2: fixed default width
        // iconWidth=-1: dynamic auto width
        // other: width
        public LabelTextAttribute(string text = null, EColor textColor = EColor.Default, bool useOldLabel = false, string icon = null, EColor iconColor = EColor.White, int iconWidth=-2)
        {
            if (useOldLabel)  // 使用原标签=>原标签+[可选]icon
            {
                // 则不得配置新标签
                Debug.Assert(text is null);
                // Debug.Assert(icon is null);
            }
            else if (icon is not null)  // 不使用原标签，没有icon=>单纯使用新标签
            {
                // 则自定义标签不得为 null
                Debug.Assert(text is not null);
            }

            this.useOldLabel = useOldLabel;
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

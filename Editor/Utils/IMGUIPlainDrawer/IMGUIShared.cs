using System;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Utils.IMGUIPlainDrawer
{
    public static class IMGUIShared
    {
        private static float SingleLineHeight => EditorGUIUtility.singleLineHeight;
        private const float VerticalPadding = 1f;

        public static float GetSingleLineHeight(bool inHorizontalLayout) =>
            SingleLineHeight * (inHorizontalLayout ? 2 : 1) + VerticalPadding * 2;

        public static float GetResponsiveMultiLineHeight(bool inHorizontalLayout, int narrowRows) =>
            SingleLineHeight * ((inHorizontalLayout || !IMGUIRawDraw.UseWideMode()) ? narrowRows : 1) +
            VerticalPadding * 2;

        public static Rect GetContentRect(Rect position) => new Rect(position)
        {
            y = position.y + VerticalPadding,
            height = Mathf.Max(0f, position.height - VerticalPadding * 2),
        };

        public static (Rect labelRect, Rect fieldRect) GetStackedRects(Rect position)
        {
            Rect contentRect = GetContentRect(position);

            Rect labelRect = new Rect(contentRect)
            {
                height = SingleLineHeight,
            };

            Rect fieldRect = new Rect(contentRect)
            {
                y = contentRect.y + SingleLineHeight,
                height = SingleLineHeight,
            };

            return (labelRect, fieldRect);
        }

        public static T WithLabelColor<T>(bool labelGrayColor, Func<T> draw)
        {
            using(new LabelColorScoop(labelGrayColor))
            {
                return draw();
            }
        }

        public static void WithLabelColor(bool labelGrayColor, Action draw)
        {
            WithLabelColor(labelGrayColor, () =>
            {
                draw();
                return 0;
            });
        }

        public static T DrawStackedField<T>(Rect position, GUIContent label, bool inHorizontalLayout, bool labelGrayColor, Func<Rect, GUIContent, T> wideDrawer, Func<Rect, T> stackedDrawer)
        {
            Rect contentRect = GetContentRect(position);
            if (!inHorizontalLayout)
            {
                return WithLabelColor(labelGrayColor, () => wideDrawer(contentRect, label));
            }

            (Rect labelRect, Rect fieldRect) = GetStackedRects(position);
            WithLabelColor(labelGrayColor, () => EditorGUI.HandlePrefixLabel(contentRect, labelRect, label, 0));
            return stackedDrawer(fieldRect);
        }
    }
}

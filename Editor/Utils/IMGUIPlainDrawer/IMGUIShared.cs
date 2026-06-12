using System;
using SaintsField.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Utils.IMGUIPlainDrawer
{
    public static class IMGUIShared
    {
        private static float SingleLineHeight => SaintsPropertyDrawer.SingleLineHeight;

        public static float GetSingleLineHeight(bool inHorizontalLayout) => SingleLineHeight * (inHorizontalLayout ? 2 : 1);

        public static float GetResponsiveMultiLineHeight(bool inHorizontalLayout, int narrowRows) =>
            SingleLineHeight * ((inHorizontalLayout || !IMGUIUtils.UseWideMode()) ? narrowRows : 1);

        public static (Rect labelRect, Rect fieldRect) GetStackedRects(Rect position)
        {
            Rect labelRect = new Rect(position)
            {
                height = SingleLineHeight,
            };

            Rect fieldRect = new Rect(position)
            {
                y = position.y + SingleLineHeight,
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
            if (!inHorizontalLayout)
            {
                return WithLabelColor(labelGrayColor, () => wideDrawer(position, label));
            }

            (Rect labelRect, Rect fieldRect) = GetStackedRects(position);
            WithLabelColor(labelGrayColor, () => EditorGUI.HandlePrefixLabel(position, labelRect, label, 0));
            return stackedDrawer(fieldRect);
        }
    }
}

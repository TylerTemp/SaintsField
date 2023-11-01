using UnityEngine;

namespace SaintsField.Editor.Utils
{
    public static class RectUtils
    {
        public static (Rect curRect, Rect leftRect) SplitHeightRect(Rect targetRect, float height)
        {
            Rect curRect = new Rect(targetRect)
            {
                height = height,
            };

            Rect leftRect = new Rect(targetRect)
            {
                y = curRect.y + curRect.height,
                height = targetRect.height - height,
            };

            return (
                curRect,
                leftRect
            );
        }

        public static (Rect curRect, Rect leftRect) SplitWidthRect(Rect targetRect, float width)
        {
            Rect curRect = new Rect(targetRect)
            {
                width = width,
            };

            Rect leftRect = new Rect(targetRect)
            {
                x = curRect.x + curRect.width,
                width = targetRect.width - width,
            };

            return (
                curRect,
                leftRect
            );
        }
    }
}

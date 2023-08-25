using UnityEngine;

namespace ExtInspector.Editor.Utils
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
    }
}

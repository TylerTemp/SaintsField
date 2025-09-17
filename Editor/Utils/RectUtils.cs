using System;
using System.Collections.Generic;
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
            float totalWidth = targetRect.width;
            if (totalWidth <= 0)
            {
                Rect zeroRect = new Rect(targetRect)
                {
                    width = 0,
                };
                return (zeroRect, zeroRect);
            }

            float canUseWidth = Mathf.Min(totalWidth, width);

            Rect curRect = new Rect(targetRect)
            {
                width = canUseWidth,
            };

            Rect leftRect = new Rect(targetRect)
            {
                x = curRect.x + curRect.width,
                width = targetRect.width - canUseWidth,
            };

            return (
                curRect,
                leftRect
            );
        }

        public static IEnumerable<Type> GetGenBaseTypes(Type type)
        {
            if (type.IsGenericType)
            {
                yield return type;
            }

            Type lastType = type;
            while (true)
            {
                Type baseType = lastType.BaseType;
                if (baseType == null)
                {
                    yield break;
                }

                if (baseType.IsGenericType)
                {
                    yield return baseType;
                }

                lastType = baseType;
            }
        }
    }
}

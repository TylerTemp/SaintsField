// ReSharper disable once RedundantUsingDirective
using System;
// ReSharper disable once RedundantUsingDirective
using UnityEditor;
using System.Collections.Generic;
using SaintsField.Editor.Core;
using UnityEngine;

namespace SaintsField.Editor.HeaderGUI
{
    public static class CacheAndUtil
    {
        private static RichTextDrawer _richTextDrawer;

        public static RichTextDrawer GetCachedRichTextDrawer()
        {
            // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
            if (_richTextDrawer is null)
            {
                _richTextDrawer = new RichTextDrawer();
            }

            return _richTextDrawer;
        }

        public static readonly Dictionary<string, IReadOnlyList<RichTextDrawer.RichTextChunk>> ParsedXmlCache = new Dictionary<string, IReadOnlyList<RichTextDrawer.RichTextChunk>>();

#if !UNITY_2021_3_OR_NEWER
        private static Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i)
            {
                pix[i] = col;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
        private static GUIStyle _iconButtonStyleFallback;
#endif
        public static GUIStyle GetIconButtonStyle()
        {
#if UNITY_2021_3_OR_NEWER
            return EditorStyles.iconButton;
#else

            // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
            if(_iconButtonStyleFallback == null)
            {
                _iconButtonStyleFallback = new GUIStyle(GUI.skin.button)
                {
                    normal =
                    {
                        background = MakeTex(2, 2, Color.clear),
                        scaledBackgrounds = new Texture2D[] { },
                    },
                    active =
                    {
                        scaledBackgrounds = new Texture2D[] { },
                    },
                    hover =
                    {
                        background = MakeTex(2, 2, EColor.EditorButtonHover.GetColor()),
                        scaledBackgrounds = new Texture2D[] { },
                    },
                    onNormal =
                    {
                        scaledBackgrounds = Array.Empty<Texture2D>(),
                    },
                    onActive =
                    {
                        scaledBackgrounds = Array.Empty<Texture2D>(),
                    },
                    border = new RectOffset(0, 0, 0, 0),
                };
            }

            return _iconButtonStyleFallback;
#endif

        }
    }
}

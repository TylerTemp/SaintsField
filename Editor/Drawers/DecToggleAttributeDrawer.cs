using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers
{
    public abstract class DecToggleAttributeDrawer: SaintsPropertyDrawer
    {
        protected string error = "";

        // ReSharper disable once InconsistentNaming
        protected readonly RichTextDrawer RichTextDrawer = new RichTextDrawer();

        protected Rect Draw(Rect position, SerializedProperty property, GUIContent label, string labelXml, bool isActive, Action<bool> activeCallback, FieldInfo info, object target)
        {
            (Rect buttonRect, Rect leftRect) = RectUtils.SplitHeightRect(position, EditorGUIUtility.singleLineHeight);

            GUIStyle style = new GUIStyle("Button")
            {
                // fixedWidth = btnWidth,
            };

            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                bool newIsActive = GUI.Toggle(position, isActive, "", style);
                if (changed.changed)
                {
                    activeCallback(newIsActive);
                }
            }

            IReadOnlyList<RichTextDrawer.RichTextChunk> richChunks =
                RichTextDrawer.ParseRichXmlWithProvider(labelXml, this).ToArray();
            float textWidth = RichTextDrawer.GetWidth(label, buttonRect.height, richChunks);
            Rect labelRect = buttonRect;
            if (textWidth < labelRect.width)
            {
                float space = (labelRect.width - textWidth) / 2f;
                labelRect.x += space;
            }

            RichTextDrawer.DrawChunks(labelRect, richChunks);

            return leftRect;
        }
    }
}

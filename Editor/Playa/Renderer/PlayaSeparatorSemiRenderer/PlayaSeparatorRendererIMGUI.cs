using System;
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Playa.Renderer.PlayaSeparatorSemiRenderer
{
    public partial class PlayaSeparatorRenderer
    {
        private const float SeparatorMarginLeftIMGUI = 4f;
        private const float SeparatorGapIMGUI = 2f;
        private const float SeparatorLineHeightIMGUI = 2f;
        private const float SeparatorLineOnlyHeightIMGUI = 4f;

        private RichTextDrawer _richTextDrawerIMGUI;
        private string _richXmlIMGUI;
        private RichTextDrawer.RichTextChunk[] _richTextChunksIMGUI = Array.Empty<RichTextDrawer.RichTextChunk>();

        protected override float GetFieldHeightIMGUI(float width, PreCheckResult preCheckResult)
        {
            if (!preCheckResult.IsShown)
            {
                return 0f;
            }

            (string error, string richXml) = RefreshSeparatorTitleIMGUI();
            if (error != "")
            {
                return ImGuiHelpBox.GetHeight(error, width, MessageType.Error);
            }

            return GetSeparatorHeightIMGUI(richXml);
        }

        protected override void RenderPositionTargetIMGUI(Rect position, PreCheckResult preCheckResult)
        {
            if (!preCheckResult.IsShown)
            {
                return;
            }

            (string error, string richXml) = RefreshSeparatorTitleIMGUI();
            if (error != "")
            {
                ImGuiHelpBox.Draw(position, error, MessageType.Error);
                return;
            }

            DrawSeparatorIMGUI(position, richXml);
        }

        private RichTextDrawer GetRichTextDrawerIMGUI()
        {
            return _richTextDrawerIMGUI ??= new RichTextDrawer();
        }

        private (string error, string richXml) RefreshSeparatorTitleIMGUI()
        {
            (string error, string richXml) = GetSeparatorRichXml();
            if (error != "")
            {
                return (error, richXml);
            }

            if (richXml != _richXmlIMGUI || (richXml != null && richXml.Contains("<field")))
            {
                _richXmlIMGUI = richXml;
                _richTextChunksIMGUI = string.IsNullOrEmpty(richXml)
                    ? Array.Empty<RichTextDrawer.RichTextChunk>()
                    : RichTextDrawer.ParseRichXmlWithProvider(richXml, this).ToArray();
            }

            return ("", _richXmlIMGUI);
        }

        private float GetSeparatorHeightIMGUI(string richXml)
        {
            int space = Mathf.Max(0, _playaSeparatorAttribute.Space);
            float height = space;

            if (_playaSeparatorAttribute.Title != null)
            {
                height += string.IsNullOrEmpty(richXml)
                    ? SeparatorLineOnlyHeightIMGUI
                    : EditorGUIUtility.singleLineHeight;
            }
            else if (space <= 0)
            {
                height += SeparatorLineOnlyHeightIMGUI;
            }

            return height;
        }

        private void DrawSeparatorIMGUI(Rect position, string richXml)
        {
            Rect contentRect = new Rect(position)
            {
                x = position.x + SeparatorMarginLeftIMGUI,
                width = Mathf.Max(0f, position.width - SeparatorMarginLeftIMGUI),
            };

            int space = Mathf.Max(0, _playaSeparatorAttribute.Space);
            if (space > 0 && !_playaSeparatorAttribute.Below)
            {
                (_, contentRect) = RectUtils.SplitHeightRect(contentRect, space);
            }

            if (_playaSeparatorAttribute.Title != null)
            {
                float rowHeight = string.IsNullOrEmpty(richXml)
                    ? SeparatorLineOnlyHeightIMGUI
                    : EditorGUIUtility.singleLineHeight;
                (Rect rowRect, _) = RectUtils.SplitHeightRect(contentRect, rowHeight);
                DrawSeparatorTitleRowIMGUI(rowRect, richXml);
            }
            else if (space <= 0)
            {
                DrawSeparatorLineIMGUI(contentRect);
            }
        }

        private void DrawSeparatorTitleRowIMGUI(Rect rowRect, string richXml)
        {
            if (string.IsNullOrEmpty(richXml))
            {
                DrawSeparatorLineIMGUI(rowRect);
                return;
            }

            RichTextDrawer.RichTextChunk[] chunks =
                _richTextChunksIMGUI ?? Array.Empty<RichTextDrawer.RichTextChunk>();
            float textWidth = GetRichTextDrawerIMGUI().GetWidth(new GUIContent(GetLabel()),
                EditorGUIUtility.singleLineHeight, chunks);

            Rect titleRect = new Rect(rowRect)
            {
                width = Mathf.Min(rowRect.width, textWidth),
            };

            switch (_playaSeparatorAttribute.EAlign)
            {
                case EAlign.FieldStart:
                case EAlign.Start:
                {
                    Rect rightLineRect = new Rect(rowRect)
                    {
                        x = rowRect.x + textWidth + SeparatorGapIMGUI,
                        width = rowRect.width - textWidth - SeparatorGapIMGUI,
                    };
                    DrawSeparatorLineIMGUI(rightLineRect);
                }
                    break;
                case EAlign.Center:
                {
                    if (textWidth + SeparatorGapIMGUI * 2f < rowRect.width)
                    {
                        float barWidth = (rowRect.width - textWidth - SeparatorGapIMGUI * 2f) / 2f;
                        Rect leftLineRect = new Rect(rowRect)
                        {
                            width = barWidth,
                        };
                        Rect rightLineRect = new Rect(rowRect)
                        {
                            x = rowRect.x + rowRect.width - barWidth,
                            width = barWidth,
                        };
                        titleRect.x = rowRect.x + barWidth + SeparatorGapIMGUI;
                        titleRect.width = textWidth;
                        DrawSeparatorLineIMGUI(leftLineRect);
                        DrawSeparatorLineIMGUI(rightLineRect);
                    }
                    else
                    {
                        titleRect.width = rowRect.width;
                    }
                }
                    break;
                case EAlign.End:
                {
                    float barWidth = rowRect.width - textWidth - SeparatorGapIMGUI;
                    if (barWidth > 0f)
                    {
                        Rect leftLineRect = new Rect(rowRect)
                        {
                            width = barWidth,
                        };
                        titleRect.x = rowRect.x + barWidth + SeparatorGapIMGUI;
                        titleRect.width = textWidth;
                        DrawSeparatorLineIMGUI(leftLineRect);
                    }
                    else
                    {
                        titleRect.width = rowRect.width;
                    }
                }
                    break;
            }

            GetRichTextDrawerIMGUI().DrawChunks(titleRect, chunks);
        }

        private void DrawSeparatorLineIMGUI(Rect rect)
        {
            if (rect.width <= 0f || rect.height <= 0f)
            {
                return;
            }

            float height = Mathf.Min(rect.height, SeparatorLineHeightIMGUI);
            Rect lineRect = new Rect(rect)
            {
                y = rect.y + (rect.height - height) / 2f,
                height = height,
            };
            EditorGUI.DrawRect(lineRect, _playaSeparatorAttribute.Color);
        }
    }
}

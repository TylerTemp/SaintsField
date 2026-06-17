using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.ValueButtonsDrawer
{
    public partial class ValueButtonsAttributeDrawer
    {
        private sealed class InfoIMGUI
        {
            public string Error = "";
            public readonly RichTextDrawer RichTextDrawer = new RichTextDrawer();
        }

        internal sealed class ImGuiButtonInfo
        {
            public ValueButtonRawInfo RawInfo;
            public float Width;
            public float ContentWidth;
        }

        internal sealed class ImGuiButtonLayout
        {
            public readonly IReadOnlyList<IReadOnlyList<ImGuiButtonInfo>> Rows;
            public readonly float MainAvailableWidth;
            public readonly float SubAvailableWidth;

            public ImGuiButtonLayout(IReadOnlyList<IReadOnlyList<ImGuiButtonInfo>> rows, float mainAvailableWidth,
                float subAvailableWidth)
            {
                Rows = rows;
                MainAvailableWidth = mainAvailableWidth;
                SubAvailableWidth = subAvailableWidth;
            }

            public bool HasSubRow => Rows.Count > 1;
        }

        private static readonly Dictionary<string, InfoIMGUI> InfoCacheIMGUI =
            new Dictionary<string, InfoIMGUI>();

        private static GUIStyle _miniButtonStyle;
        private static Texture2D _foldoutCollapsedTexture;
        private static Texture2D _foldoutExpandedTexture;

        internal const float ExpandButtonWidth = 18f;

        private static GUIStyle GetMiniButtonStyle()
        {
            if (_miniButtonStyle != null)
            {
                return _miniButtonStyle;
            }

            _miniButtonStyle = new GUIStyle(EditorStyles.miniButton)
            {
                alignment = TextAnchor.MiddleCenter,
            };
            return _miniButtonStyle;
        }

        private static InfoIMGUI EnsureKey(SerializedProperty property)
        {
            string key = SerializedUtils.GetUniqueId(property);
            if (InfoCacheIMGUI.TryGetValue(key, out InfoIMGUI cache))
            {
                return cache;
            }

            InfoCacheIMGUI[key] = cache = new InfoIMGUI();
            NoLongerInspectingWatch(property.serializedObject.targetObject, key, () => InfoCacheIMGUI.Remove(key));
            return cache;
        }

        internal static float UtilGetFieldInputWidth(float width, GUIContent label)
        {
            if (label == null || string.IsNullOrEmpty(label.text))
            {
                return width;
            }

            return Mathf.Max(1f, width - EditorGUIUtility.labelWidth);
        }

        internal static Rect UtilDrawFoldout(Rect position, SerializedProperty property)
        {
            Rect foldoutRect = new Rect(position)
            {
                width = ExpandButtonWidth,
            };
            _foldoutCollapsedTexture ??= Util.LoadResource<Texture2D>("IN foldout");
            _foldoutExpandedTexture ??= Util.LoadResource<Texture2D>("IN foldout on");

            Texture2D icon = property.isExpanded ? _foldoutExpandedTexture : _foldoutCollapsedTexture;
            if (GUI.Button(foldoutRect, icon, GUIStyle.none))
            {
                property.isExpanded = !property.isExpanded;
            }

            return new Rect(position)
            {
                x = foldoutRect.xMax,
                width = Mathf.Max(0f, position.width - ExpandButtonWidth),
            };
        }

        internal static ValueButtonRawInfo[] UtilMakeButtonRawInfos(AdvancedDropdownMetaInfo metaInfo,
            IRichTextTagProvider richTextTagProvider)
        {
            if (metaInfo.Error != "" || metaInfo.DropdownListValue == null)
            {
                return Array.Empty<ValueButtonRawInfo>();
            }

            return metaInfo.DropdownListValue
                .Select(each => new ValueButtonRawInfo(
                    RichTextDrawer.ParseRichXmlWithProvider(each.displayName, richTextTagProvider).ToArray(),
                    each.disabled,
                    each.value))
                .ToArray();
        }

        internal static ImGuiButtonLayout UtilGetButtonLayout(float mainWidth, float subWidth, bool greedy,
            IReadOnlyList<ValueButtonRawInfo> rawInfos, RichTextDrawer richTextDrawer)
        {
            float mainAvailableWidth = Mathf.Max(1f, mainWidth - ExpandButtonWidth);
            float subAvailableWidth = Mathf.Max(1f, subWidth);

            if (rawInfos == null || rawInfos.Count == 0)
            {
                return new ImGuiButtonLayout(Array.Empty<IReadOnlyList<ImGuiButtonInfo>>(), mainAvailableWidth,
                    subAvailableWidth);
            }

            GUIStyle miniButtonStyle = GetMiniButtonStyle();
            List<ImGuiButtonInfo> buttonInfos = new List<ImGuiButtonInfo>(rawInfos.Count);
            foreach (ValueButtonRawInfo rawInfo in rawInfos)
            {
                float contentWidth = richTextDrawer.GetWidth(GUIContent.none, EditorGUIUtility.singleLineHeight,
                    rawInfo.DisplayChunks);
                float width = Mathf.Max(EditorGUIUtility.singleLineHeight,
                    contentWidth + miniButtonStyle.padding.left + miniButtonStyle.padding.right + 8f);

                buttonInfos.Add(new ImGuiButtonInfo
                {
                    RawInfo = rawInfo,
                    Width = width,
                    ContentWidth = contentWidth,
                });
            }

            List<List<ImGuiButtonInfo>> rows = greedy
                ? SplitRowsGreedy(buttonInfos, mainAvailableWidth, subAvailableWidth)
                : SplitRowsBalanced(buttonInfos, mainAvailableWidth, subAvailableWidth);

            return new ImGuiButtonLayout(rows.Cast<IReadOnlyList<ImGuiButtonInfo>>().ToArray(), mainAvailableWidth,
                subAvailableWidth);
        }

        internal static float UtilGetBelowHeight(float width, bool drawSubRows, string error, ImGuiButtonLayout layout)
        {
            float result = 0f;
            if (drawSubRows && layout != null && layout.HasSubRow)
            {
                result += (layout.Rows.Count - 1) * EditorGUIUtility.singleLineHeight;
            }

            if (error != "")
            {
                result += ImGuiHelpBox.GetHeight(error, width, MessageType.Error);
            }

            return result;
        }

        internal static Rect UtilDrawBelow(Rect position, bool drawSubRows, string error, ImGuiButtonLayout layout,
            RichTextDrawer richTextDrawer, Func<ValueButtonRawInfo, bool> isOnFunc,
            Action<ValueButtonRawInfo> onClick)
        {
            Rect leftRect = position;

            if (drawSubRows && layout != null && layout.HasSubRow)
            {
                for (int rowIndex = 1; rowIndex < layout.Rows.Count; rowIndex++)
                {
                    (Rect rowRect, Rect nextRect) = RectUtils.SplitHeightRect(leftRect,
                        EditorGUIUtility.singleLineHeight);
                    UtilDrawButtonRow(rowRect, layout.Rows[rowIndex], layout.SubAvailableWidth, richTextDrawer,
                        isOnFunc, onClick);
                    leftRect = nextRect;
                }
            }

            if (error != "")
            {
                leftRect = ImGuiHelpBox.Draw(leftRect, error, MessageType.Error);
            }

            return leftRect;
        }

        internal static void UtilDrawButtonRow(Rect position, IReadOnlyList<ImGuiButtonInfo> rowButtons,
            float availableWidth, RichTextDrawer richTextDrawer, Func<ValueButtonRawInfo, bool> isOnFunc,
            Action<ValueButtonRawInfo> onClick)
        {
            if (rowButtons == null || rowButtons.Count == 0)
            {
                return;
            }

            GUIStyle miniButtonStyle = GetMiniButtonStyle();
            float x = position.x;
            float remainingWidth = Mathf.Max(1f, availableWidth);
            float totalPreferredWidth = rowButtons.Sum(each => each.Width);
            float extraWidthPerButton = rowButtons.Count == 0
                ? 0f
                : Mathf.Max(0f, availableWidth - totalPreferredWidth) / rowButtons.Count;

            for (int index = 0; index < rowButtons.Count; index++)
            {
                ImGuiButtonInfo buttonInfo = rowButtons[index];
                float desiredWidth = buttonInfo.Width + extraWidthPerButton;
                float buttonWidth = index == rowButtons.Count - 1
                    ? remainingWidth
                    : Mathf.Min(remainingWidth, desiredWidth);

                Rect buttonRect = new Rect(position)
                {
                    x = x,
                    width = Mathf.Max(1f, buttonWidth),
                };

                bool isOn = isOnFunc(buttonInfo.RawInfo);
                using (new EditorGUI.DisabledScope(buttonInfo.RawInfo.Disabled))
                using (EditorGUIBackgroundColor.ToggleButton(isOn))
                {
                    if (GUI.Button(buttonRect, GUIContent.none, miniButtonStyle))
                    {
                        onClick(buttonInfo.RawInfo);
                    }
                }

                float drawWidth = Mathf.Min(buttonInfo.ContentWidth, buttonRect.width);
                Rect contentRect = new Rect(buttonRect)
                {
                    x = buttonRect.x + Mathf.Max(0f, (buttonRect.width - drawWidth) * 0.5f),
                    width = drawWidth,
                };
                richTextDrawer.DrawChunks(contentRect, buttonInfo.RawInfo.DisplayChunks);

                x += buttonRect.width;
                remainingWidth = Mathf.Max(0f, remainingWidth - buttonRect.width);
            }
        }

        private static AdvancedDropdownMetaInfo UpdateStatus(SerializedProperty property,
            PathedDropdownAttribute valueButtonsAttribute, MemberInfo info, object parent,
            IRichTextTagProvider richTextTagProvider, out InfoIMGUI cache, out ValueButtonRawInfo[] rawInfos)
        {
            cache = EnsureKey(property);
            AdvancedDropdownMetaInfo metaInfo =
                AdvancedDropdownAttributeDrawer.GetMetaInfo(property, valueButtonsAttribute, info, parent, true, true);
            cache.Error = metaInfo.Error;
            rawInfos = UtilMakeButtonRawInfos(metaInfo, richTextTagProvider);
            return metaInfo;
        }

        private static List<List<ImGuiButtonInfo>> SplitRowsBalanced(IReadOnlyList<ImGuiButtonInfo> buttonInfos,
            float mainWidth, float subWidth)
        {
            List<List<ImGuiButtonInfo>> greedyRows = SplitRowsGreedy(buttonInfos, mainWidth, subWidth);
            int rowCount = greedyRows.Count;
            int buttonCount = buttonInfos.Count;
            if (rowCount <= 1 || buttonCount <= 2)
            {
                return greedyRows;
            }

            float[] prefixWidths = new float[buttonCount + 1];
            for (int index = 0; index < buttonCount; index++)
            {
                prefixWidths[index + 1] = prefixWidths[index] + buttonInfos[index].Width;
            }

            float[,] costs = new float[rowCount + 1, buttonCount + 1];
            int[,] previousBreaks = new int[rowCount + 1, buttonCount + 1];
            for (int rowIndex = 0; rowIndex <= rowCount; rowIndex++)
            {
                for (int buttonIndex = 0; buttonIndex <= buttonCount; buttonIndex++)
                {
                    costs[rowIndex, buttonIndex] = float.PositiveInfinity;
                    previousBreaks[rowIndex, buttonIndex] = -1;
                }
            }

            costs[0, 0] = 0;
            for (int rowIndex = 1; rowIndex <= rowCount; rowIndex++)
            {
                float rowWidth = rowIndex == 1 ? mainWidth : subWidth;
                for (int buttonIndex = rowIndex; buttonIndex <= buttonCount; buttonIndex++)
                {
                    for (int previousBreak = rowIndex - 1; previousBreak < buttonIndex; previousBreak++)
                    {
                        if (float.IsPositiveInfinity(costs[rowIndex - 1, previousBreak]))
                        {
                            continue;
                        }

                        float segmentWidth = prefixWidths[buttonIndex] - prefixWidths[previousBreak];
                        bool singleOversizedButton = buttonIndex - previousBreak == 1 && segmentWidth > rowWidth;
                        if (segmentWidth > rowWidth && !singleOversizedButton)
                        {
                            continue;
                        }

                        float unusedWidth = Mathf.Max(0f, rowWidth - segmentWidth);
                        float candidateCost = costs[rowIndex - 1, previousBreak] + unusedWidth * unusedWidth;
                        if (candidateCost >= costs[rowIndex, buttonIndex])
                        {
                            continue;
                        }

                        costs[rowIndex, buttonIndex] = candidateCost;
                        previousBreaks[rowIndex, buttonIndex] = previousBreak;
                    }
                }
            }

            if (previousBreaks[rowCount, buttonCount] < 0)
            {
                return greedyRows;
            }

            int[] breaks = new int[rowCount + 1];
            breaks[rowCount] = buttonCount;
            for (int rowIndex = rowCount; rowIndex > 0; rowIndex--)
            {
                breaks[rowIndex - 1] = previousBreaks[rowIndex, breaks[rowIndex]];
            }

            List<List<ImGuiButtonInfo>> balancedRows = new List<List<ImGuiButtonInfo>>(rowCount);
            for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
            {
                List<ImGuiButtonInfo> rowInfos =
                    new List<ImGuiButtonInfo>(breaks[rowIndex + 1] - breaks[rowIndex]);
                for (int buttonIndex = breaks[rowIndex]; buttonIndex < breaks[rowIndex + 1]; buttonIndex++)
                {
                    rowInfos.Add(buttonInfos[buttonIndex]);
                }

                balancedRows.Add(rowInfos);
            }

            return balancedRows;
        }

        private static List<List<ImGuiButtonInfo>> SplitRowsGreedy(IReadOnlyList<ImGuiButtonInfo> buttonInfos,
            float mainWidth, float subWidth)
        {
            int rowIndex = 0;
            float accWidth = mainWidth;
            List<List<ImGuiButtonInfo>> splitRowInfos = new List<List<ImGuiButtonInfo>>();

            for (int index = 0; index < buttonInfos.Count; index++)
            {
                ImGuiButtonInfo buttonInfo = buttonInfos[index];

                if (splitRowInfos.Count <= rowIndex)
                {
                    splitRowInfos.Add(new List<ImGuiButtonInfo>
                    {
                        buttonInfo,
                    });
                    accWidth = (index == 0 ? mainWidth : subWidth) - buttonInfo.Width;
                    if (accWidth < 0f)
                    {
                        rowIndex += 1;
                    }

                    continue;
                }

                if (accWidth < buttonInfo.Width)
                {
                    splitRowInfos.Add(new List<ImGuiButtonInfo>
                    {
                        buttonInfo,
                    });
                    accWidth = subWidth - buttonInfo.Width;
                    rowIndex += 1;
                }
                else
                {
                    splitRowInfos[rowIndex].Add(buttonInfo);
                    accWidth -= buttonInfo.Width;
                }
            }

            return splitRowInfos;
        }

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label, float width,
            int index,
            ISaintsAttribute saintsAttribute, FieldInfo info, bool hasLabelWidth, object parent) =>
            EditorGUIUtility.singleLineHeight;

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            int index,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes, FieldInfo info,
            object parent)
        {
            ValueButtonsAttribute valueButtonsAttribute = (ValueButtonsAttribute)saintsAttribute;
            AdvancedDropdownMetaInfo metaInfo = UpdateStatus(property, valueButtonsAttribute, info, parent, this,
                out InfoIMGUI cache, out ValueButtonRawInfo[] rawInfos);

            Rect fieldRect = EditorGUI.PrefixLabel(position, label);
            ImGuiButtonLayout layout = UtilGetButtonLayout(fieldRect.width, fieldRect.width,
                !valueButtonsAttribute.NoFold && !property.isExpanded, rawInfos, cache.RichTextDrawer);

            Rect buttonsRect = fieldRect;
            if (!valueButtonsAttribute.NoFold && layout.HasSubRow)
            {
                buttonsRect = UtilDrawFoldout(buttonsRect, property);
            }

            if (layout.Rows.Count == 0)
            {
                return;
            }

            UtilDrawButtonRow(buttonsRect, layout.Rows[0], layout.MainAvailableWidth, cache.RichTextDrawer,
                buttonInfo => metaInfo.CurValues.Any(each => Util.GetIsEqual(each, buttonInfo.Value)),
                buttonInfo =>
                {
                    ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info,
                        parent, buttonInfo.Value);
                    Util.SignPropertyValue(property, info, parent, buttonInfo.Value);
                    property.serializedObject.ApplyModifiedProperties();
                    TriggerChangedIMGUI(property, buttonInfo.Value);
                });
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute, int index,
            FieldInfo info, object parent)
        {
            UpdateStatus(property, (PathedDropdownAttribute)saintsAttribute, info, parent, this, out _, out _);
            ValueButtonsAttribute valueButtonsAttribute = (ValueButtonsAttribute)saintsAttribute;
            return EnsureKey(property).Error != "" || valueButtonsAttribute.NoFold || property.isExpanded;
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute, int index,
            FieldInfo info, object parent)
        {
            ValueButtonsAttribute valueButtonsAttribute = (ValueButtonsAttribute)saintsAttribute;
            AdvancedDropdownMetaInfo metaInfo = UpdateStatus(property, valueButtonsAttribute, info, parent, this,
                out InfoIMGUI cache, out ValueButtonRawInfo[] rawInfos);

            float inputWidth = UtilGetFieldInputWidth(width, label);
            ImGuiButtonLayout layout = UtilGetButtonLayout(inputWidth, width,
                !valueButtonsAttribute.NoFold && !property.isExpanded, rawInfos, cache.RichTextDrawer);

            return UtilGetBelowHeight(width, valueButtonsAttribute.NoFold || property.isExpanded, metaInfo.Error,
                layout);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent)
        {
            ValueButtonsAttribute valueButtonsAttribute = (ValueButtonsAttribute)saintsAttribute;
            AdvancedDropdownMetaInfo metaInfo = UpdateStatus(property, valueButtonsAttribute, info, parent, this,
                out InfoIMGUI cache, out ValueButtonRawInfo[] rawInfos);

            float inputWidth = UtilGetFieldInputWidth(position.width, label);
            ImGuiButtonLayout layout = UtilGetButtonLayout(inputWidth, position.width,
                !valueButtonsAttribute.NoFold && !property.isExpanded, rawInfos, cache.RichTextDrawer);

            return UtilDrawBelow(position, valueButtonsAttribute.NoFold || property.isExpanded, metaInfo.Error, layout,
                cache.RichTextDrawer,
                buttonInfo => metaInfo.CurValues.Any(each => Util.GetIsEqual(each, buttonInfo.Value)),
                buttonInfo =>
                {
                    ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info,
                        parent, buttonInfo.Value);
                    Util.SignPropertyValue(property, info, parent, buttonInfo.Value);
                    property.serializedObject.ApplyModifiedProperties();
                    TriggerChangedIMGUI(property, buttonInfo.Value);
                });
        }
    }
}

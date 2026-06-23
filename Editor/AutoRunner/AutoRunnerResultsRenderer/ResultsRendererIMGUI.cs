using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.AutoRunner.AutoRunnerResultsRenderer
{
    public partial class ResultsRenderer
    {
        private sealed class ResultsStatusIMGUI
        {
            public readonly Dictionary<string, bool> Foldouts = new Dictionary<string, bool>();
        }

        private const float RowSpacingIMGUI = 2f;
        private const float ChildIndentIMGUI = SaintsPropertyDrawer.IndentWidth;

        private readonly ResultsStatusIMGUI _resultsStatusIMGUI = new ResultsStatusIMGUI();

        protected override float GetFieldHeightIMGUI(float width, PreCheckResult preCheckResult)
        {
            if (!RenderField || !preCheckResult.IsShown || _autoRunner.Results.Count == 0)
            {
                return 0f;
            }

            float height = 0f;
            IReadOnlyList<AutoRunnerResult> results = _autoRunner.Results;

            if (GetFixableResultsWithIndex(results).Length > 0)
            {
                height += LineHeightIMGUI + RowSpacingIMGUI;
            }

            foreach ((object mainTarget, IEnumerable<IGrouping<Object, AutoRunnerResultInfo>> subGroup) in
                     FormatResults(results))
            {
                if (!TryGetMainTargetInfo(mainTarget, out _, out _))
                {
                    continue;
                }

                string mainKey = GetMainFoldoutKey(mainTarget);
                height += LineHeightIMGUI + RowSpacingIMGUI;
                if (!GetFoldoutIMGUI(mainKey))
                {
                    continue;
                }

                if (!(mainTarget is string))
                {
                    height += LineHeightIMGUI + RowSpacingIMGUI;
                }

                foreach (IGrouping<Object, AutoRunnerResultInfo> grouping in subGroup)
                {
                    if (grouping.Key == null)
                    {
                        continue;
                    }

                    string subKey = GetSubFoldoutKey(mainKey, grouping.Key);
                    height += LineHeightIMGUI + RowSpacingIMGUI;
                    if (!GetFoldoutIMGUI(subKey))
                    {
                        continue;
                    }

                    height += LineHeightIMGUI + RowSpacingIMGUI;
                    foreach (AutoRunnerResultInfo autoRunnerResultInfo in grouping)
                    {
                        height += GetResultHeightIMGUI(autoRunnerResultInfo, GetChildWidth(width, 2)) +
                                  RowSpacingIMGUI;
                    }
                }
            }

            return Mathf.Max(0f, height - RowSpacingIMGUI);
        }

        protected override void RenderPositionTargetIMGUI(Rect position, PreCheckResult preCheckResult)
        {
            if (!RenderField || !preCheckResult.IsShown || _autoRunner.Results.Count == 0)
            {
                return;
            }

            using (new EditorGUI.DisabledScope(preCheckResult.IsDisabled))
            {
                DrawResultsIMGUI(position);
            }
        }

        private void DrawResultsIMGUI(Rect position)
        {
            Rect leftRect = position;
            IReadOnlyList<AutoRunnerResult> results = _autoRunner.Results;

            (AutoRunnerResult value, int index)[] canFixWithIndex = GetFixableResultsWithIndex(results);
            if (canFixWithIndex.Length > 0)
            {
                (Rect fixAllRect, Rect afterFixAllRect) = RectUtils.SplitHeightRect(leftRect, LineHeightIMGUI);
                if (GUI.Button(fixAllRect, "Fix All"))
                {
                    RunFixAllAndRemove(canFixWithIndex);
                    RepaintAndExitGUI();
                }

                leftRect = AddRowSpacing(afterFixAllRect);
            }

            foreach ((object mainTarget, IEnumerable<IGrouping<Object, AutoRunnerResultInfo>> subGroup) in
                     FormatResults(results).ToArray())
            {
                if (!TryGetMainTargetInfo(mainTarget, out string mainLabel, out Object mainObject))
                {
                    continue;
                }

                string mainKey = GetMainFoldoutKey(mainTarget);
                (Rect mainFoldoutRect, Rect afterMainFoldoutRect) =
                    RectUtils.SplitHeightRect(leftRect, LineHeightIMGUI);
                bool mainExpanded = DrawFoldoutIMGUI(mainFoldoutRect, mainKey, mainLabel);
                leftRect = AddRowSpacing(afterMainFoldoutRect);
                if (!mainExpanded)
                {
                    continue;
                }

                if (mainObject != null)
                {
                    (Rect objectRect, Rect afterObjectRect) = RectUtils.SplitHeightRect(leftRect, LineHeightIMGUI);
                    DrawReadonlyObjectFieldIMGUI(IndentRect(objectRect, 1), mainObject);
                    leftRect = AddRowSpacing(afterObjectRect);
                }

                foreach (IGrouping<Object, AutoRunnerResultInfo> grouping in subGroup.ToArray())
                {
                    if (grouping.Key == null)
                    {
                        continue;
                    }

                    string subKey = GetSubFoldoutKey(mainKey, grouping.Key);
                    (Rect subFoldoutRect, Rect afterSubFoldoutRect) =
                        RectUtils.SplitHeightRect(leftRect, LineHeightIMGUI);
                    bool subExpanded = DrawFoldoutIMGUI(IndentRect(subFoldoutRect, 1), subKey, grouping.Key.name);
                    leftRect = AddRowSpacing(afterSubFoldoutRect);
                    if (!subExpanded)
                    {
                        continue;
                    }

                    (Rect subObjectRect, Rect afterSubObjectRect) =
                        RectUtils.SplitHeightRect(leftRect, LineHeightIMGUI);
                    DrawReadonlyObjectFieldIMGUI(IndentRect(subObjectRect, 2), grouping.Key);
                    leftRect = AddRowSpacing(afterSubObjectRect);

                    foreach (AutoRunnerResultInfo autoRunnerResultInfo in grouping.ToArray())
                    {
                        float resultHeight = GetResultHeightIMGUI(autoRunnerResultInfo, GetChildWidth(position.width, 2));
                        (Rect resultRect, Rect afterResultRect) = RectUtils.SplitHeightRect(leftRect, resultHeight);
                        DrawResultIMGUI(IndentRect(resultRect, 2), autoRunnerResultInfo);
                        leftRect = AddRowSpacing(afterResultRect);
                    }
                }
            }
        }

        private static float LineHeightIMGUI => SaintsPropertyDrawer.SingleLineHeight;

        private bool GetFoldoutIMGUI(string key)
        {
            if (_resultsStatusIMGUI.Foldouts.TryGetValue(key, out bool expanded))
            {
                return expanded;
            }

            _resultsStatusIMGUI.Foldouts[key] = true;
            return true;
        }

        private bool DrawFoldoutIMGUI(Rect position, string key, string label)
        {
            bool expanded = GetFoldoutIMGUI(key);
            bool nextExpanded = EditorGUI.Foldout(position, expanded, label, true);
            if (nextExpanded != expanded)
            {
                _resultsStatusIMGUI.Foldouts[key] = nextExpanded;
            }

            return nextExpanded;
        }

        private float GetResultHeightIMGUI(AutoRunnerResultInfo autoRunnerResultInfo, float width)
        {
            AutoRunnerFixerResult fixerResult = autoRunnerResultInfo.AutoRunnerResult.FixerResult;
            if (fixerResult == null)
            {
                return 0f;
            }

            float height = LineHeightIMGUI;
            if (!string.IsNullOrEmpty(fixerResult.ExecError))
            {
                height += RowSpacingIMGUI +
                          ImGuiHelpBox.GetHeight(fixerResult.ExecError, width, MessageType.Warning);
            }

            if (!string.IsNullOrEmpty(fixerResult.Error))
            {
                height += RowSpacingIMGUI +
                          ImGuiHelpBox.GetHeight(fixerResult.Error, width, MessageType.Error);
            }

            if (fixerResult.CanFix)
            {
                height += RowSpacingIMGUI + LineHeightIMGUI;
            }

            return height;
        }

        private void DrawResultIMGUI(Rect position, AutoRunnerResultInfo autoRunnerResultInfo)
        {
            AutoRunnerFixerResult fixerResult = autoRunnerResultInfo.AutoRunnerResult.FixerResult;
            if (fixerResult == null)
            {
                return;
            }

            Rect leftRect = position;
            (Rect propertyPathRect, Rect afterPropertyPathRect) =
                RectUtils.SplitHeightRect(leftRect, LineHeightIMGUI);
            EditorGUI.TextField(propertyPathRect, "Field/Property",
                autoRunnerResultInfo.AutoRunnerResult.propertyPath ?? "");
            leftRect = AddRowSpacing(afterPropertyPathRect);

            if (!string.IsNullOrEmpty(fixerResult.ExecError))
            {
                (Rect execErrorRect, Rect afterExecErrorRect) = RectUtils.SplitHeightRect(leftRect,
                    ImGuiHelpBox.GetHeight(fixerResult.ExecError, position.width, MessageType.Warning));
                ImGuiHelpBox.Draw(execErrorRect, fixerResult.ExecError, MessageType.Warning);
                leftRect = AddRowSpacing(afterExecErrorRect);
            }

            if (!string.IsNullOrEmpty(fixerResult.Error))
            {
                (Rect errorRect, Rect afterErrorRect) = RectUtils.SplitHeightRect(leftRect,
                    ImGuiHelpBox.GetHeight(fixerResult.Error, position.width, MessageType.Error));
                ImGuiHelpBox.Draw(errorRect, fixerResult.Error, MessageType.Error);
                leftRect = AddRowSpacing(afterErrorRect);
            }

            if (!fixerResult.CanFix)
            {
                return;
            }

            (Rect fixRect, _) = RectUtils.SplitHeightRect(leftRect, LineHeightIMGUI);
            if (GUI.Button(fixRect, "Fix"))
            {
                RunFixAndRemove(autoRunnerResultInfo);
                RepaintAndExitGUI();
            }
        }

        private void RepaintAndExitGUI()
        {
            _autoRunner.Repaint();
            GUI.changed = true;
            GUIUtility.ExitGUI();
        }

        private static void DrawReadonlyObjectFieldIMGUI(Rect position, Object value)
        {
            Type objectType = value == null ? typeof(Object) : value.GetType();
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUI.ObjectField(position, GUIContent.none, value, objectType, true);
            }
        }

        private static bool TryGetMainTargetInfo(object mainTarget, out string label, out Object mainObject)
        {
            mainObject = null;
            if (mainTarget is string mainTargetString)
            {
                label = mainTargetString;
                return true;
            }

            mainObject = mainTarget as Object;
            if (mainObject == null)
            {
                label = "";
                return false;
            }

            label = mainObject.name;
            return true;
        }

        private static string GetMainFoldoutKey(object mainTarget)
        {
            if (mainTarget is string mainTargetString)
            {
                return $"string:{mainTargetString}";
            }

            Object mainObject = mainTarget as Object;
            return mainObject == null
                ? "object:null"
                : $"object:{GetObjectIdIMGUI(mainObject)}";
        }

        private static string GetSubFoldoutKey(string mainKey, Object subTarget) =>
            $"{mainKey}/sub:{GetObjectIdIMGUI(subTarget)}";

        private static string GetObjectIdIMGUI(Object target)
        {
#if UNITY_6000_4_OR_NEWER
            return target.GetEntityId().ToString();
#else
            return target.GetInstanceID().ToString();
#endif
        }

        private static Rect AddRowSpacing(Rect rect)
        {
            return new Rect(rect)
            {
                y = rect.y + RowSpacingIMGUI,
                height = Mathf.Max(0f, rect.height - RowSpacingIMGUI),
            };
        }

        private static Rect IndentRect(Rect rect, int indentLevel)
        {
            float indent = ChildIndentIMGUI * indentLevel;
            return new Rect(rect)
            {
                x = rect.x + indent,
                width = Mathf.Max(0f, rect.width - indent),
            };
        }

        private static float GetChildWidth(float width, int indentLevel) =>
            Mathf.Max(1f, width - ChildIndentIMGUI * indentLevel);
    }
}

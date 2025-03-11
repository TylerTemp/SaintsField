using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;


namespace SaintsField.Editor.Drawers.ButtonDrawers.DecButtonDrawer
{
    public partial class DecButtonAttributeDrawer
    {

        protected class ButtonInfo
        {
            public string Error = "";
            public string ExecError = "";
            public IEnumerator Enumerator;
        }

        private static readonly Dictionary<string, ButtonInfo> ImGuiSharedInfo = new Dictionary<string, ButtonInfo>();
        private static readonly Dictionary<Object, HashSet<string>> InspectingTargets = new Dictionary<Object, HashSet<string>>();

        protected static string GetDisplayError(SerializedProperty property)
        {
            string key = SerializedUtils.GetUniqueId(property);
            if (!ImGuiSharedInfo.TryGetValue(key, out ButtonInfo errorInfo))
            {
                return "";
            }

            string error = errorInfo.Error;
            string execError = errorInfo.ExecError;

            if (error != "" && execError != "")
            {
                return $"{error}\n\n{execError}";
            }
            return $"{error}{execError}";
        }

        protected static ButtonInfo GetOrCreateButtonInfo(SerializedProperty property)
        {
            string key = SerializedUtils.GetUniqueId(property);
            // ReSharper disable once InvertIf
            if (!ImGuiSharedInfo.TryGetValue(key, out ButtonInfo buttonInfo))
            {
                buttonInfo = new ButtonInfo
                {
                    Error = "",
                    ExecError = "",
                };
                ImGuiSharedInfo[key] = buttonInfo;
            }

            return buttonInfo;
        }

        // ReSharper disable once InconsistentNaming
        protected readonly RichTextDrawer RichTextDrawer = new RichTextDrawer();

        protected override void ImGuiOnDispose()
        {
            base.ImGuiOnDispose();
            RichTextDrawer.Dispose();
        }

        protected Rect Draw(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute, FieldInfo info, object target)
        {
            string key = SerializedUtils.GetUniqueId(property);

            Object curInspectingTarget = property.serializedObject.targetObject;

            if (!InspectingTargets.TryGetValue(curInspectingTarget, out HashSet<string> keySet))
            {
                InspectingTargets[curInspectingTarget] = keySet = new HashSet<string>();

                void OnSelectionChangedIMGUI()
                {
                    bool stillSelected = Array.IndexOf(Selection.objects, curInspectingTarget) != -1;
                    if (stillSelected)
                    {
                        return;
                    }

                    Selection.selectionChanged -= OnSelectionChangedIMGUI;
                    if (InspectingTargets.TryGetValue(curInspectingTarget, out HashSet<string> set))
                    {
                        foreach (string removeKey in set)
                        {
                            ImGuiSharedInfo.Remove(removeKey);
                        }
                    }
                    InspectingTargets.Remove(curInspectingTarget);
                }

                Selection.selectionChanged += OnSelectionChangedIMGUI;
            }
            keySet.Add(key);

            ButtonInfo buttonInfo = GetOrCreateButtonInfo(property);
            if(buttonInfo.Enumerator != null && !buttonInfo.Enumerator.MoveNext())
            {
                buttonInfo.Enumerator = null;
            }

            DecButtonAttribute decButtonAttribute = (DecButtonAttribute) saintsAttribute;

            (Rect buttonRect, Rect leftRect) = RectUtils.SplitHeightRect(position, EditorGUIUtility.singleLineHeight);

            // object target = GetParentTarget(property);
            (string xmlError, string buttonLabelXml) = RichTextDrawer.GetLabelXml(property, decButtonAttribute.ButtonLabel, decButtonAttribute.IsCallback, info, target);
            // Error = xmlError;
            if (xmlError != "")
            {
                buttonInfo.Error = xmlError;
            }

            if (GUI.Button(buttonRect, string.Empty))
            {
                (string error, object result) = CallButtonFunc(property, decButtonAttribute, info, target);
                buttonInfo.ExecError = error;
                if (result is IEnumerator enumerator)
                {
                    buttonInfo.Enumerator = enumerator;
                }
                else
                {
                    buttonInfo.Enumerator = null;
                }
            }

            IReadOnlyList<RichTextDrawer.RichTextChunk> richChunks;
            // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
            if (buttonLabelXml is null)
            {
                buttonLabelXml = ObjectNames.NicifyVariableName(decButtonAttribute.FuncName);
                richChunks = new[]
                {
                    new RichTextDrawer.RichTextChunk
                    {
                        IsIcon = false,
                        Content = buttonLabelXml,
                    },
                };
            }
            else
            {
                richChunks = RichTextDrawer.ParseRichXml(buttonLabelXml, label.text, property, info, target).ToArray();
            }

            // GetWidth
            float textWidth = RichTextDrawer.GetWidth(label, buttonRect.height, richChunks);
            Rect labelRect = buttonRect;
            // EditorGUI.DrawRect(labelRect, Color.blue);
            if (textWidth < labelRect.width)
            {
                float space = (labelRect.width - textWidth) / 2f;
                labelRect.x += space;
                labelRect.width -= space;
                // EditorGUI.DrawRect(labelRect, Color.yellow);
            }

            ImGuiEnsureDispose(property.serializedObject.targetObject);
            RichTextDrawer.DrawChunks(labelRect, label, richChunks);

            return leftRect;

        }
    }
}

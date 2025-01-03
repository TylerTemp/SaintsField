﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
#if UNITY_2021_3_OR_NEWER
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Drawers
{
    public abstract class DecButtonAttributeDrawer: SaintsPropertyDrawer
    {
        protected class ErrorInfo
        {
            public string Error = "";
            public string ExecError = "";
        }

        private static readonly Dictionary<string, ErrorInfo> ImGuiSharedErrorInfo = new Dictionary<string, ErrorInfo>();

#if UNITY_2019_2_OR_NEWER
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
#endif
        [InitializeOnLoadMethod]
        private static void ImGuiClearSharedData() => ImGuiSharedErrorInfo.Clear();

        protected static string GetDisplayError(SerializedProperty property)
        {
            string key = SerializedUtils.GetUniqueId(property);
            if (!ImGuiSharedErrorInfo.TryGetValue(key, out ErrorInfo errorInfo))
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

        protected static ErrorInfo GetOrCreateErrorInfo(SerializedProperty property)
        {
            string key = SerializedUtils.GetUniqueId(property);
            if (!ImGuiSharedErrorInfo.TryGetValue(key, out ErrorInfo errorInfo))
            {
                errorInfo = new ErrorInfo
                {
                    Error = "",
                    ExecError = "",
                };
                ImGuiSharedErrorInfo[key] = errorInfo;
            }

            return errorInfo;
        }

        // ReSharper disable once InconsistentNaming
        protected readonly RichTextDrawer RichTextDrawer = new RichTextDrawer();

        protected override void ImGuiOnDispose()
        {
            base.ImGuiOnDispose();
            RichTextDrawer.Dispose();
        }

        #region IMGUI
        protected Rect Draw(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute, FieldInfo info, object target)
        {
            DecButtonAttribute decButtonAttribute = (DecButtonAttribute) saintsAttribute;

            (Rect buttonRect, Rect leftRect) = RectUtils.SplitHeightRect(position, EditorGUIUtility.singleLineHeight);

            // object target = GetParentTarget(property);
            (string xmlError, string buttonLabelXml) = RichTextDrawer.GetLabelXml(property, decButtonAttribute.ButtonLabel, decButtonAttribute.IsCallback, info, target);
            // Error = xmlError;
            if (xmlError != "")
            {
                GetOrCreateErrorInfo(property).Error = xmlError;
            }

            if (GUI.Button(buttonRect, string.Empty))
            {
                GetOrCreateErrorInfo(property).ExecError = CallButtonFunc(property, decButtonAttribute, info, target).error;
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
                richChunks = RichTextDrawer.ParseRichXml(buttonLabelXml, label.text, info, target).ToArray();
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
        #endregion

        private static (string error, object result) CallButtonFunc(SerializedProperty property, DecButtonAttribute decButtonAttribute, FieldInfo fieldInfo, object target)
        {
            return Util.GetMethodOf<object>(decButtonAttribute.FuncName, null, property, fieldInfo, target);
            // return error;
        }

#if UNITY_2021_3_OR_NEWER

        #region UIToolkit

        // private static string ClassButton(SerializedProperty property) => $"{property.propertyPath}__Button";
        private static string ClassLabelContainer(SerializedProperty property, int index) => $"{property.propertyPath}__{index}__LabelContainer";
        private static string ClassLabelError(SerializedProperty property, int index) => $"{property.propertyPath}__{index}__LabelError";
        private static string ClassExecError(SerializedProperty property, int index) => $"{property.propertyPath}__{index}__ExecError";

        protected static VisualElement DrawUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, FieldInfo info, object parent, VisualElement container)
        {
            Button buttonElement = null;
            IVisualElementScheduledItem buttonTask = null;
            buttonElement = new Button(() =>
            {
                (string buttonError, object buttonResult) = CallButtonFunc(property, (DecButtonAttribute) saintsAttribute, info, parent);
                HelpBox helpBox = container.Query<HelpBox>(className: ClassExecError(property, index)).First();
                helpBox.style.display = buttonError == ""? DisplayStyle.None: DisplayStyle.Flex;
                helpBox.text = buttonError;

                buttonTask?.Pause();
                if (buttonResult is System.Collections.IEnumerator enumerator)
                {
                    buttonElement.userData = enumerator;
                    buttonTask?.Pause();
                    buttonTask = buttonElement.schedule.Execute(() =>
                    {
                        if (buttonElement.userData is System.Collections.IEnumerator bindEnumerator)
                        {
                            if (!bindEnumerator.MoveNext())
                            {
                                buttonTask?.Pause();
                            }
                        }
                    }).Every(1);
                }
            })
            {
                style =
                {
                    height = EditorGUIUtility.singleLineHeight,
                    flexGrow = 1,
                },
            };

            VisualElement labelContainer = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    // flexGrow = 1,
                    justifyContent = Justify.Center,  // horizontal
                    alignItems = Align.Center,  // vertical
                },
                userData = "",
            };
            labelContainer.AddToClassList(ClassLabelContainer(property, index));
            // labelContainer.Add(new Label("test label"));

            buttonElement.Add(labelContainer);
            // button.AddToClassList();
            buttonElement.AddToClassList(ClassAllowDisable);
            return buttonElement;
        }

        protected static HelpBox DrawLabelError(SerializedProperty property, int index) => DrawError(ClassLabelError(property, index));

        protected static HelpBox DrawExecError(SerializedProperty property, int index) => DrawError(ClassExecError(property, index));

        private static HelpBox DrawError(string className)
        {
            HelpBox helpBox = new HelpBox("", HelpBoxMessageType.Error)
            {
                style =
                {
                    display = DisplayStyle.None,
                },
            };
            helpBox.AddToClassList(className);
            helpBox.AddToClassList(ClassAllowDisable);
            return helpBox;
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info)
        {
            // if (parent == null)
            // {
            //     return;
            // }

            VisualElement labelContainer = container.Query<VisualElement>(className: ClassLabelContainer(property, index)).First();
            string oldXml = (string)labelContainer.userData;
            DecButtonAttribute decButtonAttribute = (DecButtonAttribute) saintsAttribute;

            object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
            (string xmlError, string newXml) = RichTextDrawer.GetLabelXml(property, decButtonAttribute.ButtonLabel, decButtonAttribute.IsCallback, info, parent);

            // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
            if (newXml == null)
            {
                newXml = ObjectNames.NicifyVariableName(decButtonAttribute.FuncName);
            }

            HelpBox helpBox = container.Query<HelpBox>(className: ClassLabelError(property, index)).First();
            helpBox.style.display = xmlError == ""? DisplayStyle.None: DisplayStyle.Flex;
            helpBox.text = xmlError;

            if (oldXml == newXml)
            {
                return;
            }

            // Debug.Log($"update xml={newXml}");

            labelContainer.userData = newXml;
            labelContainer.Clear();
            IEnumerable<RichTextDrawer.RichTextChunk> richChunks = RichTextDrawer.ParseRichXml(newXml, property.displayName, info, parent);
            foreach (VisualElement visualElement in RichTextDrawer.DrawChunksUIToolKit(richChunks))
            {
                labelContainer.Add(visualElement);
            }
        }

        #endregion

#endif
    }
}

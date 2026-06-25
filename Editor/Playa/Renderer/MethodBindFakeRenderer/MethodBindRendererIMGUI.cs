using System.Collections.Generic;
using System;
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Playa.Renderer.MethodBindFakeRenderer
{
    public partial class MethodBindRenderer
    {
        private RichTextDrawer _richTextDrawer;

        private string _cachedCallbackLabelIMGUI;
        private IReadOnlyList<RichTextDrawer.RichTextChunk> _cachedRichTextChunksIMGUI;

        private bool _addListener;

        private void EnsureAddListener()
        {
            if (_addListener)
            {
                return;
            }

            _addListener = true;
            SaintsEditorApplicationChanged.OnAnyEvent.AddListener(OnApplicationChanged);
        }

        private void OnApplicationChanged()
        {
            RefreshCheckMethodBind().fixer?.Invoke();
        }

        public override void OnDestroyIMGUI()
        {
            SaintsEditorApplicationChanged.OnAnyEvent.RemoveListener(OnApplicationChanged);
        }

        protected override float GetFieldHeightIMGUI(float width, PreCheckResult preCheckResult)
        {
            EnsureAddListener();

            Object targetObj = _serializedObject.targetObject;
            (string error, bool _, Object __, UnityEventBase unityEventBase, object ___, UnityEventCallState ____, bool hasValue, Type valueType, object value) =
                FindAlreadyAddedCallback(_methodBindAttribute, FieldWithInfo.MethodInfo, targetObj, FieldWithInfo.Targets[0]);
            if (!string.IsNullOrEmpty(error))
            {
                return ImGuiHelpBox.GetHeight(error, width, MessageType.Error);
            }

            float titleHeight = SaintsPropertyDrawer.SingleLineHeight;
            float bodyHeight = Mathf.Max(SaintsPropertyDrawer.SingleLineHeight, hasValue ? FieldHeight(value, "") : SaintsPropertyDrawer.SingleLineHeight);
            return titleHeight + bodyHeight + 4f;
        }

        protected override void RenderPositionTargetIMGUI(Rect position, PreCheckResult preCheckResult)
        {
            EnsureAddListener();

            (string checkError, Action fixer) = RefreshCheckMethodBind();
            if (string.IsNullOrEmpty(checkError))
            {
                fixer?.Invoke();
            }

            Object targetObj = _serializedObject.targetObject;
            (string error, bool _, Object unityEventContainerObject, UnityEventBase unityEventBase, object _, UnityEventCallState unityEventCallState, bool hasValue, Type valueType, object value) =
                FindAlreadyAddedCallback(_methodBindAttribute, FieldWithInfo.MethodInfo, targetObj, FieldWithInfo.Targets[0]);
            if (!string.IsNullOrEmpty(error))
            {
                EditorGUI.HelpBox(position, error, MessageType.Error);
                return;
            }

            string eventSuffix = "";
            if (unityEventBase is not UnityEvent)
            {
                Type type = unityEventBase.GetType();
                if (type.IsGenericType)
                {
                    Type[] genericTypes = type.GetGenericArguments();
                    if (genericTypes.Length > 0)
                    {
                        string[] typeNames = new string[genericTypes.Length];
                        for (int i = 0; i < genericTypes.Length; i++)
                        {
                            typeNames[i] = ReflectUtils.StringifyType(genericTypes[i]);
                        }

                        eventSuffix = $" ({string.Join(", ", typeNames)})";
                    }
                }
            }

            string rawEventTarget = _methodBindAttribute.EventTarget ?? "Button.onClick";
            string targetEventName = $"{rawEventTarget.Split('.').Last()}{eventSuffix}";

            GUI.Box(position, GUIContent.none);

            float titleHeight = SaintsPropertyDrawer.SingleLineHeight;
            float rowHeight = Mathf.Max(SaintsPropertyDrawer.SingleLineHeight, hasValue ? FieldHeight(value, "") : SaintsPropertyDrawer.SingleLineHeight);
            (Rect titleRect, Rect bodyRect) = RectUtils.SplitHeightRect(position, titleHeight);
            bodyRect = new Rect(bodyRect) { y = bodyRect.y + 2f, height = rowHeight };

            EditorGUI.LabelField(titleRect, ObjectNames.NicifyVariableName(FieldWithInfo.MethodInfo.Name), EditorStyles.boldLabel);

            float innerY = bodyRect.y + 1f;
            Rect innerRect = new Rect(bodyRect.x + 2f, innerY, bodyRect.width - 4f, bodyRect.height - 2f);
            float colWidth = innerRect.width / 4f;

            using (new EditorGUI.DisabledScope(true))
            {
                Rect callStateRect = new Rect(innerRect.x, innerRect.y, colWidth, innerRect.height);
                EditorGUI.EnumPopup(callStateRect, unityEventCallState);

                Rect targetRect = new Rect(innerRect.x + colWidth, innerRect.y, colWidth, innerRect.height);
                EditorGUI.ObjectField(targetRect, GUIContent.none, unityEventContainerObject, typeof(Object), true);

                Rect eventRect = new Rect(innerRect.x + colWidth * 2, innerRect.y, colWidth, innerRect.height);
                EditorGUI.LabelField(eventRect, targetEventName, new GUIStyle(EditorStyles.label)
                {
                    alignment = TextAnchor.MiddleLeft,
                });

                Rect valueRect = new Rect(innerRect.x + colWidth * 3, innerRect.y, colWidth, innerRect.height);
                if (hasValue)
                {
                    FieldPosition(valueRect, value, "", valueType, true);
                }
                // else
                // {
                //     EditorGUI.LabelField(valueRect, "No Value");
                // }
            }
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Utils;
using SaintsField.Editor.Utils.IMGUIEditDrawer;
using SaintsField.Editor.Utils.WaitableUtils;
using SaintsField.Playa;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Playa.Renderer.ButtonFakeRenderer
{
    public partial class ButtonRenderer
    {
        private enum ButtonStatusIMGUI
        {
            None,
            Loading,
            Ok,
            Error,
            Warning,
            Pause,
        }

        private sealed class ButtonUserDataIMGUI
        {
            public MethodInfo MethodInfo;
            public ParameterInfo[] Parameters = Array.Empty<ParameterInfo>();
            public object[] ParameterValues = Array.Empty<object>();
            public IReadOnlyList<Attribute>[] ParameterAttributes = Array.Empty<IReadOnlyList<Attribute>>();
            public IReadOnlyList<Attribute> ReturnAttributes = Array.Empty<Attribute>();
            public string ButtonId;

            public string Xml;
            public string Callback;
            public readonly RichTextDrawer RichTextDrawer = new RichTextDrawer();
            public IReadOnlyList<RichTextDrawer.RichTextChunk> RichTextChunks;
            public string RichTextChunksXml;

            public readonly List<Waiter> Enumerators = new List<Waiter>();
            public bool WaiterHasError;
            public bool WaiterHasFinished;

            public readonly List<string> ResultErrors = new List<string>();
            public bool ShowReturnValue;
            public object ReturnValue;

            public ButtonStatusIMGUI Status;
            public double StatusHideAt = -1d;
            public float Progress = -1f;
            public readonly IMGUILoading Loading = new IMGUILoading();
        }

        private const float PaddingBox = 2f;
        private const float CloseButtonWidthIMGUI = 18f;
        private const float StatusSizeIMGUI = 14f;
        private const float StatusDurationIMGUI = 2f;
        private const string StatusOkIconIMGUI = "check.png";
        private const string StatusOkColorIMGUI = "#49FF7B";
        private const string StatusErrorIconIMGUI = "close.png";
        private const string StatusErrorColorIMGUI = "#FF2D17";
        private const string StatusWarningIconIMGUI = "console.warnicon";
        private const string StatusWarningColorIMGUI = null;
        private const string StatusPauseIconIMGUI = "d_PauseButton";
        private const string StatusPauseColorIMGUI = "#9717FF";

        private ButtonUserDataIMGUI _buttonUserDataIMGUI;
        private static readonly Dictionary<string, Texture2D> StatusIconCacheIMGUI = new Dictionary<string, Texture2D>();

        private ButtonUserDataIMGUI EnsureButtonUserDataIMGUI()
        {
            MethodInfo methodInfo = FieldWithInfo.MethodInfo;
            if (_buttonUserDataIMGUI != null && _buttonUserDataIMGUI.MethodInfo == methodInfo)
            {
                return _buttonUserDataIMGUI;
            }

            ParameterInfo[] parameters = methodInfo.GetParameters();
            string buttonText = string.IsNullOrEmpty(_buttonAttribute.Label) || _buttonAttribute.IsCallback
                ? ObjectNames.NicifyVariableName(methodInfo.Name)
                : _buttonAttribute.Label;

            _buttonUserDataIMGUI = new ButtonUserDataIMGUI
            {
                MethodInfo = methodInfo,
                Parameters = parameters,
                ParameterValues = parameters.Select(GetParameterDefaultValue).ToArray(),
                ParameterAttributes = parameters
                    .Select(each => (IReadOnlyList<Attribute>)each.GetCustomAttributes().OfType<Attribute>().ToArray())
                    .ToArray(),
                ReturnAttributes = ReflectCache.GetCustomAttributes(methodInfo),
                ButtonId = $"{FieldWithInfo.Targets[0].GetHashCode()}.{methodInfo.Name}",
                Xml = buttonText,
                Callback = _buttonAttribute.IsCallback ? _buttonAttribute.Label : "",
            };

            return _buttonUserDataIMGUI;
        }

        protected override float GetFieldHeightIMGUI(float width, PreCheckResult preCheckResult)
        {
            if (!preCheckResult.IsShown || _buttonAttribute == null)
            {
                return 0f;
            }

            ButtonUserDataIMGUI userData = EnsureButtonUserDataIMGUI();
            return GetParametersHeightIMGUI(userData, width)
                   + SaintsPropertyDrawer.SingleLineHeight
                   + GetResultHeightIMGUI(userData, width);
        }

        protected override void RenderPositionTargetIMGUI(Rect position, PreCheckResult preCheckResult)
        {
            if (!preCheckResult.IsShown || _buttonAttribute == null)
            {
                return;
            }

            ButtonUserDataIMGUI userData = EnsureButtonUserDataIMGUI();
            TickEnumeratorsIMGUI(userData);

            float parameterHeight = GetParametersHeightIMGUI(userData, position.width);
            (Rect parametersRect, Rect leftRect) = RectUtils.SplitHeightRect(position, parameterHeight);
            if (parameterHeight > Mathf.Epsilon)
            {
                DrawParametersIMGUI(parametersRect, userData);
            }

            (Rect buttonRect, Rect resultRect) =
                RectUtils.SplitHeightRect(leftRect, SaintsPropertyDrawer.SingleLineHeight);
            DrawButtonAreaIMGUI(buttonRect, preCheckResult, userData);

            float resultHeight = GetResultHeightIMGUI(userData, position.width);
            if (resultHeight > Mathf.Epsilon)
            {
                DrawResultIMGUI(new Rect(resultRect)
                {
                    height = resultHeight,
                }, userData);
            }
        }

        private float GetParametersHeightIMGUI(ButtonUserDataIMGUI userData, float width)
        {
            if (userData.Parameters.Length == 0)
            {
                return 0f;
            }

            float contentWidth = Mathf.Max(1f, width - PaddingBox * 2f);
            return userData.Parameters
                       .Select((each, index) => GetParameterHeightIMGUI(userData, index, contentWidth))
                       .Sum()
                   + PaddingBox * 2f;
        }

        private float GetParameterHeightIMGUI(ButtonUserDataIMGUI userData, int index, float width)
        {
            ParameterInfo parameterInfo = userData.Parameters[index];
            return IMGUIEdit.GetPropertyHeight(
                ObjectNames.NicifyVariableName(parameterInfo.Name),
                parameterInfo.ParameterType,
                userData.ParameterValues[index],
                NoBeforeSetIMGUI,
                newValue => SetParameterValueIMGUI(userData, index, newValue),
                false,
                InAnyHorizontalLayout,
                userData.ParameterAttributes[index],
                FieldWithInfo.Targets,
                this,
                $"{userData.ButtonId}.{parameterInfo.Name}");
        }

        private float GetResultHeightIMGUI(ButtonUserDataIMGUI userData, float width)
        {
            if (!HasResultIMGUI(userData))
            {
                return 0f;
            }

            float contentWidth = Mathf.Max(1f, width - PaddingBox * 2f);
            float height = PaddingBox * 2f;
            foreach (string error in userData.ResultErrors)
            {
                height += ImGuiHelpBox.GetHeight(error, contentWidth, MessageType.Error);
            }

            if (userData.ShowReturnValue)
            {
                height += IMGUIEdit.GetPropertyHeight(
                    "[return]",
                    userData.MethodInfo.ReturnType,
                    userData.ReturnValue,
                    NoBeforeSetIMGUI,
                    newValue => userData.ReturnValue = newValue,
                    false,
                    InAnyHorizontalLayout,
                    userData.ReturnAttributes,
                    FieldWithInfo.Targets,
                    this,
                    $"{userData.ButtonId}.[return]");
            }

            return height;
        }

        private static bool HasResultIMGUI(ButtonUserDataIMGUI userData)
        {
            return userData.ShowReturnValue || userData.ResultErrors.Count > 0;
        }

        private void DrawParametersIMGUI(Rect position, ButtonUserDataIMGUI userData)
        {
            GUI.Box(position, GUIContent.none);

            Rect contentRect = new Rect(position)
            {
                x = position.x + PaddingBox,
                y = position.y + PaddingBox,
                width = Mathf.Max(0f, position.width - PaddingBox * 2f),
                height = Mathf.Max(0f, position.height - PaddingBox * 2f),
            };

            float y = contentRect.y;
            foreach ((ParameterInfo parameterInfo, int index) in userData.Parameters.WithIndex())
            {
                float height = GetParameterHeightIMGUI(userData, index, contentRect.width);
                Rect parameterRect = new Rect(contentRect)
                {
                    y = y,
                    height = height,
                };
                y += height;

                IMGUIEdit.OnGUI(
                    parameterRect,
                    ObjectNames.NicifyVariableName(parameterInfo.Name),
                    parameterInfo.ParameterType,
                    userData.ParameterValues[index],
                    NoBeforeSetIMGUI,
                    newValue => SetParameterValueIMGUI(userData, index, newValue),
                    false,
                    InAnyHorizontalLayout,
                    userData.ParameterAttributes[index],
                    FieldWithInfo.Targets,
                    this,
                    $"{userData.ButtonId}.{parameterInfo.Name}");
            }
        }

        private void DrawButtonAreaIMGUI(Rect position, PreCheckResult preCheckResult, ButtonUserDataIMGUI userData)
        {
            Rect mainButtonRect = position;
            bool showCloseButton = HasResultIMGUI(userData) || userData.Enumerators.Count > 0;
            Rect closeButtonRect = default;
            if (showCloseButton)
            {
                mainButtonRect.width = Mathf.Max(0f, mainButtonRect.width - CloseButtonWidthIMGUI);
                closeButtonRect = new Rect(position)
                {
                    x = mainButtonRect.xMax,
                    width = CloseButtonWidthIMGUI,
                };
            }

            using (new EditorGUI.DisabledScope(preCheckResult.IsDisabled))
            {
                if (GUI.Button(mainButtonRect, GUIContent.none))
                {
                    InvokeButtonIMGUI(userData);
                }

                DrawButtonLabelIMGUI(mainButtonRect, userData);

                if (showCloseButton && GUI.Button(closeButtonRect, "x"))
                {
                    CloseResultIMGUI(userData);
                }
            }

            DrawStatusIMGUI(position, userData);
        }

        private void DrawButtonLabelIMGUI(Rect buttonRect, ButtonUserDataIMGUI userData)
        {
            IReadOnlyList<RichTextDrawer.RichTextChunk> richTextChunks = GetRichIMGUI(userData);
            if (richTextChunks.Count == 0)
            {
                return;
            }

            GUIContent oldLabel = new GUIContent(ObjectNames.NicifyVariableName(FieldWithInfo.MethodInfo.Name));
            float drawNeedWidth = userData.RichTextDrawer.GetWidth(oldLabel, buttonRect.height, richTextChunks);
            Rect drawRect = drawNeedWidth > buttonRect.width
                ? buttonRect
                : new Rect(buttonRect.x + (buttonRect.width - drawNeedWidth) / 2f, buttonRect.y, drawNeedWidth,
                    buttonRect.height);
            userData.RichTextDrawer.DrawChunks(drawRect, richTextChunks);
        }

        private void DrawResultIMGUI(Rect position, ButtonUserDataIMGUI userData)
        {
            GUI.Box(position, GUIContent.none);

            Rect contentRect = new Rect(position)
            {
                x = position.x + PaddingBox,
                y = position.y + PaddingBox,
                width = Mathf.Max(0f, position.width - PaddingBox * 2f),
                height = Mathf.Max(0f, position.height - PaddingBox * 2f),
            };

            float y = contentRect.y;
            foreach (string error in userData.ResultErrors)
            {
                float height = ImGuiHelpBox.GetHeight(error, contentRect.width, MessageType.Error);
                Rect errorRect = new Rect(contentRect)
                {
                    y = y,
                    height = height,
                };
                ImGuiHelpBox.Draw(errorRect, error, MessageType.Error);
                y += height;
            }

            if (!userData.ShowReturnValue)
            {
                return;
            }

            float returnHeight = IMGUIEdit.GetPropertyHeight(
                "[return]",
                userData.MethodInfo.ReturnType,
                userData.ReturnValue,
                NoBeforeSetIMGUI,
                newValue => userData.ReturnValue = newValue,
                false,
                InAnyHorizontalLayout,
                userData.ReturnAttributes,
                FieldWithInfo.Targets,
                this,
                $"{userData.ButtonId}.[return]");

            Rect returnRect = new Rect(contentRect)
            {
                y = y,
                height = returnHeight,
            };

            IMGUIEdit.OnGUI(
                returnRect,
                "[return]",
                userData.MethodInfo.ReturnType,
                userData.ReturnValue,
                NoBeforeSetIMGUI,
                newValue => userData.ReturnValue = newValue,
                false,
                InAnyHorizontalLayout,
                userData.ReturnAttributes,
                FieldWithInfo.Targets,
                this,
                $"{userData.ButtonId}.[return]");
        }

        private IReadOnlyList<RichTextDrawer.RichTextChunk> GetRichIMGUI(ButtonUserDataIMGUI userData)
        {
            string useXml = userData.Xml;
            if (!string.IsNullOrEmpty(userData.Callback))
            {
                (string error, MemberInfo _, string result) = Util.GetOf<string>(userData.Callback, null,
                    FieldWithInfo.SerializedProperty, FieldWithInfo.MethodInfo, FieldWithInfo.Targets[0], null);

                if (error != "")
                {
#if SAINTSFIELD_DEBUG
                    Debug.LogError(error);
#endif
                    useXml = ObjectNames.NicifyVariableName(userData.MethodInfo.Name);
                }
                else
                {
                    useXml = result;
                }
            }

            if (useXml == "")
            {
                userData.RichTextChunksXml = useXml;
                userData.RichTextChunks = Array.Empty<RichTextDrawer.RichTextChunk>();
                return userData.RichTextChunks;
            }

            if (useXml is null)
            {
                useXml = ObjectNames.NicifyVariableName(userData.MethodInfo.Name);
            }

            if (userData.RichTextChunks != null && userData.RichTextChunksXml == useXml)
            {
                return userData.RichTextChunks;
            }

            userData.Xml = useXml;
            userData.RichTextChunksXml = useXml;
            userData.RichTextChunks = RichTextDrawer.ParseRichXmlWithProvider(useXml, this).ToArray();
            return userData.RichTextChunks;
        }

        private void InvokeButtonIMGUI(ButtonUserDataIMGUI userData)
        {
            userData.ResultErrors.Clear();
            userData.ShowReturnValue = false;
            userData.ReturnValue = null;
            userData.Enumerators.Clear();
            userData.WaiterHasError = false;
            userData.WaiterHasFinished = false;
            userData.Progress = -1f;
            userData.Status = ButtonStatusIMGUI.None;
            userData.StatusHideAt = -1d;

            SaintsContext.SerializedProperty = _serializedProperty;
            int targetCount = FieldWithInfo.Targets.Count;
            object[] returnValues = new object[targetCount];
            Exception error = null;
            bool isStruct = ReflectUtils.TypeIsStruct(FieldWithInfo.Targets[0].GetType());

            for (int index = 0; index < targetCount; index++)
            {
                object eachTarget = FieldWithInfo.Targets[index];
                (object rawMemberValue, object useTarget) = GetRefreshedTarget(FieldWithInfo, eachTarget);

                object result;
                try
                {
                    result = userData.MethodInfo.Invoke(useTarget, userData.ParameterValues);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    error = e;
                    break;
                }

                returnValues[index] = result;
                if (isStruct)
                {
                    BackWriteCallback(rawMemberValue, useTarget);
                }
            }

            if (error != null)
            {
                userData.ResultErrors.Add(error.InnerException?.Message ?? error.Message);
                PlayStatusIMGUI(userData, ButtonStatusIMGUI.Error);
                return;
            }

            if (HasReturnValueIMGUI(userData.MethodInfo))
            {
                userData.ReturnValue = returnValues[0];
                userData.ShowReturnValue = true;
            }

            foreach (IEnumerator enumerator in returnValues.OfType<IEnumerator>())
            {
                userData.Enumerators.Add(new Waiter(enumerator));
            }

            if (userData.Enumerators.Count == 0)
            {
                PlayStatusIMGUI(userData, ButtonStatusIMGUI.Ok);
            }
            else
            {
                PlayStatusIMGUI(userData, ButtonStatusIMGUI.Loading);
            }
        }

        private void TickEnumeratorsIMGUI(ButtonUserDataIMGUI userData)
        {
            if (Event.current == null || Event.current.type != EventType.Repaint || userData.Enumerators.Count == 0)
            {
                return;
            }

            List<Waiter> finishedEnumerators = new List<Waiter>();
            int oldCounter = userData.Enumerators.Count;
            float progress = -1f;

            foreach (Waiter waiter in userData.Enumerators)
            {
                waiter.Update();

                if (!waiter.Done())
                {
                    if (waiter.Waitable != null)
                    {
                        progress = Mathf.Max(progress, waiter.Waitable.Progress);
                    }

                    continue;
                }

                bool moveNext;
                bool thisHasMoveError = false;
                try
                {
                    moveNext = waiter.Enumerator.MoveNext();
                }
                catch (Exception e)
                {
                    Debug.LogException(e.InnerException ?? e);
                    moveNext = false;
                    thisHasMoveError = true;
                    userData.WaiterHasError = true;
                    userData.ResultErrors.Add(e.InnerException?.Message ?? e.Message);
                }

                if (thisHasMoveError)
                {
                    waiter.Waitable = null;
                }
                else
                {
                    waiter.CheckCurrent();
                }

                if (!moveNext)
                {
                    finishedEnumerators.Add(waiter);

                    if (!thisHasMoveError)
                    {
                        userData.WaiterHasFinished = true;
                    }
                }
            }

            userData.Enumerators.RemoveAll(each => finishedEnumerators.Contains(each));

            bool stillHaveRunner = userData.Enumerators.Count > 0;
            if (stillHaveRunner)
            {
                userData.Status = ButtonStatusIMGUI.Loading;
                userData.Progress = progress;
                userData.StatusHideAt = -1d;
                return;
            }

            userData.Progress = -1f;
            if (oldCounter <= 0)
            {
                return;
            }

            if (userData.WaiterHasError)
            {
                PlayStatusIMGUI(userData,
                    userData.WaiterHasFinished ? ButtonStatusIMGUI.Warning : ButtonStatusIMGUI.Error);
            }
            else
            {
                PlayStatusIMGUI(userData, ButtonStatusIMGUI.Ok);
            }
        }

        private void CloseResultIMGUI(ButtonUserDataIMGUI userData)
        {
            bool hadRunner = userData.Enumerators.Count > 0;
            userData.Enumerators.Clear();
            userData.ResultErrors.Clear();
            userData.ShowReturnValue = false;
            userData.ReturnValue = null;
            userData.Progress = -1f;

            if (hadRunner)
            {
                PlayStatusIMGUI(userData, ButtonStatusIMGUI.Pause);
            }
        }

        private void SetParameterValueIMGUI(ButtonUserDataIMGUI userData, int index, object newValue)
        {
            userData.ParameterValues[index] = newValue;
            userData.ResultErrors.Clear();
            userData.ShowReturnValue = false;
            userData.ReturnValue = null;
        }

        private bool HasReturnValueIMGUI(MethodInfo methodInfo)
        {
            return !_buttonAttribute.HideReturnValue
                   && methodInfo.ReturnType != typeof(void)
                   && !typeof(IEnumerator).IsAssignableFrom(methodInfo.ReturnType);
        }

        private static void NoBeforeSetIMGUI(object _)
        {
        }

        private static void PlayStatusIMGUI(ButtonUserDataIMGUI userData, ButtonStatusIMGUI status)
        {
            userData.Status = status;
            userData.Progress = -1f;
            userData.StatusHideAt = status == ButtonStatusIMGUI.Loading
                ? -1d
                : EditorApplication.timeSinceStartup + StatusDurationIMGUI;
        }

        private void DrawStatusIMGUI(Rect position, ButtonUserDataIMGUI userData)
        {
            if (userData.Status == ButtonStatusIMGUI.None)
            {
                return;
            }

            if (userData.Status != ButtonStatusIMGUI.Loading
                && userData.StatusHideAt > 0d
                && EditorApplication.timeSinceStartup > userData.StatusHideAt)
            {
                userData.Status = ButtonStatusIMGUI.None;
                userData.StatusHideAt = -1d;
                return;
            }

            Rect statusRect = new Rect(position)
            {
                x = position.x + 4f,
                y = position.y + (position.height - StatusSizeIMGUI) / 2f,
                width = StatusSizeIMGUI,
                height = StatusSizeIMGUI,
            };

            switch (userData.Status)
            {
                case ButtonStatusIMGUI.Loading:
                    userData.Loading.Draw(statusRect);
                    DrawProgressIMGUI(position, userData.Progress);
                    break;
                case ButtonStatusIMGUI.Ok:
                    DrawStatusIconIMGUI(statusRect, StatusOkIconIMGUI, StatusOkColorIMGUI);
                    break;
                case ButtonStatusIMGUI.Error:
                    DrawStatusIconIMGUI(statusRect, StatusErrorIconIMGUI, StatusErrorColorIMGUI);
                    break;
                case ButtonStatusIMGUI.Warning:
                    DrawStatusIconIMGUI(statusRect, StatusWarningIconIMGUI, StatusWarningColorIMGUI);
                    break;
                case ButtonStatusIMGUI.Pause:
                    DrawStatusIconIMGUI(statusRect, StatusPauseIconIMGUI, StatusPauseColorIMGUI);
                    break;
                case ButtonStatusIMGUI.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void DrawProgressIMGUI(Rect position, float progress)
        {
            if (progress < 0f)
            {
                return;
            }

            Rect progressBackRect = new Rect(position)
            {
                x = position.x + 2f,
                y = position.yMax - 2f,
                width = Mathf.Max(0f, position.width - 4f),
                height = 1f,
            };
            EditorGUI.DrawRect(progressBackRect, new Color(0f, 0f, 0f, 0.25f));

            Rect progressRect = new Rect(progressBackRect)
            {
                width = progressBackRect.width * Mathf.Clamp01(progress),
            };
            EditorGUI.DrawRect(progressRect, new Color(0f, 182f / 255f, 1f, 0.75f));
        }

        private static void DrawStatusIconIMGUI(Rect texRect, string iconName, string iconColor)
        {
            Texture2D texture = GetStatusIconIMGUI(iconName);
            if (texture == null)
            {
                return;
            }

            using (new GUIColorScoop(Colors.GetColorByStringPresent(iconColor)))
            {
                GUI.DrawTexture(texRect, texture, ScaleMode.ScaleToFit, true);
            }
        }

        private static Texture2D GetStatusIconIMGUI(string iconName)
        {
            if (StatusIconCacheIMGUI.TryGetValue(iconName, out Texture2D texture) && texture != null)
            {
                return texture;
            }

            texture = Util.LoadResource<Texture2D>(iconName);
            if (texture != null)
            {
                StatusIconCacheIMGUI[iconName] = texture;
            }

            return texture;
        }
    }
}

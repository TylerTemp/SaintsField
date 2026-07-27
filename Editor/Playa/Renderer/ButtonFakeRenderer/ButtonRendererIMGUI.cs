using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using SaintsField.Editor.Core;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Utils;
using SaintsField.Editor.Utils.IMGUIEditDrawer;
using SaintsField.Editor.Utils.WaitableUtils;
using SaintsField.Playa;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;

#if SAINTSFIELD_UNITASK && !SAINTSFIELD_UNITASK_DISABLE
using Cysharp.Threading.Tasks;
#endif

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
            public AsyncReturnType ReturnAsync;
            public Type ReturnAsyncValueType;

            public string Xml;
            public string Callback;
            public readonly RichTextDrawer RichTextDrawer = new RichTextDrawer();
            public IReadOnlyList<RichTextDrawer.RichTextChunk> RichTextChunks;
            public string RichTextChunksXml;

            public readonly List<Waiter> Enumerators = new List<Waiter>();
            public bool WaiterHasError;
            public bool WaiterHasFinished;
            public bool WaiterHasCancel;

            public readonly List<string> ResultErrors = new List<string>();
            public bool ShowReturnValue;
            public Type ReturnType;
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

            (_buttonUserDataIMGUI.ReturnAsync, _buttonUserDataIMGUI.ReturnAsyncValueType) =
                GetAsyncReturnInfo(methodInfo.ReturnType);

            return _buttonUserDataIMGUI;
        }

        public override void OnDestroyIMGUI()
        {
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

            #region Tick Enumerators

            if (userData.Enumerators.Count > 0)
            {
                EditorApplication.QueuePlayerLoopUpdate();

                List<Waiter> finishedEnumerators = new List<Waiter>();
                int oldCounter = userData.Enumerators.Count;
                float progress = -1f;

                foreach (Waiter waiter in userData.Enumerators)
                {
                    waiter.Update();

                    bool subWaiterDone = waiter.SubWaiterDone();
                    if (!subWaiterDone)
                    {
                        float curProgress = waiter.GetProgress();
                        if (curProgress >= 0)
                        {
                            progress = Mathf.Max(progress, curProgress);
                        }

                        continue;
                    }

                    Waiter.MoveNextResult moveNext = waiter.MoveNext();
                    if (moveNext.Exception != null)
                    {
                        Debug.LogException(moveNext.Exception.InnerException ?? moveNext.Exception);
                        userData.WaiterHasError = true;
                        userData.ResultErrors.Add(moveNext.Exception.InnerException?.Message ?? moveNext.Exception.Message);
                    }

                    if (moveNext.Exception == null && moveNext.Status == Waiter.MoveNextStatus.Pending)
                    {
                        waiter.CheckCurrentNeedWaiter();
                    }

                    if (!_buttonAttribute.HideReturnValue
                        && moveNext.Status == Waiter.MoveNextStatus.Completed
                        && moveNext.ReturnType != null
                        && !userData.ShowReturnValue)
                    {
                        userData.ReturnType = moveNext.ReturnType;
                        userData.ReturnValue = moveNext.ReturnValue;
                        userData.ShowReturnValue = true;
                    }

                    if (moveNext.Status != Waiter.MoveNextStatus.Pending)
                    {
                        finishedEnumerators.Add(waiter);

                        if (moveNext.Status == Waiter.MoveNextStatus.Completed)
                        {
                            userData.WaiterHasFinished = true;
                        }
                        else if (moveNext.Status == Waiter.MoveNextStatus.Cancelled)
                        {
                            userData.WaiterHasCancel = true;
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
                }
                else
                {
                    userData.Progress = -1f;
                    if (oldCounter > 0)
                    {
                        if (userData.WaiterHasError)
                        {
                            PlayStatusIMGUI(userData,
                                userData.WaiterHasFinished ? ButtonStatusIMGUI.Warning : ButtonStatusIMGUI.Error);
                        }
                        else if (userData.WaiterHasCancel)
                        {
                            PlayStatusIMGUI(userData, ButtonStatusIMGUI.Pause);
                        }
                        else
                        {
                            PlayStatusIMGUI(userData, ButtonStatusIMGUI.Ok);
                        }
                    }
                }
            }

            #endregion

            float parameterHeight = GetParametersHeightIMGUI(userData, position.width);
            (Rect parametersRect, Rect leftRect) = RectUtils.SplitHeightRect(position, parameterHeight);
            if (parameterHeight > Mathf.Epsilon)
            {
                #region Draw Parameters

                GUI.Box(parametersRect, GUIContent.none);

                Rect contentRect = new Rect(parametersRect)
                {
                    x = parametersRect.x + PaddingBox,
                    y = parametersRect.y + PaddingBox,
                    width = Mathf.Max(0f, parametersRect.width - PaddingBox * 2f),
                    height = Mathf.Max(0f, parametersRect.height - PaddingBox * 2f),
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

                #endregion
            }

            (Rect buttonRect, Rect resultRect) =
                RectUtils.SplitHeightRect(leftRect, SaintsPropertyDrawer.SingleLineHeight);

            #region Draw Button Area

            Rect mainButtonRect = buttonRect;
            bool showCloseButton = HasResultIMGUI(userData) || userData.Enumerators.Count > 0;
            Rect closeButtonRect = default;
            if (showCloseButton)
            {
                mainButtonRect.width = Mathf.Max(0f, mainButtonRect.width - CloseButtonWidthIMGUI);
                closeButtonRect = new Rect(buttonRect)
                {
                    x = mainButtonRect.xMax,
                    width = CloseButtonWidthIMGUI,
                };
            }

            using (new EditorGUI.DisabledScope(preCheckResult.IsDisabled))
            {
                if (GUI.Button(mainButtonRect, GUIContent.none))
                {
                    #region Invoke Button

                    userData.ResultErrors.Clear();
                    userData.ShowReturnValue = false;
                    userData.ReturnType = null;
                    userData.ReturnValue = null;
                    userData.Enumerators.Clear();
                    userData.WaiterHasError = false;
                    userData.WaiterHasFinished = false;
                    userData.WaiterHasCancel = false;
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
                    }
                    else
                    {
                        if (!_buttonAttribute.HideReturnValue
                            && userData.MethodInfo.ReturnType != typeof(void)
                            && !typeof(IEnumerator).IsAssignableFrom(userData.MethodInfo.ReturnType)
                            && !typeof(Task).IsAssignableFrom(userData.MethodInfo.ReturnType)
                            && userData.ReturnAsync == AsyncReturnType.None
                           )
                        {
                            userData.ReturnType = userData.MethodInfo.ReturnType;
                            userData.ReturnValue = returnValues[0];
                            userData.ShowReturnValue = true;
                        }

                        foreach (object returnValue in returnValues)
                        {
                            switch (returnValue)
                            {
#if UNITY_6000_0_OR_NEWER
                                case Awaitable awaitable:
                                    userData.Enumerators.Add(new Waiter(awaitable));
                                    break;
#endif
                                case IEnumerator enumerator:
                                    userData.Enumerators.Add(new Waiter(enumerator));
                                    break;
                                case Task task:
                                    userData.Enumerators.Add(new Waiter(task));
                                    break;
#if SAINTSFIELD_UNITASK && !SAINTSFIELD_UNITASK_DISABLE
                                case UniTask uniTask:
                                    userData.Enumerators.Add(new Waiter(uniTask));
                                    break;
#endif
                                default:
                                    switch (userData.ReturnAsync)
                                    {
                                        case AsyncReturnType.None:
                                            break;
#if UNITY_6000_0_OR_NEWER
                                        case AsyncReturnType.Awaitable:
                                            userData.Enumerators.Add(Waiter.AwaitableT(returnValue,
                                                userData.ReturnAsyncValueType));
                                            break;
#endif
#if SAINTSFIELD_UNITASK && !SAINTSFIELD_UNITASK_DISABLE
                                        case AsyncReturnType.UniTask:
                                            userData.Enumerators.Add(Waiter.UniTaskWithValue(returnValue,
                                                userData.ReturnAsyncValueType));
                                            break;
#endif
                                        default:
                                            throw new ArgumentOutOfRangeException(nameof(userData.ReturnAsync),
                                                userData.ReturnAsync, null);
                                    }
                                    break;
                            }
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

                    #endregion
                }

                #region Draw Button Label

                IReadOnlyList<RichTextDrawer.RichTextChunk> richTextChunks;
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
                    richTextChunks = userData.RichTextChunks;
                }
                else
                {
                    if (useXml is null)
                    {
                        useXml = ObjectNames.NicifyVariableName(userData.MethodInfo.Name);
                    }

                    if (userData.RichTextChunks != null && userData.RichTextChunksXml == useXml)
                    {
                        richTextChunks = userData.RichTextChunks;
                    }
                    else
                    {
                        userData.Xml = useXml;
                        userData.RichTextChunksXml = useXml;
                        userData.RichTextChunks = RichTextDrawer.ParseRichXmlWithProvider(useXml, this).ToArray();
                        richTextChunks = userData.RichTextChunks;
                    }
                }

                if (richTextChunks.Count > 0)
                {
                    GUIContent oldLabel = new GUIContent(ObjectNames.NicifyVariableName(FieldWithInfo.MethodInfo.Name));
                    float drawNeedWidth = userData.RichTextDrawer.GetWidth(oldLabel, mainButtonRect.height, richTextChunks);
                    Rect drawRect = drawNeedWidth > mainButtonRect.width
                        ? mainButtonRect
                        : new Rect(mainButtonRect.x + (mainButtonRect.width - drawNeedWidth) / 2f, mainButtonRect.y,
                            drawNeedWidth, mainButtonRect.height);
                    userData.RichTextDrawer.DrawChunks(drawRect, richTextChunks);
                }

                #endregion

                if (showCloseButton && GUI.Button(closeButtonRect, "x"))
                {
                    #region Close Result

                    bool hadRunner = userData.Enumerators.Count > 0;
                    userData.Enumerators.Clear();
                    userData.ResultErrors.Clear();
                    userData.ShowReturnValue = false;
                    userData.ReturnType = null;
                    userData.ReturnValue = null;
                    userData.Progress = -1f;

                    if (hadRunner)
                    {
                        PlayStatusIMGUI(userData, ButtonStatusIMGUI.Pause);
                    }

                    #endregion
                }
            }

            #region Draw Status

            if (userData.Status != ButtonStatusIMGUI.None)
            {
                if (userData.Status != ButtonStatusIMGUI.Loading
                    && userData.StatusHideAt > 0d
                    && EditorApplication.timeSinceStartup > userData.StatusHideAt)
                {
                    userData.Status = ButtonStatusIMGUI.None;
                    userData.StatusHideAt = -1d;
                }
                else
                {
                    Rect statusRect = new Rect(buttonRect)
                    {
                        x = buttonRect.x + 4f,
                        y = buttonRect.y + (buttonRect.height - StatusSizeIMGUI) / 2f,
                        width = StatusSizeIMGUI,
                        height = StatusSizeIMGUI,
                    };

                    switch (userData.Status)
                    {
                        case ButtonStatusIMGUI.Loading:
                            userData.Loading.Draw(statusRect);
                            if (userData.Progress >= 0f)
                            {
                                #region Draw Progress

                                Rect progressBackRect = new Rect(buttonRect)
                                {
                                    x = buttonRect.x + 2f,
                                    y = buttonRect.yMax - 2f,
                                    width = Mathf.Max(0f, buttonRect.width - 4f),
                                    height = 1f,
                                };
                                EditorGUI.DrawRect(progressBackRect, new Color(0f, 0f, 0f, 0.25f));

                                Rect progressRect = new Rect(progressBackRect)
                                {
                                    width = progressBackRect.width * Mathf.Clamp01(userData.Progress),
                                };
                                EditorGUI.DrawRect(progressRect, new Color(0f, 182f / 255f, 1f, 0.75f));

                                #endregion
                            }
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
            }

            #endregion

            #endregion

            float resultHeight = GetResultHeightIMGUI(userData, position.width);
            if (resultHeight > Mathf.Epsilon)
            {
                #region Draw Result

                Rect drawResultRect = new Rect(resultRect)
                {
                    height = resultHeight,
                };

                GUI.Box(drawResultRect, GUIContent.none);

                Rect contentRect = new Rect(drawResultRect)
                {
                    x = drawResultRect.x + PaddingBox,
                    y = drawResultRect.y + PaddingBox,
                    width = Mathf.Max(0f, drawResultRect.width - PaddingBox * 2f),
                    height = Mathf.Max(0f, drawResultRect.height - PaddingBox * 2f),
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

                if (userData.ShowReturnValue)
                {
                    float returnHeight = IMGUIEdit.GetPropertyHeight(
                        "[return]",
                        userData.ReturnType,
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
                        userData.ReturnType,
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

                #endregion
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
                    userData.ReturnType,
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

        private void SetParameterValueIMGUI(ButtonUserDataIMGUI userData, int index, object newValue)
        {
            userData.ParameterValues[index] = newValue;
            userData.ResultErrors.Clear();
            userData.ShowReturnValue = false;
            userData.ReturnType = null;
            userData.ReturnValue = null;
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

        private static void DrawStatusIconIMGUI(Rect texRect, string iconName, string iconColor)
        {
            Texture2D texture;
            if (!StatusIconCacheIMGUI.TryGetValue(iconName, out texture) || texture == null)
            {
                texture = Util.LoadResource<Texture2D>(iconName);
                if (texture != null)
                {
                    StatusIconCacheIMGUI[iconName] = texture;
                }
            }

            if (texture == null)
            {
                return;
            }

            using (new GUIColorScoop(Colors.GetColorByStringPresent(iconColor)))
            {
                GUI.DrawTexture(texRect, texture, ScaleMode.ScaleToFit, true);
            }
        }
    }
}

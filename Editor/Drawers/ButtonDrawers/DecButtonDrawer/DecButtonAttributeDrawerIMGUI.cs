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
using SaintsField.Interfaces;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;

#if SAINTSFIELD_UNITASK && !SAINTSFIELD_UNITASK_DISABLE
using Cysharp.Threading.Tasks;
#endif

namespace SaintsField.Editor.Drawers.ButtonDrawers.DecButtonDrawer
{
    public partial class DecButtonAttributeDrawer
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

        private sealed class ButtonReturnValueIMGUI
        {
            public MethodInfo MethodInfo;
            public Type ReturnType;
            public object Parent;
            public object Value;
        }

        private sealed class ButtonInfoIMGUI
        {
            public string LabelError = "";
            public string Xml;
            public string Callback;
            public readonly RichTextDrawer RichTextDrawer = new RichTextDrawer();
            public IReadOnlyList<RichTextDrawer.RichTextChunk> RichTextChunks;
            public string RichTextChunksXml;

            public readonly List<string> ResultErrors = new List<string>();
            public readonly List<ButtonReturnValueIMGUI> ReturnValues = new List<ButtonReturnValueIMGUI>();
            public readonly List<Waiter> Enumerators = new List<Waiter>();
            public readonly Dictionary<Waiter, (MethodInfo methodInfo, object parent)> WaiterReturnTargets =
                new Dictionary<Waiter, (MethodInfo methodInfo, object parent)>();
            public bool WaiterHasError;
            public bool WaiterHasFinished;
            public bool WaiterHasCancel;

            public ButtonStatusIMGUI Status;
            public double StatusHideAt = -1d;
            public float Progress = -1f;
            public readonly IMGUILoading Loading = new IMGUILoading();
        }

        private const float PaddingWidthIMGUI = 3f;
        private const float CloseButtonWidthIMGUI = 18f;
        private const float StatusSizeIMGUI = 14f;
        private const float StatusDurationIMGUI = 2f;
        private const string ReturnLabelIMGUI = "<color=green>[return]</color>";

        private const string StatusOkIconIMGUI = "check.png";
        private const string StatusOkColorIMGUI = "#49FF7B";
        private const string StatusErrorIconIMGUI = "close.png";
        private const string StatusErrorColorIMGUI = "#FF2D17";
        private const string StatusWarningIconIMGUI = "console.warnicon";
        private const string StatusWarningColorIMGUI = null;
        private const string StatusPauseIconIMGUI = "d_PauseButton";
        private const string StatusPauseColorIMGUI = "#9717FF";

        private static readonly Dictionary<string, ButtonInfoIMGUI> InfoCacheIMGUI =
            new Dictionary<string, ButtonInfoIMGUI>();

        private static readonly Dictionary<string, Texture2D> StatusIconCacheIMGUI =
            new Dictionary<string, Texture2D>();

        protected readonly RichTextDrawer RichTextDrawer = new RichTextDrawer();

        private static ButtonInfoIMGUI EnsureKey(SerializedProperty property, int index)
        {
            string key = MakeKeyIMGUI(property, index);
            if (InfoCacheIMGUI.TryGetValue(key, out ButtonInfoIMGUI buttonInfo))
            {
                return buttonInfo;
            }

            buttonInfo = new ButtonInfoIMGUI();
            InfoCacheIMGUI[key] = buttonInfo;
            NoLongerInspectingWatch(property.serializedObject.targetObject, key, () => InfoCacheIMGUI.Remove(key));
            return buttonInfo;
        }

        private static string MakeKeyIMGUI(SerializedProperty property, int index) =>
            $"{SerializedUtils.GetUniqueId(property)}[{index}]";

        protected float GetButtonHeightIMGUI() => EditorGUIUtility.singleLineHeight;

        protected void UpdateButtonLabelIMGUI(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            FieldInfo info, object parent)
        {
            UpdateLabelIMGUI(EnsureKey(property, index), property, saintsAttribute, info, parent);
        }

        protected float GetResultHeightIMGUI(SerializedProperty property, int index, float width)
        {
            ButtonInfoIMGUI buttonInfo = EnsureKey(property, index);
            float height = 0f;
            foreach (string error in GetDisplayErrorsIMGUI(buttonInfo))
            {
                height += ImGuiHelpBox.GetHeight(error, width, MessageType.Error);
            }

            foreach ((ButtonReturnValueIMGUI returnValue, int returnIndex) in buttonInfo.ReturnValues.WithIndex())
            {
                height += IMGUIEdit.GetPropertyHeight(
                    ReturnLabelIMGUI,
                    returnValue.ReturnType,
                    returnValue.Value,
                    NoBeforeSetIMGUI,
                    _ => { },
                    false,
                    InHorizontalLayout,
                    ReflectCache.GetCustomAttributes(returnValue.MethodInfo),
                    new[] { returnValue.Parent },
                    this,
                    $"{MakeKeyIMGUI(property, index)}.[return].{returnIndex}");
            }

            return height;
        }

        protected Rect DrawResultIMGUI(Rect position, SerializedProperty property, int index)
        {
            ButtonInfoIMGUI buttonInfo = EnsureKey(property, index);
            Rect leftRect = position;
            foreach (string error in GetDisplayErrorsIMGUI(buttonInfo))
            {
                leftRect = ImGuiHelpBox.Draw(leftRect, error, MessageType.Error);
            }

            foreach ((ButtonReturnValueIMGUI returnValue, int returnIndex) in buttonInfo.ReturnValues.WithIndex())
            {
                float height = IMGUIEdit.GetPropertyHeight(
                    ReturnLabelIMGUI,
                    returnValue.ReturnType,
                    returnValue.Value,
                    NoBeforeSetIMGUI,
                    _ => { },
                    false,
                    InHorizontalLayout,
                    ReflectCache.GetCustomAttributes(returnValue.MethodInfo),
                    new[] { returnValue.Parent },
                    this,
                    $"{MakeKeyIMGUI(property, index)}.[return].{returnIndex}");

                Rect returnRect = new Rect(leftRect)
                {
                    height = height,
                };

                IMGUIEdit.OnGUI(
                    returnRect,
                    ReturnLabelIMGUI,
                    returnValue.ReturnType,
                    returnValue.Value,
                    NoBeforeSetIMGUI,
                    _ => { },
                    false,
                    InHorizontalLayout,
                    ReflectCache.GetCustomAttributes(returnValue.MethodInfo),
                    new[] { returnValue.Parent },
                    this,
                    $"{MakeKeyIMGUI(property, index)}.[return].{returnIndex}");

                leftRect.y += height;
                leftRect.height = Mathf.Max(0f, leftRect.height - height);
            }

            return leftRect;
        }

        protected bool HasResultIMGUI(SerializedProperty property, int index)
        {
            ButtonInfoIMGUI buttonInfo = EnsureKey(property, index);
            return GetDisplayErrorsIMGUI(buttonInfo).Any() || buttonInfo.ReturnValues.Count > 0;
        }

        protected float GetButtonWidthIMGUI(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            ButtonInfoIMGUI buttonInfo = EnsureKey(property, index);
            UpdateLabelIMGUI(buttonInfo, property, saintsAttribute, info, parent);

            float extraWidth = PaddingWidthIMGUI * 2f;
            if (HasVisibleStatusIMGUI(buttonInfo))
            {
                extraWidth += StatusSizeIMGUI + PaddingWidthIMGUI;
            }

            if (ShouldShowCloseButtonIMGUI(buttonInfo))
            {
                extraWidth += CloseButtonWidthIMGUI;
            }

            float labelWidth = buttonInfo.RichTextChunks == null
                ? 10f
                : buttonInfo.RichTextDrawer.GetWidth(label, position.height, buttonInfo.RichTextChunks);

            return Mathf.Min(position.width, Mathf.Max(10f, labelWidth) + extraWidth);
        }

        protected Rect Draw(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute,
            int index, FieldInfo info, object parent)
        {
            ButtonInfoIMGUI buttonInfo = EnsureKey(property, index);
            UpdateLabelIMGUI(buttonInfo, property, saintsAttribute, info, parent);
            TickEnumeratorsIMGUI(buttonInfo);

            (Rect buttonRect, Rect leftRect) = RectUtils.SplitHeightRect(position, EditorGUIUtility.singleLineHeight);
            DrawButtonIMGUI(buttonRect, property, label, saintsAttribute, info, parent, buttonInfo);
            return leftRect;
        }

        private static IEnumerable<string> GetDisplayErrorsIMGUI(ButtonInfoIMGUI buttonInfo)
        {
            if (!string.IsNullOrEmpty(buttonInfo.LabelError))
            {
                yield return buttonInfo.LabelError;
            }

            foreach (string error in buttonInfo.ResultErrors.Where(each => !string.IsNullOrEmpty(each)))
            {
                yield return error;
            }
        }

        private static bool ShouldShowCloseButtonIMGUI(ButtonInfoIMGUI buttonInfo) =>
            buttonInfo.Enumerators.Count > 0 || buttonInfo.ReturnValues.Count > 0 || buttonInfo.ResultErrors.Count > 0;

        private void UpdateLabelIMGUI(ButtonInfoIMGUI buttonInfo, SerializedProperty property,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            DecButtonAttribute decButtonAttribute = (DecButtonAttribute)saintsAttribute;
            buttonInfo.Callback = decButtonAttribute.IsCallback ? decButtonAttribute.ButtonLabel : "";

            string useXml = decButtonAttribute.ButtonLabel ?? ObjectNames.NicifyVariableName(decButtonAttribute.FuncName);
            if (!string.IsNullOrEmpty(buttonInfo.Callback))
            {
                (string xmlError, string newXml) = RichTextDrawer.GetLabelXml(property, decButtonAttribute.ButtonLabel,
                    decButtonAttribute.IsCallback, info, parent);
                buttonInfo.LabelError = xmlError;
                useXml = string.IsNullOrEmpty(newXml)
                    ? ObjectNames.NicifyVariableName(decButtonAttribute.FuncName)
                    : newXml;
            }
            else
            {
                buttonInfo.LabelError = "";
            }

            if (useXml is null)
            {
                useXml = ObjectNames.NicifyVariableName(decButtonAttribute.FuncName);
            }

            bool needLiveRefresh = useXml.Contains("<field");
            if (buttonInfo.RichTextChunks != null
                && buttonInfo.RichTextChunksXml == useXml
                && !needLiveRefresh)
            {
                return;
            }

            buttonInfo.Xml = useXml;
            buttonInfo.RichTextChunksXml = useXml;
            buttonInfo.RichTextChunks = RichTextDrawer.ParseRichXmlWithProvider(useXml, this).ToArray();
        }

        private void DrawButtonIMGUI(Rect buttonRect, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent, ButtonInfoIMGUI buttonInfo)
        {
            Rect mainButtonRect = buttonRect;
            bool showCloseButton = ShouldShowCloseButtonIMGUI(buttonInfo);
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

            if (GUI.Button(mainButtonRect, GUIContent.none))
            {
                InvokeButtonIMGUI(buttonInfo, property, saintsAttribute, info, parent);
            }

            DrawButtonLabelIMGUI(mainButtonRect, label, buttonInfo);

            if (showCloseButton && GUI.Button(closeButtonRect, "x"))
            {
                CloseButtonIMGUI(buttonInfo);
            }

            DrawStatusIMGUI(buttonRect, buttonInfo);
        }

        private static void DrawButtonLabelIMGUI(Rect buttonRect, GUIContent label, ButtonInfoIMGUI buttonInfo)
        {
            if (buttonInfo.RichTextChunks == null || buttonInfo.RichTextChunks.Count == 0)
            {
                return;
            }

            Rect labelRect = buttonRect;
            if (HasVisibleStatusIMGUI(buttonInfo))
            {
                float statusOffset = StatusSizeIMGUI + PaddingWidthIMGUI * 2f;
                labelRect.x += statusOffset;
                labelRect.width = Mathf.Max(0f, labelRect.width - statusOffset);
            }

            float drawNeedWidth = buttonInfo.RichTextDrawer.GetWidth(label, labelRect.height, buttonInfo.RichTextChunks);
            Rect drawRect = drawNeedWidth > labelRect.width
                ? labelRect
                : new Rect(labelRect.x + (labelRect.width - drawNeedWidth) / 2f, labelRect.y, drawNeedWidth,
                    labelRect.height);

            buttonInfo.RichTextDrawer.DrawChunks(drawRect, buttonInfo.RichTextChunks);
        }

        private void InvokeButtonIMGUI(ButtonInfoIMGUI buttonInfo, SerializedProperty property,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            DecButtonAttribute decButtonAttribute = (DecButtonAttribute)saintsAttribute;
            CleanResultIMGUI(buttonInfo);
            buttonInfo.Enumerators.Clear();
            buttonInfo.WaiterHasError = false;
            buttonInfo.WaiterHasFinished = false;
            buttonInfo.WaiterHasCancel = false;
            buttonInfo.Progress = -1f;

            List<string> errors = new List<string>();
            List<(MethodInfo methodInfo, object result)> results = new List<(MethodInfo methodInfo, object result)>();

            foreach ((string eachError, MemberInfo memberInfo, object buttonResult) in CallButtonFunc(property,
                         decButtonAttribute.FuncName, info, parent))
            {
                if (eachError == "")
                {
                    results.Add(((MethodInfo)memberInfo, buttonResult));
                }
                else
                {
                    errors.Add(eachError);
                }
            }

            foreach (string error in errors)
            {
                buttonInfo.ResultErrors.Add(error);
            }

            object refreshedParent = null;
            foreach ((MethodInfo methodInfo, object result) in results)
            {
#if SAINTSFIELD_UNITASK && !SAINTSFIELD_UNITASK_DISABLE
                (bool returnIsUniTask, Type returnUniTaskValueType) = GetUniTaskReturnInfo(methodInfo.ReturnType);
#endif
                if (result is IEnumerator enumerator)
                {
                    buttonInfo.Enumerators.Add(new Waiter(enumerator));
                }
                else if (result is Task task)
                {
                    Waiter waiter = new Waiter(task);
                    buttonInfo.Enumerators.Add(waiter);
                    if (!decButtonAttribute.HideReturnValue)
                    {
                        refreshedParent ??= SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
                        buttonInfo.WaiterReturnTargets[waiter] = (methodInfo, refreshedParent);
                    }
                }
#if SAINTSFIELD_UNITASK && !SAINTSFIELD_UNITASK_DISABLE
                else if (result is UniTask uniTask)
                {
                    buttonInfo.Enumerators.Add(new Waiter(uniTask));
                }
                else if (returnIsUniTask && returnUniTaskValueType != null)
                {
                    Waiter waiter = Waiter.UniTaskWithValue(result, returnUniTaskValueType);
                    buttonInfo.Enumerators.Add(waiter);
                    if (!decButtonAttribute.HideReturnValue)
                    {
                        refreshedParent ??= SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
                        buttonInfo.WaiterReturnTargets[waiter] = (methodInfo, refreshedParent);
                    }
                }
#endif
                else if (!decButtonAttribute.HideReturnValue && result != null && result.GetType() != typeof(void))
                {
                    refreshedParent ??= SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
                    buttonInfo.ReturnValues.Add(new ButtonReturnValueIMGUI
                    {
                        MethodInfo = methodInfo,
                        ReturnType = methodInfo.ReturnType,
                        Parent = refreshedParent,
                        Value = result,
                    });
                }
            }

            PlayStatusIMGUI(buttonInfo, buttonInfo.Enumerators.Count > 0
                ? ButtonStatusIMGUI.Loading
                : errors.Count > 0
                    ? ButtonStatusIMGUI.Error
                    : ButtonStatusIMGUI.Ok);
        }

        private static void CleanResultIMGUI(ButtonInfoIMGUI buttonInfo)
        {
            buttonInfo.ResultErrors.Clear();
            buttonInfo.ReturnValues.Clear();
            buttonInfo.WaiterReturnTargets.Clear();
            buttonInfo.Status = ButtonStatusIMGUI.None;
            buttonInfo.StatusHideAt = -1d;
        }

        private static void CloseButtonIMGUI(ButtonInfoIMGUI buttonInfo)
        {
            bool hadRunner = buttonInfo.Enumerators.Count > 0;
            buttonInfo.Enumerators.Clear();
            buttonInfo.WaiterReturnTargets.Clear();
            buttonInfo.ResultErrors.Clear();
            buttonInfo.ReturnValues.Clear();
            buttonInfo.Progress = -1f;

            if (hadRunner)
            {
                PlayStatusIMGUI(buttonInfo, ButtonStatusIMGUI.Pause);
            }
        }

        private static void TickEnumeratorsIMGUI(ButtonInfoIMGUI buttonInfo)
        {
            if (Event.current == null || Event.current.type != EventType.Repaint || buttonInfo.Enumerators.Count == 0)
            {
                return;
            }

            List<Waiter> finishedEnumerators = new List<Waiter>();
            int oldCounter = buttonInfo.Enumerators.Count;
            float progress = -1f;

            foreach (Waiter waiter in buttonInfo.Enumerators)
            {
                waiter.Update();

                if (!waiter.Done())
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
                    buttonInfo.WaiterHasError = true;
                    buttonInfo.ResultErrors.Add(moveNext.Exception.InnerException?.Message ?? moveNext.Exception.Message);
                }

                if (moveNext.Exception == null && moveNext.Status == Waiter.MoveNextStatus.Pending)
                {
                    waiter.CheckCurrentNeedWaiter();
                }

                if (moveNext.Status == Waiter.MoveNextStatus.Completed
                    && moveNext.ReturnType != null
                    && buttonInfo.WaiterReturnTargets.TryGetValue(waiter, out (MethodInfo methodInfo, object parent) returnTarget))
                {
                    buttonInfo.ReturnValues.Add(new ButtonReturnValueIMGUI
                    {
                        MethodInfo = returnTarget.methodInfo,
                        ReturnType = moveNext.ReturnType,
                        Parent = returnTarget.parent,
                        Value = moveNext.ReturnValue,
                    });
                }

                if (moveNext.Status != Waiter.MoveNextStatus.Pending)
                {
                    finishedEnumerators.Add(waiter);
                    if (moveNext.Status == Waiter.MoveNextStatus.Completed)
                    {
                        buttonInfo.WaiterHasFinished = true;
                    }
                    else if (moveNext.Status == Waiter.MoveNextStatus.Cancelled)
                    {
                        buttonInfo.WaiterHasCancel = true;
                    }
                }
            }

            buttonInfo.Enumerators.RemoveAll(each => finishedEnumerators.Contains(each));
            foreach (Waiter waiter in finishedEnumerators)
            {
                buttonInfo.WaiterReturnTargets.Remove(waiter);
            }

            bool stillHaveRunner = buttonInfo.Enumerators.Count > 0;
            if (stillHaveRunner)
            {
                buttonInfo.Status = ButtonStatusIMGUI.Loading;
                buttonInfo.Progress = progress;
                buttonInfo.StatusHideAt = -1d;
                return;
            }

            buttonInfo.Progress = -1f;
            if (oldCounter <= 0)
            {
                return;
            }

            if (buttonInfo.WaiterHasError)
            {
                PlayStatusIMGUI(buttonInfo,
                    buttonInfo.WaiterHasFinished ? ButtonStatusIMGUI.Warning : ButtonStatusIMGUI.Error);
            }
            else if (buttonInfo.WaiterHasCancel)
            {
                PlayStatusIMGUI(buttonInfo, ButtonStatusIMGUI.Pause);
            }
            else
            {
                PlayStatusIMGUI(buttonInfo, ButtonStatusIMGUI.Ok);
            }
        }

        private static void NoBeforeSetIMGUI(object _)
        {
        }

        private static void PlayStatusIMGUI(ButtonInfoIMGUI buttonInfo, ButtonStatusIMGUI status)
        {
            buttonInfo.Status = status;
            buttonInfo.Progress = -1f;
            buttonInfo.StatusHideAt = status == ButtonStatusIMGUI.Loading
                ? -1d
                : EditorApplication.timeSinceStartup + StatusDurationIMGUI;
        }

        private static bool HasVisibleStatusIMGUI(ButtonInfoIMGUI buttonInfo)
        {
            if (buttonInfo.Status == ButtonStatusIMGUI.None)
            {
                return false;
            }

            if (buttonInfo.Status != ButtonStatusIMGUI.Loading
                && buttonInfo.StatusHideAt > 0d
                && EditorApplication.timeSinceStartup > buttonInfo.StatusHideAt)
            {
                buttonInfo.Status = ButtonStatusIMGUI.None;
                buttonInfo.StatusHideAt = -1d;
                return false;
            }

            return true;
        }

        private static void DrawStatusIMGUI(Rect position, ButtonInfoIMGUI buttonInfo)
        {
            if (!HasVisibleStatusIMGUI(buttonInfo))
            {
                return;
            }

            Rect statusRect = new Rect(position)
            {
                x = position.x + PaddingWidthIMGUI,
                y = position.y + (position.height - StatusSizeIMGUI) / 2f,
                width = StatusSizeIMGUI,
                height = StatusSizeIMGUI,
            };

            switch (buttonInfo.Status)
            {
                case ButtonStatusIMGUI.Loading:
                    buttonInfo.Loading.Draw(statusRect);
                    DrawProgressIMGUI(position, buttonInfo.Progress);
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

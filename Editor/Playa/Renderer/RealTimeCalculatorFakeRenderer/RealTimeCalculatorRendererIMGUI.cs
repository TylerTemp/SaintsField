using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Utils;
using SaintsField.Editor.Utils.IMGUIEditDrawer;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Playa.Renderer.RealTimeCalculatorFakeRenderer
{
    public partial class RealTimeCalculatorRenderer
    {
        private sealed class DataPayloadIMGUI
        {
            public bool HasDrawer;
            public object Value;
            public bool IsGeneralCollection;
            public IReadOnlyList<object> OldCollection;
            public bool AlwaysCheckUpdate;
        }

        private const float PaddingBoxIMGUI = 3f;
        private const float SeparatorHeightIMGUI = 1f;
        private const float SeparatorMarginYIMGUI = 3f;
        private const double ImGuiValueRefreshInterval = 0.02d;

        private bool _imguiInit;
        private ParameterInfo[] _imguiParameters;
        private object[] _imguiParameterValues;
        private IReadOnlyList<Attribute>[] _imguiParameterAttributes;
        private IReadOnlyList<Attribute> _imguiReturnAttributes;
        private DataPayloadIMGUI _imguiData;
        private string _imguiTargetId;
        private string _imguiReturnLabel;
        private string _imguiInvokeError = "";
        private double _imguiNextRefreshTime = -1d;

        private void EnsureInitIMGUI()
        {
            if (_imguiInit)
            {
                return;
            }

            _imguiInit = true;

            MethodInfo methodInfo = FieldWithInfo.MethodInfo;
            _imguiParameters = methodInfo.GetParameters();
            _imguiParameterValues = _imguiParameters.Select(GetParameterDefaultValueIMGUI).ToArray();
            _imguiParameterAttributes = _imguiParameters
                .Select(each => (IReadOnlyList<Attribute>)each.GetCustomAttributes().OfType<Attribute>().ToArray())
                .ToArray();
            _imguiReturnAttributes = ReflectCache.GetCustomAttributes(methodInfo);
            _imguiTargetId = $"{FieldWithInfo.Targets[0].GetHashCode()}.{methodInfo.Name}";
            _imguiReturnLabel = NoLabel ? "" : GetName(FieldWithInfo);

            Type fieldType = methodInfo.ReturnType;
            bool isCollection = !typeof(UnityEngine.Object).IsAssignableFrom(fieldType)
                                && (fieldType.IsArray || typeof(IEnumerable).IsAssignableFrom(fieldType));
            _imguiData = new DataPayloadIMGUI
            {
                HasDrawer = false,
                Value = null,
                IsGeneralCollection = isCollection,
                OldCollection = null,
            };
        }

        private static object GetParameterDefaultValueIMGUI(ParameterInfo parameterInfo)
        {
            if (parameterInfo.HasDefaultValue)
            {
                return parameterInfo.DefaultValue;
            }

            return parameterInfo.ParameterType.IsValueType
                ? Activator.CreateInstance(parameterInfo.ParameterType)
                : null;
        }

        protected override float GetFieldHeightIMGUI(float width, PreCheckResult preCheckResult)
        {
            if (!preCheckResult.IsShown)
            {
                return 0f;
            }

            EnsureInitIMGUI();
            UpdateReturnValueIMGUI();

            if (_imguiParameters.Length == 0)
            {
                return GetReturnHeightIMGUI(width);
            }

            float contentWidth = Mathf.Max(1f, width - PaddingBoxIMGUI * 2f);
            return PaddingBoxIMGUI * 2f
                   + GetParametersHeightIMGUI(contentWidth)
                   + SeparatorMarginYIMGUI * 2f
                   + SeparatorHeightIMGUI
                   + GetReturnHeightIMGUI(contentWidth);
        }

        protected override void RenderPositionTargetIMGUI(Rect position, PreCheckResult preCheckResult)
        {
            if (!preCheckResult.IsShown)
            {
                return;
            }

            EnsureInitIMGUI();
            UpdateReturnValueIMGUI();

            using (new EditorGUI.DisabledScope(preCheckResult.IsDisabled))
            {
                if (_imguiParameters.Length == 0)
                {
                    DrawReturnIMGUI(position);
                    return;
                }

                DrawWithParametersIMGUI(position);
            }
        }

        private void UpdateReturnValueIMGUI()
        {
            double now = EditorApplication.timeSinceStartup;
            if (_imguiData.HasDrawer && now < _imguiNextRefreshTime)
            {
                return;
            }

            _imguiNextRefreshTime = now + ImGuiValueRefreshInterval;

            MethodInfo methodInfo = FieldWithInfo.MethodInfo;
            object value;
            try
            {
                object[] returnValues = FieldWithInfo.Targets
                    .Select(each => methodInfo.Invoke(each, _imguiParameterValues))
                    .ToArray();
                value = returnValues[0];
                _imguiInvokeError = "";
            }
            catch (Exception e)
            {
                _imguiInvokeError = e.InnerException?.Message ?? e.Message;
                _imguiData.HasDrawer = false;
                return;
            }

            bool valueIsNull = RuntimeUtil.IsNull(value);
            bool isEqual = _imguiData.AlwaysCheckUpdate
                ? false
                : _imguiData.HasDrawer && Util.GetIsEqual(_imguiData.Value, value);

            if (isEqual && _imguiData.IsGeneralCollection)
            {
                IReadOnlyList<object> oldCollection = _imguiData.OldCollection;
                if (oldCollection == null && valueIsNull)
                {
                }
                else if (oldCollection != null && valueIsNull)
                {
                    isEqual = false;
                }
                else if (oldCollection == null)
                {
                    isEqual = false;
                }
                else
                {
                    isEqual = oldCollection.SequenceEqual(((IEnumerable)value).Cast<object>());
                }
            }

            if (isEqual)
            {
                return;
            }

            _imguiData.Value = value;
            _imguiData.HasDrawer = true;
            _imguiData.AlwaysCheckUpdate = false;
            if (_imguiData.IsGeneralCollection)
            {
                _imguiData.OldCollection = !RuntimeUtil.IsNull(value) && value is IEnumerable ie
                    ? ie.Cast<object>().ToArray()
                    : null;
            }
        }

        private float GetParametersHeightIMGUI(float width)
        {
            return _imguiParameters
                .Select((each, index) => GetParameterHeightIMGUI(index, width))
                .Sum();
        }

        private float GetParameterHeightIMGUI(int index, float width)
        {
            ParameterInfo parameterInfo = _imguiParameters[index];
            return IMGUIEdit.GetPropertyHeight(
                parameterInfo.Name,
                parameterInfo.ParameterType,
                _imguiParameterValues[index],
                NoBeforeSetIMGUI,
                newValue => SetParameterValueIMGUI(index, newValue),
                false,
                InAnyHorizontalLayout,
                _imguiParameterAttributes[index],
                FieldWithInfo.Targets,
                this,
                $"{_imguiTargetId}.{parameterInfo.Name}");
        }

        private float GetReturnHeightIMGUI(float width)
        {
            if (_imguiInvokeError != "")
            {
                return ImGuiHelpBox.GetHeight(_imguiInvokeError, width, MessageType.Error);
            }

            MethodInfo methodInfo = FieldWithInfo.MethodInfo;
            Type valueType = RuntimeUtil.IsNull(_imguiData.Value)
                ? methodInfo.ReturnType
                : _imguiData.Value.GetType();
            return IMGUIEdit.GetPropertyHeight(
                _imguiReturnLabel,
                valueType,
                _imguiData.Value,
                NoBeforeSetIMGUI,
                _ => { },
                false,
                InAnyHorizontalLayout,
                _imguiReturnAttributes,
                FieldWithInfo.Targets,
                this,
                $"{_imguiTargetId}.{valueType.FullName}");
        }

        private void DrawWithParametersIMGUI(Rect position)
        {
            GUI.Box(position, GUIContent.none);

            Rect contentRect = new Rect(position)
            {
                x = position.x + PaddingBoxIMGUI,
                y = position.y + PaddingBoxIMGUI,
                width = Mathf.Max(0f, position.width - PaddingBoxIMGUI * 2f),
                height = Mathf.Max(0f, position.height - PaddingBoxIMGUI * 2f),
            };

            float y = contentRect.y;
            foreach ((ParameterInfo _, int index) in _imguiParameters.WithIndex())
            {
                float height = GetParameterHeightIMGUI(index, contentRect.width);
                Rect parameterRect = new Rect(contentRect)
                {
                    y = y,
                    height = height,
                };
                DrawParameterIMGUI(parameterRect, index);
                y += height;
            }

            Rect separatorRect = new Rect(contentRect)
            {
                y = y + SeparatorMarginYIMGUI,
                height = SeparatorHeightIMGUI,
            };
            EditorGUI.DrawRect(separatorRect, new Color(1f, 1f, 1f, 0.2f));
            y = separatorRect.yMax + SeparatorMarginYIMGUI;

            Rect returnRect = new Rect(contentRect)
            {
                y = y,
                height = Mathf.Max(0f, contentRect.yMax - y),
            };
            DrawReturnIMGUI(returnRect);
        }

        private void DrawParameterIMGUI(Rect position, int index)
        {
            ParameterInfo parameterInfo = _imguiParameters[index];
            IMGUIEdit.OnGUI(
                position,
                parameterInfo.Name,
                parameterInfo.ParameterType,
                _imguiParameterValues[index],
                NoBeforeSetIMGUI,
                newValue => SetParameterValueIMGUI(index, newValue),
                false,
                InAnyHorizontalLayout,
                _imguiParameterAttributes[index],
                FieldWithInfo.Targets,
                this,
                $"{_imguiTargetId}.{parameterInfo.Name}");
        }

        private void DrawReturnIMGUI(Rect position)
        {
            if (_imguiInvokeError != "")
            {
                ImGuiHelpBox.Draw(position, _imguiInvokeError, MessageType.Error);
                return;
            }

            MethodInfo methodInfo = FieldWithInfo.MethodInfo;
            Type valueType = RuntimeUtil.IsNull(_imguiData.Value)
                ? methodInfo.ReturnType
                : _imguiData.Value.GetType();
            IMGUIEdit.OnGUI(
                position,
                _imguiReturnLabel,
                valueType,
                _imguiData.Value,
                NoBeforeSetIMGUI,
                _ => { },
                false,
                InAnyHorizontalLayout,
                _imguiReturnAttributes,
                FieldWithInfo.Targets,
                this,
                $"{_imguiTargetId}.{valueType.FullName}");
        }

        private void SetParameterValueIMGUI(int index, object newValue)
        {
            _imguiParameterValues[index] = newValue;
            _imguiNextRefreshTime = -1d;
            _imguiData.AlwaysCheckUpdate = true;
        }

        private static void NoBeforeSetIMGUI(object _)
        {
        }
    }
}

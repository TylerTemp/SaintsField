using System;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Editor.Utils.WaitableUtils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Playa.Renderer.OnInspectorInitAttributeRenderer
{
    public partial class OnInspectorInitRenderer
    {
        private bool _initializedIMGUI;
        private MethodRunnerUtil.Payload[] _methodRunnersIMGUI;
        private string _errorIMGUI = "";

        public override void OnDestroyIMGUI()
        {
            _methodRunnersIMGUI = null;
        }

        protected override float GetFieldHeightIMGUI(float width, PreCheckResult preCheckResult)
        {
            EnsureInitializedIMGUI();
            TickMethodRunnersIMGUI();

            return _errorIMGUI == ""
                ? 0f
                : ImGuiHelpBox.GetHeight(_errorIMGUI, width, MessageType.Error);
        }

        protected override void RenderPositionTargetIMGUI(Rect position, PreCheckResult preCheckResult)
        {
            if (!preCheckResult.IsShown || _errorIMGUI == "")
            {
                return;
            }

            ImGuiHelpBox.Draw(position, _errorIMGUI, MessageType.Error);
        }

        private void EnsureInitializedIMGUI()
        {
            if (_initializedIMGUI)
            {
                return;
            }

            _initializedIMGUI = true;

            bool isStruct = ReflectUtils.TypeIsStruct(FieldWithInfo.Targets[0].GetType());
            int targetCount = FieldWithInfo.Targets.Count;
            MethodInfo methodInfo = FieldWithInfo.MethodInfo;
            MethodRunnerUtil.Payload[] methodRunners = new MethodRunnerUtil.Payload[targetCount];
            ParameterInfo[] parameters = methodInfo.GetParameters();
            object[] parameterValues = new object[parameters.Length];

            for (int index = 0; index < parameters.Length; index++)
            {
                ParameterInfo parameter = parameters[index];
                if (!parameter.HasDefaultValue)
                {
                    _errorIMGUI = $"Parameter {parameter.Name} on {methodInfo.Name} is not optional";
                    return;
                }

                parameterValues[index] = parameter.DefaultValue;
            }

            for (int index = 0; index < targetCount; index++)
            {
                object eachTarget = FieldWithInfo.Targets[index];
                (object rawMemberValue, object useTarget) = GetRefreshedTarget(FieldWithInfo, eachTarget);

                MethodRunnerUtil.Payload methodRunner =
                    MethodRunnerUtil.Invoke(methodInfo, useTarget, parameterValues);
                methodRunners[index] = methodRunner;
                if (methodRunner.Status == MethodRunnerUtil.RunStatus.Faulted)
                {
                    Debug.LogException(methodRunner.Exception);
                    _errorIMGUI =
                        $"{methodInfo.Name}: {methodRunner.Exception.InnerException?.Message ?? methodRunner.Exception.Message}";
                    return;
                }

                if (isStruct)
                {
                    BackWriteCallback(rawMemberValue, useTarget);
                }
            }

            foreach (MethodRunnerUtil.Payload methodRunner in methodRunners)
            {
                if (methodRunner.Status != MethodRunnerUtil.RunStatus.Pending)
                {
                    continue;
                }

                _methodRunnersIMGUI = methodRunners;
                break;
            }
        }

        private void TickMethodRunnersIMGUI()
        {
            MethodRunnerUtil.Payload[] methodRunners = _methodRunnersIMGUI;
            if (methodRunners == null)
            {
                return;
            }

            bool hasPendingRunner = false;
            foreach (MethodRunnerUtil.Payload methodRunner in methodRunners)
            {
                if (methodRunner.Status != MethodRunnerUtil.RunStatus.Pending)
                {
                    continue;
                }

                MethodRunnerUtil.RunStatus status = MethodRunnerUtil.Tick(methodRunner);
                if (status == MethodRunnerUtil.RunStatus.Pending)
                {
                    hasPendingRunner = true;
                    continue;
                }

                if (status == MethodRunnerUtil.RunStatus.Faulted)
                {
                    Exception error = methodRunner.Exception;
                    Debug.LogException(error);
                    _errorIMGUI =
                        $"{methodRunner.MethodInfo.Name}: {error.InnerException?.Message ?? error.Message}";
                }
            }

            if (!hasPendingRunner)
            {
                _methodRunnersIMGUI = null;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Editor.Utils.WaitableUtils;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.Renderer.OnInspectorInitAttributeRenderer
{
    public partial class OnInspectorInitRenderer
    {
        protected override bool AllowGuiColor => false;

        public override void OnDestroyUIToolkit()
        {
        }

        private MethodRunnerUtil.Payload[] _methodRunners;
        private HelpBox _helpBox;

        // this won't work because of how Unity renders
        // will not try to workaround it.
        // private readonly HashSet<object> processedObjects = new HashSet<object>();

        protected override (VisualElement target, bool needUpdate) CreateTargetUIToolkit(VisualElement inspectorRoot, VisualElement container)
        {
            bool isStruct = ReflectUtils.TypeIsStruct(FieldWithInfo.Targets[0].GetType());

            int targetCount = FieldWithInfo.Targets.Count;
            MethodInfo methodInfo = FieldWithInfo.MethodInfo;
            MethodRunnerUtil.Payload[] methodRunners = new MethodRunnerUtil.Payload[targetCount];
            ParameterInfo[] parameters = methodInfo.GetParameters();
            object[] parameterValues = new object[parameters.Length];

            for (int index = 0; index < parameters.Length; index++)
            {
                ParameterInfo para = parameters[index];
                if (!para.HasDefaultValue)
                {
                    return (
                        new HelpBox($"Parameter {para.Name} on {methodInfo.Name} is not optional",
                            HelpBoxMessageType.Error), false);
                }

                parameterValues[index] = para.DefaultValue;
            }

            // Exception error = null;

            for (int index = 0; index < targetCount; index++)
            {
                object eachTarget = FieldWithInfo.Targets[index];
                // if (!processedObjects.Add(eachTarget))  // avoid re-run on multiple selector
                // {
                //     continue;
                // }
                (object rawMemberValue, object useTarget) = GetRefreshedTarget(FieldWithInfo, eachTarget);

                MethodRunnerUtil.Payload methodRunner =
                    MethodRunnerUtil.Invoke(methodInfo, useTarget, parameterValues);
                methodRunners[index] = methodRunner;
                if (methodRunner.Status == MethodRunnerUtil.RunStatus.Faulted)
                {
                    Debug.LogException(methodRunner.Exception);
                    return (
                        new HelpBox($"{methodInfo.Name}: {methodRunner.Exception.InnerException?.Message ?? methodRunner.Exception.Message}",
                            HelpBoxMessageType.Error), false);
                }

                if (isStruct)
                {
                    BackWriteCallback(rawMemberValue, useTarget);
                }
            }

            bool needUpdate = false;
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (MethodRunnerUtil.Payload methodRunner in methodRunners)
            {
                // ReSharper disable once InvertIf
                if (methodRunner.Status == MethodRunnerUtil.RunStatus.Pending)
                {
                    needUpdate = true;
                    break;
                }
            }

            if (needUpdate)
            {
                _methodRunners = methodRunners;
            }

            _helpBox = new HelpBox("", HelpBoxMessageType.Error)
            {
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                    display = DisplayStyle.None,
                },
            };

            return (_helpBox, needUpdate);
        }

        protected override PreCheckResult OnUpdateUIToolKit(VisualElement root)
        {
            PreCheckResult result = base.OnUpdateUIToolKit(root);
            if (_methodRunners == null)
            {
                return result;
            }

            bool hasPendingRunner = false;
            foreach (MethodRunnerUtil.Payload methodRunner in _methodRunners)
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
                    _helpBox.text =
                        $"{methodRunner.MethodInfo.Name}: {error.InnerException?.Message ?? error.Message}";
                    _helpBox.style.display = DisplayStyle.Flex;
                }
            }

            if (!hasPendingRunner)
            {
                _methodRunners = null;
            }

            return result;
        }
    }
}

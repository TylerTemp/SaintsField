using System;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Editor.Utils;
using SaintsField.Editor.Utils.WaitableUtils;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Playa.Renderer.OnInspectorDisposeAttributeRenderer
{
    public partial class OnInspectorDisposeRenderer: AbsRenderer
    {
        private bool _disposeInvoked;

        public OnInspectorDisposeRenderer(OnInspectorDisposeAttribute onInspectorDisposeAttribute,
            SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo) : base(serializedObject, fieldWithInfo)
        {
        }

        public override void OnSearchField(string searchString)
        {
        }

        private void InvokeDispose()
        {
            if (_disposeInvoked)
            {
                return;
            }

            _disposeInvoked = true;

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
                    Debug.LogError($"Parameter {parameter.Name} on {methodInfo.Name} is not optional");
                    return;
                }

                parameterValues[index] = parameter.DefaultValue;
            }

            for (int index = 0; index < targetCount; index++)
            {
                object eachTarget = FieldWithInfo.Targets[index];
                if (RuntimeUtil.IsNull(eachTarget))
                {
                    continue;
                }

                (object rawMemberValue, object useTarget) = GetRefreshedTarget(FieldWithInfo, eachTarget);
                MethodRunnerUtil.Payload methodRunner =
                    MethodRunnerUtil.Invoke(methodInfo, useTarget, parameterValues);
                methodRunners[index] = methodRunner;
                if (methodRunner.Status == MethodRunnerUtil.RunStatus.Faulted)
                {
                    Debug.LogException(methodRunner.Exception);
                    return;
                }

                if (isStruct)
                {
                    BackWriteCallback(rawMemberValue, useTarget);
                }
            }

            MethodRunnerUtil.Payload[] needTickRunners = methodRunners
                .Where(each => each != null && each.Status == MethodRunnerUtil.RunStatus.Pending)
                .ToArray();

            if (needTickRunners.Length > 0)
            {
                EditorApplication.update += OnEditorUpdate;
            }

            return;

            void OnEditorUpdate()
            {
                bool hasPendingRunner = false;
                foreach (MethodRunnerUtil.Payload methodRunner in needTickRunners)
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
                    }
                }

                if (!hasPendingRunner)
                {
                    EditorApplication.update -= OnEditorUpdate;
                }
            }
        }
    }
}

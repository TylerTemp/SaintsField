using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using SaintsField.Playa;
using UnityEngine;
using UnityEngine.Events;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Testing.UnityEventCheck
{
    public class UnityEventCheck : SaintsMonoBehaviour
    {
        [ShowInInspector]
        [SerializeField] private UnityEvent _event = new UnityEvent();

        private UnityAction _instanceListener;
        private UnityAction _staticListener;
        private UnityAction _capturedLambdaListener;
        private int _lambdaCaptureValue;

        [Button]
        private void AddInstanceListener()
        {
            _instanceListener ??= InstanceCallback;
            _event.AddListener(_instanceListener);
            Debug.Log("Added instance listener.", this);
            LogRuntimeListeners();
        }

        [Button]
        private void AddStaticListener()
        {
            _staticListener ??= StaticCallback;
            _event.AddListener(_staticListener);
            Debug.Log("Added static listener.", this);
            LogRuntimeListeners();
        }

        [Button]
        private void AddCapturedLambdaListener()
        {
            int captureValue = ++_lambdaCaptureValue;
            _capturedLambdaListener = () => Debug.Log(
                $"Captured lambda invoked. Captured value={captureValue}, component={GetEntityId()}.", this);
            _event.AddListener(_capturedLambdaListener);
            Debug.Log($"Added captured lambda. Capture value={captureValue}.", this);
            LogRuntimeListeners();
        }

        [Button]
        private void RemoveInstanceListener()
        {
            if (_instanceListener == null)
            {
                Debug.LogWarning("No instance listener reference has been created yet.", this);
                return;
            }

            _event.RemoveListener(_instanceListener);
            Debug.Log("Removed instance listener by reusing its original UnityAction reference.", this);
            LogRuntimeListeners();
        }

        [Button]
        private void RemoveCapturedLambdaListener()
        {
            if (_capturedLambdaListener == null)
            {
                Debug.LogWarning("No captured lambda reference has been created yet.", this);
                return;
            }

            _event.RemoveListener(_capturedLambdaListener);
            Debug.Log("Removed captured lambda by reusing its original UnityAction reference.", this);
            LogRuntimeListeners();
        }

        [Button]
        private void InvokeEvent()
        {
            Debug.Log("Invoking UnityEvent.", this);
            _event.Invoke();
        }

        [Button]
        private void LogRuntimeListeners()
        {
            IList runtimeCalls = GetRuntimeCalls(_event);
            if (runtimeCalls == null)
            {
                Debug.LogWarning(
                    "Could not inspect UnityEvent runtime calls. Unity may have changed its private event fields.", this);
                return;
            }

            Debug.Log($"UnityEvent runtime listener count: {runtimeCalls.Count}.", this);
            for (int index = 0; index < runtimeCalls.Count; index++)
            {
                object call = runtimeCalls[index];
                Delegate listener = GetListenerDelegate(call);
                if (listener == null)
                {
                    Debug.Log($"Runtime listener [{index}]: invokable={DescribeObject(call)}; delegate unavailable.", this);
                    continue;
                }

                foreach (Delegate invocation in listener.GetInvocationList())
                {
                    MethodInfo method = invocation.Method;
                    object target = invocation.Target;
                    Debug.Log(
                        $"Runtime listener [{index}]: method={FormatMethod(method)}, " +
                        $"target={DescribeObject(target)}, targetKind={GetTargetKind(target)}, " +
                        $"delegateType={invocation.GetType().FullName}.", this);
                    LogCapturedFields(index, target);
                }
            }
        }

        private void InstanceCallback()
        {
            Debug.Log($"Instance callback invoked on {name} (instance ID {GetEntityId()}).", this);
        }

        private static void StaticCallback()
        {
            Debug.Log("Static callback invoked.");
        }

        private static IList GetRuntimeCalls(UnityEvent unityEvent)
        {
            object calls = GetFieldValue(unityEvent, "m_Calls");
            object runtimeCalls = calls == null ? null : GetFieldValue(calls, "m_RuntimeCalls");
            return runtimeCalls as IList;
        }

        private static Delegate GetListenerDelegate(object call)
        {
            for (Type type = call.GetType(); type != null; type = type.BaseType)
            {
                foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Public |
                                                            BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    if (typeof(Delegate).IsAssignableFrom(field.FieldType))
                    {
                        return field.GetValue(call) as Delegate;
                    }
                }
            }

            return null;
        }

        private static object GetFieldValue(object instance, string fieldName)
        {
            if (instance == null)
            {
                return null;
            }

            for (Type type = instance.GetType(); type != null; type = type.BaseType)
            {
                FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public |
                                                         BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                if (field != null)
                {
                    return field.GetValue(instance);
                }
            }

            return null;
        }

        private void LogCapturedFields(int listenerIndex, object target)
        {
            if (target == null || target is UnityEngine.Object)
            {
                return;
            }

            Type targetType = target.GetType();
            FieldInfo[] fields = targetType.GetFields(BindingFlags.Instance | BindingFlags.Public |
                                                       BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            if (fields.Length == 0)
            {
                return;
            }

            Debug.Log($"Runtime listener [{listenerIndex}] target fields ({targetType.FullName}):", this);
            foreach (FieldInfo field in fields)
            {
                object value;
                try
                {
                    value = field.GetValue(target);
                }
                catch (Exception exception)
                {
                    Debug.Log($"  {field.Name}: <read failed: {exception.GetType().Name}>", this);
                    continue;
                }

                Debug.Log($"  {field.Name} ({field.FieldType.FullName}) = {DescribeObject(value)}", this);
            }
        }

        private static string FormatMethod(MethodInfo method)
        {
            ParameterInfo[] parameters = method.GetParameters();
            string parameterText = string.Join(", ", Array.ConvertAll(parameters,
                parameter => $"{parameter.ParameterType.FullName} {parameter.Name}"));
            return $"{method.DeclaringType?.FullName}.{method.Name}({parameterText})";
        }

        private static string GetTargetKind(object target)
        {
            if (target == null)
            {
                return "static/no target";
            }

            if (target is UnityEngine.Object)
            {
                return "UnityEngine.Object instance";
            }

            Type type = target.GetType();
            return type.IsDefined(typeof(CompilerGeneratedAttribute), false) ||
                   type.Name.Contains("DisplayClass", StringComparison.Ordinal)
                ? "compiler-generated closure or helper"
                : "managed instance";
        }

        private static string DescribeObject(object value)
        {
            if (value == null)
            {
                return "null";
            }

            if (value is UnityEngine.Object unityObject)
            {
                return unityObject == null
                    ? "<destroyed UnityEngine.Object>"
                    : $"{unityObject.GetType().FullName} '{unityObject.name}' (instance ID {unityObject.GetEntityId()})";
            }

            return $"{value.GetType().FullName}: {value}";
        }
    }
}

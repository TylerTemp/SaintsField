using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

// ReSharper disable once CheckNamespace
namespace SaintsField.Events
{
    [Serializable]
    public class PersistentCall: ISerializationCallbackReceiver
    {
        [SerializeField] public UnityEventCallState callState = UnityEventCallState.RuntimeOnly;
        [SerializeField] private string _methodName;

        [SerializeField] private bool _isStatic;
        [SerializeField, DisableIf(nameof(_isStatic))] private Object _target;
        [SerializeField, EnableIf(nameof(_isStatic)), TypeReference(EType.AllAssembly | EType.AllowInternal)]
        private TypeReference _staticType;

        [SerializeField] private PersistentArgument[] _persistentArguments;

        [SerializeField] private TypeReference _returnType;

        private bool _methodCached;
        private MethodCache _methodCache;

        public void Invoke(object[] args)
        {
            if (callState == UnityEventCallState.Off)
            {
                return;
            }

            if (string.IsNullOrEmpty(_methodName))
            {
#if SAINTSFIELD_DEBUG
                Debug.LogWarning("PersistentCall: Method name is empty or null.");
#endif
                return;
            }

#if UNITY_EDITOR
            if (callState == UnityEventCallState.RuntimeOnly && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
#if SAINTSFIELD_DEBUG
                Debug.Log("PersistentCall: Call state is RuntimeOnly, but not in play mode.");
#endif
                return;
            }
#endif

            Type targetType = _isStatic ? _staticType.Type : _target?.GetType();
            if (targetType == null)
            {
#if SAINTSFIELD_DEBUG
                Debug.Log("PersistentCall: targetType is null.");
#endif
                return;
            }

            Type[] argumentTypes = new Type[_persistentArguments.Length];
            object[] argumentValues = new object[_persistentArguments.Length];
            List<int> defaultFillIndices = new List<int>(_persistentArguments.Length);
            for (int i = 0; i < _persistentArguments.Length; i++)
            {
                PersistentArgument persistentArgument = _persistentArguments[i];

                switch (persistentArgument.callType)
                {
                    case PersistentArgument.CallType.Dynamic:
                    {
                        if (persistentArgument.invokedParameterIndex < 0 ||
                            persistentArgument.invokedParameterIndex >= args.Length)
                        {
                            return;
                        }
                        argumentValues[i] = args[persistentArgument.invokedParameterIndex];
                    }
                        break;
                    case PersistentArgument.CallType.Serialized:
                    {
                        argumentValues[i] = persistentArgument.isUnityObject
                            ? persistentArgument.unityObject
                            : persistentArgument.SerializeObject;
                    }
                        break;
                    case PersistentArgument.CallType.OptionalDefault:
                    {
                        defaultFillIndices.Add(i);
                    }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(persistentArgument.callType), persistentArgument.callType, null);
                }

                argumentTypes[i] = persistentArgument.typeReference.Type;
            }

            Debug.Log($"argumentTypes={string.Join<Type>(", ", argumentTypes)}");

            MethodCache methodCache;
            if (_methodCached)
            {
                Debug.Log("use cached method");
                methodCache = _methodCache;
            }
            else
            {
                Debug.Log("fetch method");
                methodCache = _methodCache = GetMethod(_isStatic, _staticType.Type, _target, _methodName, argumentTypes);
                _methodCached = true;
            }
            // MethodInfo method = targetType.GetMethod(_methodName, flags, null, CallingConventions.Any, argumentTypes, null);
            MethodInfo methodInfo = methodCache.MethodInfo;
            if (methodInfo == null)
            {
#if SAINTSFIELD_DEBUG
                Debug.Log($"PersistentCall: method {_methodName} on {targetType} is null.");
#endif
                return;
            }

            ParameterInfo[] methodParams = methodInfo.GetParameters();
            foreach (int defaultFillIndex in defaultFillIndices)
            {
                if (defaultFillIndex >= methodParams.Length)
                {
                    return;
                }
                ParameterInfo param = methodParams[defaultFillIndex];
                if (!param.IsOptional)
                {
                    return;
                }

                argumentValues[defaultFillIndex] = param.DefaultValue;
            }

            object invokeTarget = methodCache.InvokeTarget;
            Debug.Log($"find method {methodInfo.Name} {string.Join(",", methodParams.Select(each => $"{each.Name}({each.ParameterType})"))} => {methodInfo.ReturnType}");
            methodInfo.Invoke(invokeTarget, argumentValues);
        }

        public readonly struct MethodCache
        {
            public readonly MethodInfo MethodInfo;
            public readonly object InvokeTarget;

            public MethodCache(MethodInfo methodInfo, object invokeTarget)
            {
                MethodInfo = methodInfo;
                InvokeTarget = invokeTarget;
            }
        }

        public static MethodCache GetMethod(bool isStatic, Type staticType, Object target, string methodName, Type[] argumentTypes)
        {
            Type targetType = isStatic ? staticType : target?.GetType();
            if (targetType == null)
            {
#if SAINTSFIELD_DEBUG
                Debug.Log("PersistentCall: targetType is null.");
#endif
                return new MethodCache(null, null);
            }


            const BindingFlags flagsStatic = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            const BindingFlags flagsInstance = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
            BindingFlags flags = isStatic ? flagsStatic : flagsInstance;

            MethodInfo method = targetType.GetMethod(methodName, flags, null, CallingConventions.Any, argumentTypes, null);
            if (method == null)
            {
#if SAINTSFIELD_DEBUG
                Debug.Log($"PersistentCall: method {methodName} on {targetType} is null.");
#endif
                return new MethodCache(null, null);
            }

            // bool methodReturnVoid = method.ReturnType == typeof(void);
            object methodTarget = isStatic ? null : target;
            return new MethodCache(method, methodTarget);
            // method.Invoke(methodTarget, argumentValues);
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            _methodCached = false;
        }
    }
}

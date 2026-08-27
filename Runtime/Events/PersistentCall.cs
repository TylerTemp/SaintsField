using System;
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
        [SerializeField] public string methodName;

        [SerializeField] public bool isStatic;
        [SerializeField, FieldDisableIf(nameof(isStatic))] public Object target;
        [SerializeField, FieldEnableIf(nameof(isStatic)), TypeReference(EType.AllAssembly | EType.AllowInternal)]
        public TypeReference staticType;

        [SerializeField] public PersistentArgument[] persistentArguments = Array.Empty<PersistentArgument>();
        [SerializeField] public TypeReference returnType;

        private bool _invokerCached;
        private CachedInvoker _invokerCache;

        public void Invoke(object[] args)
        {
            if (callState == UnityEventCallState.Off || string.IsNullOrEmpty(methodName))
            {
                return;
            }

#if UNITY_EDITOR
            if (callState == UnityEventCallState.RuntimeOnly && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }
#endif

            Type targetType = isStatic ? staticType?.Type : target?.GetType();
            if (targetType == null)
            {
                return;
            }

            if (!_invokerCached)
            {
                _invokerCache = CreateInvoker(targetType);
                _invokerCached = true;
            }

            _invokerCache?.Invoke(args);
        }

        private CachedInvoker CreateInvoker(Type targetType)
        {
            PersistentArgument[] arguments = persistentArguments;
            Type[] argumentTypes = new Type[arguments.Length];
            for (int index = 0; index < arguments.Length; index++)
            {
                argumentTypes[index] = arguments[index].typeReference.Type;
            }

            MethodCache methodCache = GetMethod(isStatic, staticType?.Type, target, methodName, argumentTypes);
            MethodInfo methodInfo = methodCache.MethodInfo;
            if (methodInfo == null)
            {
#if SAINTSFIELD_DEBUG
                Debug.Log($"PersistentCall: method {methodName} on {targetType} is null.");
#endif
                return null;
            }

            ParameterInfo[] methodParameters = methodInfo.GetParameters();
            object[] optionalDefaults = new object[arguments.Length];
            for (int index = 0; index < arguments.Length; index++)
            {
                if (arguments[index].callType != PersistentArgument.CallType.OptionalDefault)
                {
                    continue;
                }

                if (index >= methodParameters.Length || !methodParameters[index].IsOptional)
                {
                    return null;
                }

                optionalDefaults[index] = methodParameters[index].DefaultValue;
            }

            try
            {
                Type invokerType = GetTypedInvokerType(argumentTypes, methodInfo.ReturnType);
                if (invokerType != null)
                {
                    return (CachedInvoker)Activator.CreateInstance(invokerType, methodInfo, methodCache.InvokeTarget,
                        arguments, optionalDefaults);
                }
            }
            catch (Exception exception)
            {
#if SAINTSFIELD_DEBUG
                Debug.LogWarning($"PersistentCall: could not create a typed delegate for {methodName}; using reflection. {exception}");
#endif
            }

            // Preserve unusual or >4-parameter signatures. Normal SaintsEvent calls never enter this hot path.
            return new ReflectionInvoker(methodInfo, methodCache.InvokeTarget, arguments, optionalDefaults);
        }

        private static Type GetTypedInvokerType(Type[] argumentTypes, Type resultType)
        {
            bool returnsVoid = resultType == typeof(void);
            Type openType;
            switch (argumentTypes.Length)
            {
                case 0:
                    return returnsVoid ? typeof(VoidInvoker) : typeof(ResultInvoker<>).MakeGenericType(resultType);
                case 1:
                    openType = returnsVoid ? typeof(VoidInvoker<>) : typeof(ResultInvoker<,>);
                    break;
                case 2:
                    openType = returnsVoid ? typeof(VoidInvoker<,>) : typeof(ResultInvoker<,,>);
                    break;
                case 3:
                    openType = returnsVoid ? typeof(VoidInvoker<,,>) : typeof(ResultInvoker<,,,>);
                    break;
                case 4:
                    openType = returnsVoid ? typeof(VoidInvoker<,,,>) : typeof(ResultInvoker<,,,,>);
                    break;
                default:
                    return null;
            }

            if (returnsVoid)
            {
                return openType.MakeGenericType(argumentTypes);
            }

            Type[] genericTypes = new Type[argumentTypes.Length + 1];
            Array.Copy(argumentTypes, genericTypes, argumentTypes.Length);
            genericTypes[genericTypes.Length - 1] = resultType;
            return openType.MakeGenericType(genericTypes);
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

        public static MethodCache GetMethod(bool isStatic, Type staticType, Object target, string methodName,
            Type[] argumentTypes)
        {
            Type targetType = isStatic ? staticType : target?.GetType();
            if (targetType == null)
            {
                return new MethodCache(null, null);
            }

            const BindingFlags flagsStatic = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            const BindingFlags flagsInstance = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                                               BindingFlags.Static;
            BindingFlags flags = isStatic ? flagsStatic : flagsInstance;
            MethodInfo method = targetType.GetMethod(methodName, flags, null, CallingConventions.Any, argumentTypes,
                null);
            return method == null
                ? new MethodCache(null, null)
                : new MethodCache(method, isStatic ? null : target);
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            _invokerCached = false;
            _invokerCache = null;
        }
    }
}

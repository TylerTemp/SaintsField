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
    public class PersistentCall
    {
        [SerializeField] public UnityEventCallState callState = UnityEventCallState.RuntimeOnly;
        [SerializeField] private string _methodName;

        [SerializeField] private bool _isStatic;
        [SerializeField, DisableIf(nameof(_isStatic))] private Object _target;
        [SerializeField, EnableIf(nameof(_isStatic)), TypeReference(EType.AllAssembly | EType.AllowInternal)]
        private TypeReference _staticType;

        [SerializeField] private PersistentArgument[] _persistentArguments;

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
            for (int i = 0; i < _persistentArguments.Length; i++)
            {
                PersistentArgument persistentArgument = _persistentArguments[i];
                if (persistentArgument.invokedParameterIndex == -1)  // use serialized value
                {
                    Type argumentType = argumentTypes[i] = persistentArgument.typeReference.Type;
                    if (argumentType == null)
                    {
#if SAINTSFIELD_DEBUG
                        Debug.Log($"PersistentCall: persistentArguments[{i}] type is null.");
#endif
                        return;
                    }
                    argumentValues[i] = persistentArgument.GetArgumentValue();
                }
                else
                {
                    if (persistentArgument.invokedParameterIndex < 0 ||
                        persistentArgument.invokedParameterIndex >= args.Length)
                    {
                        return;
                    }

                    object dynamicInvoked = args[persistentArgument.invokedParameterIndex];
                    argumentTypes[i] = dynamicInvoked.GetType();
                    argumentValues[i] = dynamicInvoked;
                }
            }

            const BindingFlags flagsStatic = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            const BindingFlags flagsInstance = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
            BindingFlags flags = _isStatic ? flagsStatic : flagsInstance;

            MethodInfo method = targetType.GetMethod(_methodName, flags, null, CallingConventions.Any, argumentTypes, null);
            if (method == null)
            {
#if SAINTSFIELD_DEBUG
                Debug.Log($"PersistentCall: method {_methodName} on {targetType} is null.");
#endif
                return;
            }

            // bool methodReturnVoid = method.ReturnType == typeof(void);
            object methodTarget = _isStatic ? null : _target;
            method.Invoke(methodTarget, argumentValues);
        }
    }
}

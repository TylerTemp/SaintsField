using System;
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
        [SerializeField] private Object _target;
        [SerializeField, TypeReference(EType.AllAssembly | EType.AllowInternal)]
        private TypeReference _staticType;

        [SerializeField] private PersistentArgument[] _persistentArguments;

        public void Invoke(object[] args)
        {
            if (callState == UnityEventCallState.Off || string.IsNullOrEmpty(_methodName))
            {
                return;
            }

#if UNITY_EDITOR
            if (callState == UnityEventCallState.RuntimeOnly && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }
#endif

            Type targetType = _isStatic ? _staticType.Type : _target?.GetType();
            if (targetType == null)
            {
                return;
            }

            Type[] argumentTypes = new Type[_persistentArguments.Length];
            for (int i = 0; i < _persistentArguments.Length; i++)
            {
                Type argumentType = argumentTypes[i] = _persistentArguments[i].typeReference.Type;
                if (argumentType == null)
                {
                    return;
                }
            }
        }
    }
}

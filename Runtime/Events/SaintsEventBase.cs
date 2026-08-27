using System;
using UnityEngine;
using UnityEngine.Events;

// ReSharper disable once CheckNamespace
namespace SaintsField.Events
{
    [Serializable]
    public abstract class SaintsEventBase
    {
        [SerializeField] protected PersistentCall[] _persistentCalls = Array.Empty<PersistentCall>();

        public void RemoveAllListeners()
        {
            RemoveAllRuntimeListeners();
        }

        protected virtual void RemoveAllRuntimeListeners()
        {
        }

        public int GetPersistentEventCount() => _persistentCalls.Length;

        public UnityEventCallState GetPersistentListenerState(int index) => _persistentCalls[index].callState;

        public string GetPersistentMethodName(int index) => _persistentCalls[index].methodName;

        public UnityEngine.Object GetPersistentTarget(int index)
        {
            PersistentCall call = _persistentCalls[index];
            return call.isStatic ? null : call.target;
        }

        public void SetPersistentListenerState(int index, UnityEventCallState state)
        {
            _persistentCalls[index].callState = state;
        }
    }
}

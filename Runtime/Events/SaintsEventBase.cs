using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

// ReSharper disable once CheckNamespace
namespace SaintsField.Events
{
    [Serializable]
    public abstract class SaintsEventBase
    {
        internal readonly List<BaseInvokableCall> RuntimeCalls = new List<BaseInvokableCall>();

        [SerializeField] protected PersistentCall[] _persistentCalls;

        internal void AddCall(BaseInvokableCall call)
        {
            if(!RuntimeCalls.Contains(call))
            {
                RuntimeCalls.Add(call);
            }
        }

        protected void RemoveListener(object targetObj, MethodInfo method)
        {
            RuntimeCalls.RemoveAll(each => each.Find(targetObj, method));
        }

        public void RemoveAllListeners()
        {
            RuntimeCalls.Clear();
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

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

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
    }
}

using System;
using System.Reflection;
using UnityEngine.Events;

namespace SaintsField.Events
{
    [Serializable]
    public class SaintsEvent: SaintsEventBase
    {
        public void Invoke()
        {
            foreach (PersistentCall persistentCall in _persistentCalls)
            {
                persistentCall.Invoke(Array.Empty<object>());
            }

            foreach (BaseInvokableCall invokableCall in RuntimeCalls)
            {
                invokableCall.Invoke(Array.Empty<object>());
            }
        }

        public void AddListener(UnityAction call) => AddCall(new InvokableCall(call));
        public void RemoveListener(UnityAction call) => RemoveListener(call.Target, call.Method);

        private sealed class InvokableCall : BaseInvokableCall
        {
            private event UnityAction Delegate;

            public InvokableCall(UnityAction action) => Delegate += action;

            public override void Invoke(object[] args)
            {
                if (args.Length != 0)
                    throw new ArgumentException("Passed argument 'args' is invalid size. Expected size is 0");
                if (!AllowInvoke(Delegate))
                    return;
                Delegate();
            }

            public override bool Find(object targetObj, MethodInfo method)
            {
                return Delegate.Target == targetObj && Delegate.Method.Equals(method);
            }
        }
    }


}

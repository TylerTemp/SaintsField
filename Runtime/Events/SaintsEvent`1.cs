using System;
using UnityEngine.Events;

namespace SaintsField.Events
{
    [Serializable]
    public class SaintsEvent<T0>: SaintsEventBase
    {
        public void Invoke(T0 arg0)
        {
            object[] args = { arg0 };
            foreach (PersistentCall presistentCall in _persistentCalls)
            {
                presistentCall.Invoke(args);
            }

            foreach (BaseInvokableCall invokableCall in RuntimeCalls)
            {
                invokableCall.Invoke(args);
            }
        }

        public void AddListener(UnityAction<T0> call) => AddCall(GetDelegate(call));
        public void RemoveListener(UnityAction<T0> call) => RemoveListener(call.Target, call.Method);

        private static BaseInvokableCall GetDelegate(UnityAction<T0> action)
        {
            return new InvokableCall<T0>(action);
        }
    }
}

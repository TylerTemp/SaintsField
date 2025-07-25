using System;
using UnityEngine.Events;

namespace SaintsField.Events
{
    [Serializable]
    public class SaintsEvent<T0, T1, T2, T3>: SaintsEventBase
    {
        public void Invoke(T0 arg0, T1 arg1, T3 arg2, T3 arg3)
        {
            object[] args = { arg0, arg1, arg2, arg3 };
            foreach (PersistentCall presistentCall in _persistentCalls)
            {
                presistentCall.Invoke(args);
            }

            foreach (BaseInvokableCall invokableCall in RuntimeCalls)
            {
                invokableCall.Invoke(args);
            }
        }

        public void AddListener(UnityAction<T0, T1, T2, T3> call) => AddCall(GetDelegate(call));
        public void RemoveListener(UnityAction<T0, T1, T2, T3> call) => RemoveListener(call.Target, call.Method);

        private static BaseInvokableCall GetDelegate(UnityAction<T0, T1, T2, T3> action)
        {
            return new InvokableCall<T0, T1, T2, T3>(action);
        }
    }
}

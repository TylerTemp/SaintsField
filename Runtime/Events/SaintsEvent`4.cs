using System;
using UnityEngine.Events;

namespace SaintsField.Events
{
    [Serializable]
    public class SaintsEvent<T0, T1, T2, T3>: SaintsEventBase
    {
        private event UnityAction<T0, T1, T2, T3> _runtimeCalls;

        public void Invoke(T0 arg0, T1 arg1, T2 arg2, T3 arg3)
        {
            if (_persistentCalls.Length > 0)
            {
                object[] args = { arg0, arg1, arg2, arg3 };
                foreach (PersistentCall persistentCall in _persistentCalls)
                {
                    persistentCall.Invoke(args);
                }
            }

            _runtimeCalls?.Invoke(arg0, arg1, arg2, arg3);
        }

        public void AddListener(UnityAction<T0, T1, T2, T3> call) => _runtimeCalls += call;
        public void RemoveListener(UnityAction<T0, T1, T2, T3> call) => _runtimeCalls -= call;

        protected override void RemoveAllRuntimeListeners()
        {
            _runtimeCalls = null;
        }
    }
}

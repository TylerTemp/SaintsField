using System;
using UnityEngine.Events;

namespace SaintsField.Events
{
    [Serializable]
    public class SaintsEvent<T0>: SaintsEventBase
    {
        private event UnityAction<T0> _runtimeCalls;

        public void Invoke(T0 arg0)
        {
            if (_persistentCalls.Length > 0)
            {
                object[] args = { arg0 };
                foreach (PersistentCall persistentCall in _persistentCalls)
                {
                    persistentCall.Invoke(args);
                }
            }

            _runtimeCalls?.Invoke(arg0);
        }

        public void AddListener(UnityAction<T0> call) => _runtimeCalls += call;
        public void RemoveListener(UnityAction<T0> call) => _runtimeCalls -= call;

        protected override void RemoveAllRuntimeListeners()
        {
            _runtimeCalls = null;
        }
    }
}

using System;
using UnityEngine.Events;

namespace SaintsField.Events
{
    [Serializable]
    public class SaintsEvent: SaintsEventBase
    {
        private event UnityAction _runtimeCalls;

        public void Invoke()
        {
            foreach (PersistentCall persistentCall in _persistentCalls)
            {
                persistentCall.Invoke(Array.Empty<object>());
            }

            _runtimeCalls?.Invoke();
        }

        public void AddListener(UnityAction call) => _runtimeCalls += call;
        public void RemoveListener(UnityAction call) => _runtimeCalls -= call;

        protected override void RemoveAllRuntimeListeners()
        {
            _runtimeCalls = null;
        }
    }
}

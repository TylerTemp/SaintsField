using System;
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
        }
    }


}

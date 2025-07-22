using System;
using UnityEngine.Events;

namespace SaintsField.Events
{
    [Serializable]
    public class SaintsEvent: SaintsEventBase
    {
        public void Invoke()
        {
            foreach (PersistentCall presistentCall in _persistentCalls)
            {
                presistentCall.Invoke(Array.Empty<object>());
            }
        }
    }


}

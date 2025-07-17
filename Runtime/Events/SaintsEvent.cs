using System;

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

    [Serializable]
    public class SaintsEvent<T1>: SaintsEventBase
    {
        public void Invoke(T1 arg1)
        {
            foreach (PersistentCall presistentCall in _persistentCalls)
            {
                presistentCall.Invoke(new object[]{arg1});
            }
        }
    }

    [Serializable]
    public class SaintsEvent<T1, T2>: SaintsEventBase
    {
        public void Invoke(T1 arg1, T2 arg2)
        {
            foreach (PersistentCall presistentCall in _persistentCalls)
            {
                presistentCall.Invoke(new object[]{arg1, arg2});
            }
        }
    }
}

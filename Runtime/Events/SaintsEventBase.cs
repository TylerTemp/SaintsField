using System;
using SaintsField.Runtime.Events;
using UnityEngine;

namespace SaintsField.Events
{
    [Serializable]
    public class SaintsEventBase
    {
        [SerializeField] private PersistentCall[] _persistentCalls;
    }
}

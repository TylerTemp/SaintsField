using System;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace SaintsField.Events
{
    [Serializable]
    public class SaintsEventBase
    {
        [SerializeField] protected PersistentCall[] _persistentCalls;
    }
}

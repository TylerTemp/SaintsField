using System;
using UnityEngine;

namespace SaintsField.Runtime.Events
{
    [Serializable]
    public class PersistentCall
    {
        [SerializeField] internal PersistentArgument[] _persistentArguments;
    }
}

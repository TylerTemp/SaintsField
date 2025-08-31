using System;
using UnityEngine;
using Event = AK.Wwise.Event;

namespace SaintsField.Samples.Scripts.Wwise
{
    public class SaintsDictionaryWwise : MonoBehaviour
    {
        [Serializable]
        public enum WwiseId
        {
            Id1,
            Id2,
        }

        public SaintsDictionary<WwiseId, Event> idToEvent;
    }
}

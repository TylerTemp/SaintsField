using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SaintsField.Samples.Scripts.SaintsEditor.Issues
{
    public class Issue323 : SaintsMonoBehaviour
    {
        [Serializable]
        public class NamedEvent
        {
            public string name = "Default Event";
            public UnityEvent eventToTrigger = new UnityEvent();
        }

        [Tooltip("Set the events appointed here. Timeline markers will shoot events based on the corresponding 'event identifier'.")]
        public List<NamedEvent> namedEvents = new List<NamedEvent>();
        public List<UnityEvent> eventsTest;
        public UnityEvent eventTest;

        public NamedEvent nameEvent;
    }
}

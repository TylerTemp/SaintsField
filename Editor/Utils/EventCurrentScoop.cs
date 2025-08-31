using System;
using UnityEngine;

namespace SaintsField.Editor.Utils
{
    public class EventCurrentScoop: IDisposable
    {
        public readonly Event CurrentEvent;

        public EventCurrentScoop(Event newEvent)
        {
            CurrentEvent = Event.current;
            Event.current = newEvent;
        }

        public void Dispose()
        {
            Event.current = CurrentEvent;
        }
    }
}

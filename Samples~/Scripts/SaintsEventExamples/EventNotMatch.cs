using SaintsField.Events;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEventExamples
{
    public class EventNotMatch : MonoBehaviour
    {
        public SaintsEvent<string> sEvent;

        private void Callback(int i, string s)
        {
        }
    }
}

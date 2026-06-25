using SaintsField.Playa;
using UnityEngine;
using UnityEngine.Events;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    [InfoBox("Finish this one day?")]
    public class ShowInInspectorEvent : SaintsMonoBehaviour
    {
        public UnityEvent ue0;

        public UnityEvent<int, string> ue2;

        public UnityEvent<Object> uObj;

        [ShowInInspector] private UnityEvent Ue0
        {
            get => ue0;
            set => ue0 = value;
        }

        [ShowInInspector] private UnityEvent<int, string> Ue2
        {
            get => ue2;
            set => ue2 = value;
        }

        [ShowInInspector] private UnityEvent<Object> UObj
        {
            get => uObj;
            set => uObj = value;
        }
    }
}

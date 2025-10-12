#if SAINTSFIELD_SAMPLE_ULTEVENT
using UltEvents;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Issues
{
    public class Issue327UltEvent : MonoBehaviour
    {
        [Range(0,50)]
        public int max;
        // [ArraySize(nameof(max))]
        public UltEvent[] onLevel;

        public void SetActive(bool v) {}
    }
}
#endif

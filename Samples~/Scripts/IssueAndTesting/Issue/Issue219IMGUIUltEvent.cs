using SaintsField.Samples.Scripts.SaintsEditor;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue219IMGUIUltEvent : SaintsMonoBehaviour
    {
#if SAINTSFIELD_ULT_EVENT
        [SerializeField]
        private UltEvents.UltEvent _MyEvent;
#endif
    }
}

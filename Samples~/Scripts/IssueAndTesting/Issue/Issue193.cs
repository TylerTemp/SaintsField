#if SAINTSFIELD_SAMPLE_OBVIOUS_SOAP
using Obvious.Soap;
#endif
using SaintsField.Samples.Scripts.SaintsEditor;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue193: SaintsMonoBehaviour
    {
#if SAINTSFIELD_SAMPLE_OBVIOUS_SOAP
        [SerializeField, GetScriptableObject, InfoBox("Saints Fallback")] private BoolVariable _inputsEnabled = null;
#endif
    }
}

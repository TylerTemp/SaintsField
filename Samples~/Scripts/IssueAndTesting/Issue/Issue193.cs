#if SAINTSFIELD_SAMPLE_OBVIOUS_SOAP
using Obvious.Soap;
#endif
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue193: MonoBehaviour
    {
#if SAINTSFIELD_SAMPLE_OBVIOUS_SOAP
        [SerializeField, GetScriptableObject] private BoolVariable _inputsEnabled = null;
#endif
    }
}

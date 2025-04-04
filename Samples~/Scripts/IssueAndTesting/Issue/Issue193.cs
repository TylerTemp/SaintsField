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
        [SerializeField, GetScriptableObject, InfoBox("Saints Fallback"), SepTitle("Dec"), BelowRichLabel("More!")] private BoolVariable inputsEnabledMoreMore;
        [SerializeField, GetScriptableObject, InfoBox("Saints Fallback"), SepTitle("Dec")] private BoolVariable inputsEnabledMore;
        [SerializeField, GetScriptableObject, InfoBox("Saints Fallback")] private BoolVariable inputsEnabled;
        [SerializeField, GetScriptableObject] private BoolVariable inputsEnabledG;
        [SerializeField, InfoBox("Saints Fallback")] private BoolVariable inputsEnabledI;
        [SerializeField] private BoolVariable inputsEnabledBare;
#endif
    }
}

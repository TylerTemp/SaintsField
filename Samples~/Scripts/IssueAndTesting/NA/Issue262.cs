using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.NA
{
    public class Issue262 : MonoBehaviour
    {
        [
            SerializeField,
            Header("Center message"),
#if SAINTSFIELD_SAMPLE_NAUGHYTATTRIBUTES
            NaughtyAttributes.Required,
#else
            InfoBox("NaughtyAttributes not installed", above: true),
#endif
        ] Canvas naCenterMessageCanvas = default;

        // nah, SaintsField always render the error message below the field so...
        [
            SerializeField,
            Header("Center message"),
            Required,
        ] Canvas centerMessageCanvas = default;
    }
}

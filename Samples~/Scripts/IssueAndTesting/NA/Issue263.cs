using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.NA
{
    public class Issue263 : MonoBehaviour
    {
        [
            SerializeField,
#if SAINTSFIELD_SAMPLE_NAUGHYTATTRIBUTES
            NaughtyAttributes.Required,
            NaughtyAttributes.Expandable,
#else
            FieldInfoBox("NaughtyAttributes not installed"),
#endif
        ]
        private Scriptable naCarConfig;

        [Space]

        [SerializeField, Required, Expandable] private Scriptable carConfig;
    }
}

using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.NA.Issue383
{
    public class BaseM : MonoBehaviour
    {
#if SAINTSFIELD_SAMPLE_NAUGHYTATTRIBUTES
        [NaughtyAttributes.Button("Do Button Clicked")]
#else
        [Button("Do Button Clicked")]
#endif
        protected virtual void Editor_ButtonClicked() => Debug.Log("Clicked Base Class!");
    }
}

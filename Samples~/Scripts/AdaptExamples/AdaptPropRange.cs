using UnityEngine;

namespace SaintsField.Samples.Scripts.AdaptExamples
{
    public class AdaptPropRange : MonoBehaviour
    {
        [Adapt(EUnit.Percent)] public float percentF;
        [Adapt(EUnit.Percent)] public int percentI;
        [PropRange(0f, 100f), Adapt(EUnit.Percent)] public float percentRange;
    }
}

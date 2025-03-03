using UnityEngine;

namespace SaintsField.Samples.Scripts.AdaptExamples
{
    public class AdaptPropRange : MonoBehaviour
    {
        [Adapt(EUnit.Percent)] public float percentF;
        [Adapt(EUnit.Percent)] public int percentI;
        [PropRange(0f, 1f), Adapt(EUnit.Percent), PostFieldRichLabel("%"), BelowRichLabel("$" + nameof(percentRange)), BelowButton("$" + nameof(ExternalPumpValue), "Pump")] public float percentRange;
        [PropRange(0f, 1f, step: 0.05f), Adapt(EUnit.Percent), PostFieldRichLabel("%"), BelowRichLabel("$" + nameof(stepRange)), BelowButton("$" + nameof(ExternalPumpValueStep), "Pump")] public float stepRange;

        private void ExternalPumpValue(float curValue)
        {
            // Debug.Log(curValue);
            percentRange = Mathf.FloorToInt((curValue + 0.1f) / 0.1f) % 10 * 0.1f;
        }

        private void ExternalPumpValueStep(float curValue)
        {
            // Debug.Log(curValue);
            stepRange = Mathf.FloorToInt((curValue + 0.1f) / 0.1f) % 10 * 0.1f + 0.001f;
        }
    }
}

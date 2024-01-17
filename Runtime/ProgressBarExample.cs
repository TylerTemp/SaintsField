using UnityEngine;

namespace SaintsField
{
    public class ProgressBarExample: MonoBehaviour
    {
        [ProgressBar] public float fValue = -25f;
        // [ProgressBar(100)] public float v2 = -25f;
        // [ProgressBar(100, 0.5f)] public float v3 = -25f;
    }
}

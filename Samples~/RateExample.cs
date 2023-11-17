using UnityEngine;

namespace SaintsField.Samples
{
    public class RateExample: MonoBehaviour
    {
        [Rate(0, 5)] public int rate05;
        [Rate(1, 5)] public int rate15Value;
        [Rate(3, 5)] public int rate35;
    }
}

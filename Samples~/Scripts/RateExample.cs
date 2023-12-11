using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class RateExample: MonoBehaviour
    {
        [Rate(0, 5)] public int rate0To5;
        [Rate(1, 5)] public int rate1To5;
        [Rate(3, 5)] public int rate3To5;
    }
}

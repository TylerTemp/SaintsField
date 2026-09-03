using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    // ReSharper disable once InconsistentNaming
    public class SaintsArray2DRExample : MonoBehaviour
    {
        public SaintsArray2DR<bool> array2Dr;
        [SaintsArray2DR(transpose: true)]
        public SaintsArray2DR<bool> array2DrRev;
    }
}

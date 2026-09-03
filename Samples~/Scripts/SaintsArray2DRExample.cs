using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    // ReSharper disable once InconsistentNaming
    public class SaintsArray2DRExample : MonoBehaviour
    {
        public SaintsArray2DR<bool> array2Dr;

        // reverse x, y direction
        [SaintsArray2DR(transpose: true)]
        public SaintsArray2DR<bool> array2DrRev;

        private void Awake()
        {
            bool[,] to2D = array2Dr;  // convert to
            SaintsArray2DR<bool> from2D = array2DrRev;  // convert from
        }
    }
}

using System.Linq;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    // ReSharper disable once InconsistentNaming
    public class SaintsArray2DRExample : SaintsMonoBehaviour
    {
        public SaintsArray2DR<bool> array2Dr;

        [ShowInInspector]
        private bool[][] Show()
        {
            return Enumerable.Range(0, array2Dr.GetLength(0))
                .Select(row => Enumerable.Range(0, array2Dr.GetLength(1))
                    .Select(column => array2Dr[row, column])
                    .ToArray())
                .ToArray();
        }

        // reverse x, y direction
        // [SaintsArray2DR(transpose: true)]
        // public SaintsArray2DR<bool> array2DrRev;
        //
        // private void Awake()
        // {
        //     bool[,] to2D = array2Dr;  // convert to
        //     SaintsArray2DR<bool> from2D = array2DrRev;  // convert from
        // }
    }
}

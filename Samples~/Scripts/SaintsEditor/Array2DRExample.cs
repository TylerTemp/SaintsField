using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public partial class Array2DRExample : SaintsMonoBehaviour
    {
        [SaintsSerialized]
        private bool[,] array2DSerialized = new bool[2, 3];

        [SaintsSerialized, SaintsArray2DR(transpose: true)]
        private bool[,] array2DSerializedR = new bool[2, 3];

        [ShowInInspector]
        private bool[,] ShowArray2DSerialized
        {
            get => array2DSerialized;
            set => array2DSerialized = value;
        }

        [ShowInInspector, SaintsArray2DR(transpose: true)]
        private bool[,] ShowArray2DSerializedR
        {
            get => array2DSerializedR;
            set => array2DSerializedR = value;
        }
    }
}

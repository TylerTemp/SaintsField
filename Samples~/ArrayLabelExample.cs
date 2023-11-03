using System;
using UnityEngine;

namespace SaintsField.Samples
{
    public class ArrayLabelExample : MonoBehaviour
    {
        // wont work
        [SerializeField, RichLabel("HI")] private int[] _ints;
    }
}

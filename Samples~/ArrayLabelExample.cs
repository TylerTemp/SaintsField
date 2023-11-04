using System;
using UnityEngine;

namespace SaintsField.Samples
{
    public class ArrayLabelExample : MonoBehaviour
    {
        // wont work
        [SerializeField, RichLabel("HI"), InfoBox("this actually wont work", EMessageType.Warning)] private int[] _ints;
    }
}

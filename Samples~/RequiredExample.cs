using System;
using UnityEngine;

namespace SaintsField.Samples
{
    public class RequiredExample: MonoBehaviour
    {
        [SerializeField, Required] private Sprite _spriteImage;
        [field: SerializeField, Required("Add this please!")] public GameObject Go { get; private set; }
        [SerializeField, Required] private UnityEngine.Object _object;
        [SerializeField, Required] private float _wontWork;

        // [Serializable]
        // private struct MyStruct
        // {
        //     public int theInt;
        // }

        // [SerializeField, RichLabel("HI"), AboveButton(nameof(TestButton), "CLICK")] private MyStruct _myStruct;
        // [SerializeField, RichLabel("HI")] private int[] _ints;

        // private void TestButton() {}
    }
}

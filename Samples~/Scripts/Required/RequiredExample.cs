using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.Required
{
    public class RequiredExample: MonoBehaviour
    {
        [Required("Add this please!")] public Sprite _spriteImage;
        // works for property field
        [field: SerializeField, Required] public GameObject Go { get; private set; }
        [Required] public UnityEngine.Object _object;
        [SerializeField, Required] private float _floatIsVauleType;

        [SerializeField, Required, Range(-1, 1)] private int _intIsVauleType;

        [Serializable]
        public struct MyStruct
        {
            public int theInt;
        }

        [Required]
        public MyStruct myStruct;

        [Serializable]
        public struct Nest2
        {
            [Required] public GameObject n2;
        }

        [Serializable]
        public struct Nest1
        {
            [ReadOnly]
            [Required] public GameObject n1;
            public Nest2 nest2;
        }

        public Nest1 nest;
    }
}

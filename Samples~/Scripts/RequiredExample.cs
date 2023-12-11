using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class RequiredExample: MonoBehaviour
    {
        [Required("Add this please!")] public Sprite _spriteImage;
        // works for property field
        [field: SerializeField, Required] public GameObject Go { get; private set; }
        [Required] public UnityEngine.Object _object;
        [SerializeField, Required] private float _wontWork;

        [Serializable]
        public struct MyStruct
        {
            public int theInt;
        }

        [Required]
        public MyStruct wontWorkWontNoticeYou;
    }
}

using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.ReadOnlyExamples
{
    public class EnableIfEnumExample : MonoBehaviour
    {
        [Serializable]
        public enum EnumToggle
        {
            Off,
            On,
        }

        public EnumToggle enum1;
        public EnumToggle enum2;
        public EnumToggle enum3;
        public EnumToggle enum4;

        public bool bool1;
        public bool bool2;
        public bool bool3;

        [EnableIf(nameof(enum1), EnumToggle.On)] public string enum1Show;
        [EnableIf(nameof(enum1), EnumToggle.On, nameof(enum2), EnumToggle.On)] public string enum1Or2Show;
        [EnableIf(nameof(enum1), EnumToggle.On, nameof(enum2), EnumToggle.On, nameof(enum3), EnumToggle.On)] public string enum1Or2Or3Show;
        [EnableIf(nameof(enum1), EnumToggle.On, nameof(enum2), EnumToggle.On, nameof(enum3), EnumToggle.On, nameof(enum4), EnumToggle.On)] public string enum1Or2Or3Or4Show;

        [SepTitle("Bool1+Enum", EColor.Gray)]

        // 1+1
        [EnableIf(nameof(bool1), nameof(enum1), EnumToggle.On)] public string bool1OrEnum1Show;
        // 1+2
        [EnableIf(nameof(bool1), nameof(enum1), EnumToggle.On, nameof(enum2), EnumToggle.On)] public string bool1OrEnum12Show;
        // 1+3
        [EnableIf(nameof(bool1), nameof(enum1), EnumToggle.On, nameof(enum2), EnumToggle.On, nameof(enum3), EnumToggle.On)] public string bool1OrEnum123;

        [SepTitle("Bool2+Enum", EColor.Gray)]
        // 2+1
        [EnableIf(nameof(bool1), nameof(bool2), nameof(enum1), EnumToggle.On)] public string bool12OrEnum1Show;
        // 2+2
        [EnableIf(nameof(bool1), nameof(bool2), nameof(enum1), EnumToggle.On, nameof(enum2), EnumToggle.On)] public string bool12OrEnum12Show;

        [SepTitle("Bool3+Enum", EColor.Gray)]
        // 3+1
        [EnableIf(nameof(bool1), nameof(bool2), nameof(bool3), nameof(enum1), EnumToggle.On)] public string bool123OrEnum1Show;
    }
}

using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.ShowHideExamples
{
    public class ShowIfEnumExample : MonoBehaviour
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

        [ShowIf(nameof(enum1), EnumToggle.On)] public string enum1Show;
        [ShowIf(nameof(enum1), EnumToggle.On, nameof(enum2), EnumToggle.On)] public string enum1And2Show;
        [ShowIf(nameof(enum1), EnumToggle.On, nameof(enum2), EnumToggle.On, nameof(enum3), EnumToggle.On)] public string enum1And2And3Show;
        [ShowIf(nameof(enum1), EnumToggle.On, nameof(enum2), EnumToggle.On, nameof(enum3), EnumToggle.On, nameof(enum4), EnumToggle.On)] public string enum1And2And3And4Show;

        [SepTitle("Bool1+Enum", EColor.Gray)]

        // 1+1
        [ShowIf(nameof(bool1), nameof(enum1), EnumToggle.On)] public string bool1AndEnum1Show;
        // 1+2
        [ShowIf(nameof(bool1), nameof(enum1), EnumToggle.On, nameof(enum2), EnumToggle.On)] public string bool1AndEnum12Show;
        // 1+3
        [ShowIf(nameof(bool1), nameof(enum1), EnumToggle.On, nameof(enum2), EnumToggle.On, nameof(enum3), EnumToggle.On)] public string bool1AndEnum123;

        [SepTitle("Bool2+Enum", EColor.Gray)]
        // 2+1
        [ShowIf(nameof(bool1), nameof(bool2), nameof(enum1), EnumToggle.On)] public string bool12AndEnum1Show;
        // 2+2
        [ShowIf(nameof(bool1), nameof(bool2), nameof(enum1), EnumToggle.On, nameof(enum2), EnumToggle.On)] public string bool12AndEnum12Show;

        [SepTitle("Bool3+Enum", EColor.Gray)]
        // 3+1
        [ShowIf(nameof(bool1), nameof(bool2), nameof(bool3), nameof(enum1), EnumToggle.On)] public string bool123AndEnum1Show;
    }
}

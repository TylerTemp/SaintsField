using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.ShowHideExamples
{
    public class HideIfEnumExample : MonoBehaviour
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

        [HideIf(nameof(enum1), EnumToggle.On)] public string enum1Hide;
        [HideIf(nameof(enum1), EnumToggle.On, nameof(enum2), EnumToggle.On)] public string enum1And2Hide;
        [HideIf(nameof(enum1), EnumToggle.On, nameof(enum2), EnumToggle.On, nameof(enum3), EnumToggle.On)] public string enum1And2And3Hide;
        [HideIf(nameof(enum1), EnumToggle.On, nameof(enum2), EnumToggle.On, nameof(enum3), EnumToggle.On, nameof(enum4), EnumToggle.On)] public string enum1And2And3And4Hide;

        [SepTitle("Bool1+Enum", EColor.Gray)]
        // 1+1
        [HideIf(nameof(bool1), nameof(enum1), EnumToggle.On)] public string bool1AndEnum1Hide;
        // 1+2
        [HideIf(nameof(bool1), nameof(enum1), EnumToggle.On, nameof(enum2), EnumToggle.On)] public string bool1AndEnum12Hide;
        // 1+3
        [HideIf(nameof(bool1), nameof(enum1), EnumToggle.On, nameof(enum2), EnumToggle.On, nameof(enum3), EnumToggle.On)] public string bool1AndEnum123;

        [SepTitle("Bool2+Enum", EColor.Gray)]
        // 2+1
        [HideIf(nameof(bool1), nameof(bool2), nameof(enum1), EnumToggle.On)] public string bool12AndEnum1Hide;
        // 2+2
        [HideIf(nameof(bool1), nameof(bool2), nameof(enum1), EnumToggle.On, nameof(enum2), EnumToggle.On)] public string bool12AndEnum12Hide;

        [SepTitle("Bool3+Enum", EColor.Gray)]
        // 3+1
        [HideIf(nameof(bool1), nameof(bool2), nameof(bool3), nameof(enum1), EnumToggle.On)] public string bool123AndEnum1Hide;
    }
}

﻿using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.ShowHideExamples
{
    public class ShowHideSimpleExample: MonoBehaviour
    {
        [FieldHideIf] public string justHide;
        [FieldShowIf] public string justShow;

        public bool condition;

        [FieldShowIf(nameof(condition))] public string boolShow;
        [FieldHideIf(nameof(condition))] public string boolHide;

        [Serializable]
        public enum Enum1
        {
            Off,
            On,
        }

        public Enum1 enum1;

        [Serializable]
        public enum Enum2
        {
            Off,
            On,
        }

        public Enum2 enum2;

        [FieldShowIf(nameof(enum1), Enum1.On)] public string enum1Show;
        [FieldShowIf(nameof(enum1), Enum1.On, nameof(enum2), Enum2.On)] public string enum1And2Show;

        [Serializable, Flags]
        public enum EnumFlag
        {
            Flag1 = 1,
            Flag2 = 1 << 1,
            Flag3 = 1 << 2,

            Flag1And3 = Flag1 | Flag3,
        }

        [EnumToggleButtons]
        public EnumFlag enumFlag;

        [FieldShowIf(nameof(enumFlag), EnumFlag.Flag1 | EnumFlag.Flag3)] public string flag1Show;
    }
}

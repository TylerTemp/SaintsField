using System;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class AliasDisableIfTest : SaintsMonoBehaviour
    {
        [ReadOnly(nameof(ShouldBeDisabled))] public string disableMe;

        private bool ShouldBeDisabled()  // change the logic here
        {
            return true;
        }

        // This also works on static/const callbacks using `$:`
        [DisableIf("$:" + nameof(Util) + "." + nameof(Util._shouldDisable))] public int disableThis;
        // you can put this under another file like `Util.cs`
        public static class Util
        {
            public static bool _shouldDisable;
        }

        [ShowInInspector]
        private static bool s
        {
            get => Util._shouldDisable;
            set => Util._shouldDisable = value;
        }


        [Serializable]
        public enum EnumToggle
        {
            Off,
            On,
        }

        public EnumToggle enum1;
        public EnumToggle enum2;
        public bool bool1;
        public bool bool2;

        [ReadOnly(nameof(enum1), EnumToggle.On)] public string enumReadOnly;
        // example of checking two normal callbacks and two enum callbacks
        [EnableIf(
            nameof(bool1), nameof(bool2),
            nameof(enum1), EnumToggle.On,
            nameof(enum2), EnumToggle.On
            )] public string bool12AndEnum12;

        [ReadOnly] public string directlyReadOnly;

        [SerializeField] private bool _bool1;
        [SerializeField] private bool _bool2;
        [SerializeField] private bool _bool3;
        [SerializeField] private bool _bool4;

        [SerializeField]
        [ReadOnly(nameof(_bool1))]
        [ReadOnly(nameof(_bool2))]
        [LabelText("readonly=1||2")]
        private string _ro1and2;


        [SerializeField]
        [ReadOnly(nameof(_bool1), nameof(_bool2))]
        [LabelText("readonly=1&&2")]
        private string _ro1or2;


        [SerializeField]
        [ReadOnly(nameof(_bool1), nameof(_bool2))]
        [ReadOnly(nameof(_bool3), nameof(_bool4))]
        [LabelText("readonly=(1&&2)||(3&&4)")]
        private string _ro1234;
    }
}

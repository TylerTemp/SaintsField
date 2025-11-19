using System;
using SaintsField.Playa;
using SaintsField.Samples.Scripts.Interface;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    // [CreateAssetMenu(fileName = "Scriptable", menuName = "ScriptableObjects/Scriptable", order = 0)]
    [Searchable]
    public class Scriptable : SaintsScriptableObject, IInterface2, IDummy
    {
        [TableHide] public int hideMeInTable;

        [Button] private void AButton(){}

        [TableColumn("HideGroup"), TableHide]
        public int hideMeGroup1;

        [TableColumn("HideGroup")] [ShowInInspector]
        private const int HideMeGroup2 = 2;

        [SerializeField]
        [FieldLabelText("<color=red><label /></color>")]
        [PropRange(0, 100)]
        private int _intRange;

        [Range(0, 100)] public int normalRange;

        public AnimationCurve animationCurve;
        public Gradient gradient;
        public Hash128 hash128Value;

        [TableColumn("Basic!")]
        public int publicValue;

        [TableColumn("Basic!")]
        [NoLabel]
        public string noLabel;

        [Serializable]
        public struct MyStruct
        {
            public int structContent;
        }

        public MyStruct myStruct;

        public MyStruct[] myStructs;

        [TableColumn("Buttons")]
        [Button] private void B1() { }

        [TableColumn("Buttons")]
        [Button] private void B2() { }

        [ShowInInspector] private int _showI;

        public string GetComment()
        {
            return "";
        }

        [field: SerializeField]
        public int MyInt { get; set; }
    }
}

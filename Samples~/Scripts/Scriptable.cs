using System;
using SaintsField.Playa;
using SaintsField.Samples.Scripts.Interface;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    // [CreateAssetMenu(fileName = "Scriptable", menuName = "ScriptableObjects/Scriptable", order = 0)]
    public class Scriptable : ScriptableObject, IInterface2, IDummy
    {
        [SerializeField]
        [RichLabel("<color=red><label /></color>")]
        [PropRange(0, 100)]
        private int _intRange;

        [Range(0, 100)] public int normalRange;

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
        public string GetComment()
        {
            return "";
        }

        [field: SerializeField]
        public int MyInt { get; set; }

        [TableColumn("Buttons")]
        [Button] private void B1() { }

        [TableColumn("Buttons")]
        [Button] private void B2() { }

        [ShowInInspector] private int _showI;
    }
}

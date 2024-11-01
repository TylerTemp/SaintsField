using System;
using SaintsField.Samples.Scripts.Interface;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    // [CreateAssetMenu(fileName = "Scriptable", menuName = "ScriptableObjects/Scriptable", order = 0)]
    public class Scriptable : ScriptableObject, IInterface2
    {
        [SerializeField]
        [RichLabel("<color=red><label /></color>")]
        [PropRange(0, 100)]
        private int _intRange;

        [Range(0, 100)] public int normalRange;

        public int publicValue;

        [RichLabel(null)]
        public string noLabel;

        [Serializable]
        public struct MyStruct
        {
            public int structContent;
        }

        public MyStruct myStruct;

        public MyStruct[] myStructs;
    }
}

using System;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Testing
{
    public class SerEnumULong : SaintsMonoBehaviour
    {
        // [Serializable, Flags]
        // public enum LongEnum: long
        // {
        //     First = 1,
        //     Second = 1 << 1,
        //     Third = 1 << 2,
        // }
        //
        // public LongEnum longEnumPub;

        [Serializable, Flags]
        public enum TestULongEnum: ulong
        {
            First = 1,
            Second = 1 << 1,
            Third = 1 << 2,
        }

        // [NonSerialized, SaintsSerialized]
        // // ReSharper disable once InconsistentNaming
        // public TestULongEnum uLongEnumPub;
        // [SerializeField] private TestULongEnum _uLongEnumPri;
        // [field: SerializeField] public TestULongEnum ULongEnumProp { get; private set; }

        [NonSerialized, SaintsSerialized]
        // ReSharper disable once InconsistentNaming
        public TestULongEnum[] uLongEnumPubArr;
        // [SerializeField] private TestULongEnum[] _uLongEnumPriArr;
        // [field: SerializeField] public TestULongEnum[] ULongEnumPropArr { get; private set; }
        //
        // public List<TestULongEnum> uLongEnumPubLis;
        // [SerializeField] private List<TestULongEnum> _uLongEnumPriLis;
        // [field: SerializeField] public List<TestULongEnum> ULongEnumPropLis { get; private set; }
    }
}

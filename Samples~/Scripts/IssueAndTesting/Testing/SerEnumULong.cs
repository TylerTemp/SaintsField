using System;
using System.Collections.Generic;
using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Testing
{
    public class SerEnumULong : SaintsMonoBehaviour
    {
        [Serializable, Flags]
        public enum LongEnum: long
        {
            First = 1,
            Second = 1 << 1,
            Third = 1 << 2,
        }

        [NonSerialized, SaintsSerialized] public LongEnum LongEnumPub;

        [Serializable, Flags]
        public enum TestULongEnum: ulong
        {
            First = 1,
            Second = 1 << 1,
            Third = 1 << 2,
        }

        [NonSerialized, SaintsSerialized] public TestULongEnum ULongEnumPub;
        [NonSerialized, SaintsSerialized] private TestULongEnum _uLongEnumPri;
        [field: NonSerialized, SaintsSerialized] public TestULongEnum ULongEnumProp { get; private set; }

        [NonSerialized, SaintsSerialized] public TestULongEnum[] ULongEnumPubArr;
        [NonSerialized, SaintsSerialized] private TestULongEnum[] _uLongEnumPriArr;
        [field: NonSerialized, SaintsSerialized] public TestULongEnum[] ULongEnumPropArr { get; private set; }

        [NonSerialized, SaintsSerialized] public List<TestULongEnum> ULongEnumPubLis;
        [NonSerialized, SaintsSerialized] private List<TestULongEnum> _uLongEnumPriLis;
        [field: NonSerialized, SaintsSerialized] public List<TestULongEnum> ULongEnumPropLis { get; private set; }
    }
}

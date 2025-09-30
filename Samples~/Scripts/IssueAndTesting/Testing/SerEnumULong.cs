using System;
using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Testing
{
    // [SaintsSerialized]
    public partial class SerEnumULong : SaintsMonoBehaviour
    {
        [Serializable, Flags]
        public enum LongEnum: long
        {
            First = 1,
            Second = 1 << 1,
            Third = 1 << 2,
            M12 = First | Second,
        }

        [Serializable, Flags]
        public enum TestULongEnum: ulong
        {
            No = 0,
            First = 1,
            Second = 1 << 1,
            Third = 1 << 2,
            M12 = First | Second,
            All = First | Second | Third,
        }

        [Serializable]
        public enum TestULongEnumNormal: ulong
        {
            None,
            First,
            Second,
            Third,
        }

        // [NonSerialized, SaintsSerialized] public LongEnum LongEnumPub;
        // [NonSerialized, SaintsSerialized] public LongEnum[] LongEnumPubArr;
        //
        // [NonSerialized, SaintsSerialized, EnumToggleButtons] public TestULongEnum ULongEnumPub;
        // [NonSerialized, SaintsSerialized, EnumToggleButtons] public TestULongEnum[] ULongEnumPubs;
        // [NonSerialized, SaintsSerialized, EnumToggleButtons] public TestULongEnumNormal ULongEnumNormalPub;
        // [NonSerialized, SaintsSerialized] private TestULongEnum _uLongEnumPri;
        // [field: NonSerialized, SaintsSerialized] public TestULongEnum ULongEnumProp { get; private set; }
        //
        // [NonSerialized, SaintsSerialized] public TestULongEnum[] ULongEnumPubArr;
        // [NonSerialized, SaintsSerialized] private TestULongEnum[] _uLongEnumPriArr;
        // [field: NonSerialized, SaintsSerialized] public TestULongEnum[] ULongEnumPropArr { get; private set; }

        // public int serFiledMe;
        //
        // [NonSerialized, SaintsSerialized] public List<TestULongEnum> ULongEnumPubLis;
        // [NonSerialized, SaintsSerialized] private List<TestULongEnum> _uLongEnumPriLis;
        // public List<TestULongEnum> ULongEnumPropLis { get; private set; }

        [Serializable, SaintsSerialized]
        public partial struct Nested1
        {
            [Serializable, SaintsSerialized]
            public partial struct InsideClare
            {
                // [NonSerialized, SaintsSerialized] public TestULongEnum Inside;
                [NonSerialized, SaintsSerialized] public TestULongEnum[] Insides;
            }

            // [NonSerialized, SaintsSerialized] public TestULongEnum InNested1;
            // [NonSerialized, SaintsSerialized] public TestULongEnum[] InNested1Arr;
            // public InsideClare insideClare;
            public InsideClare[] insideClareArr;
        }

        // [SaintsSerialized] public Nested1 nested1;


        [Serializable, SaintsSerialized]
        public partial struct Nested2
        {
            [Serializable, SaintsSerialized]
            public partial struct InsideClare
            {
                // [NonSerialized, SaintsSerialized] public TestULongEnum Inside;
                [NonSerialized, SaintsSerialized] public TestULongEnum[] Insides;
            }

            // [NonSerialized, SaintsSerialized] public TestULongEnum InNested1;
            // [NonSerialized, SaintsSerialized] public TestULongEnum[] InNested1Arr;
            // public InsideClare insideClare;
            public InsideClare[] insideClareArr;
        }

        // [Button]
        // private void InspectIt()
        // {
        //     Debug.Log(ULongEnumPubArr);
        //     Debug.Log(ULongEnumPubArr == null);
        // }
    }
}

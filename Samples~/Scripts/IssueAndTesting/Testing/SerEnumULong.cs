using System;
using System.Collections.Generic;
using SaintsField.Playa;
using UnityEngine;
using UnityEngine.Serialization;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Testing
{
    public partial class SerEnumULong : SaintsMonoBehaviour
    {
        [Serializable, Flags]
        public enum LongEnum: long
        {
            First = 1,
            Second = 1 << 1,
            M12 = First | Second,
        }

        [Serializable, Flags]
        public enum TestULongEnum: ulong
        {
            No = 0,
            First = 1,
            Second = 1 << 1,
            Third = 1 << 2,
            Fourth = 1 << 3,
            Fifth = 1 << 4,
            Sixth = 1 << 5,
            Seventh = 1 << 6,
            M12 = First | Second,
            First3 = First | Second | Third,
        }

        [Serializable]
        public enum TestULongEnumNormal: ulong
        {
            First = 1,
            Second,
            Third,
        }

        [NonSerialized, SaintsSerialized] public TestULongEnumNormal ULongEnumNormalNoButton;

        [NonSerialized, SaintsSerialized] public LongEnum LongEnumPub;

        [NonSerialized, SaintsSerialized] public LongEnum[] LongEnumPubArr;

        [NonSerialized, SaintsSerialized, EnumToggleButtons] public TestULongEnum ULongEnumPub;
        [NonSerialized, SaintsSerialized, EnumToggleButtons] public TestULongEnum[] ULongEnumPubs;
        [NonSerialized, SaintsSerialized, EnumToggleButtons] public TestULongEnumNormal ULongEnumNormalPub;
        [SaintsSerialized] private TestULongEnum _uLongEnumPri;
        [field: SaintsSerialized] public TestULongEnum ULongEnumProp { get; private set; }

        [NonSerialized, SaintsSerialized] public TestULongEnum[] ULongEnumPubArr;
        [SaintsSerialized] private TestULongEnum[] _uLongEnumPriArr;
        [field: SaintsSerialized] public TestULongEnum[] ULongEnumPropArr { get; private set; }

        [NonSerialized, SaintsSerialized] public List<TestULongEnum> ULongEnumPubLis;
        [SaintsSerialized] private List<TestULongEnum> _uLongEnumPriLis;
        public List<TestULongEnum> ULongEnumPropLis { get; private set; }

        [Serializable]
        public partial struct Nested1
        {
            [Serializable]
            public partial struct InsideClare
            {
                [NonSerialized, SaintsSerialized] public TestULongEnum Inside;
                [NonSerialized, SaintsSerialized] public TestULongEnum[] Insides;
            }

            [NonSerialized, SaintsSerialized] public TestULongEnum InNested1;
            [NonSerialized, SaintsSerialized] public TestULongEnum[] InNested1Arr;
            public InsideClare insideClare;
            public InsideClare[] insideClareArr;
        }

        public Nested1 nested1;

        // [Button] private TestULongEnum inNest() => nested1.InNested1;
        // [ShowInInspector] private object D => nested1__SaintsSerialized__;

        [Serializable]
        public partial struct Nested2
        {
            [Serializable]
            public partial struct InsideClare
            {
                // [NonSerialized, SaintsSerialized] public TestULongEnum Inside;
                [NonSerialized, SaintsSerialized] public TestULongEnum[] Insides;
            }

            // [NonSerialized, SaintsSerialized] public TestULongEnum InNested1;
            // [NonSerialized, SaintsSerialized] public TestULongEnum[] InNested1Arr;
            public InsideClare insideClare;
            public InsideClare[] insideClareArr;
        }

        public Nested2 nested2;

        // [Button]
        // private void InspectIt()
        // {
        //     Debug.Log(ULongEnumPubArr);
        //     Debug.Log(ULongEnumPubArr == null);
        // }
    }
}

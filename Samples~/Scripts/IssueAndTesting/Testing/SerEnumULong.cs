using System;
using System.Collections.Generic;
using SaintsField.Playa;
using UnityEngine;
using UnityEngine.Serialization;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Testing
{
    public partial class SerEnumULong : SaintsMonoBehaviour
    {
        // [Serializable]

        [Flags]
        public enum LongEnum: long
        {
            First = 1,
            Second = 1 << 1,
            M12 = First | Second,
        }

        // [Serializable]
        [Flags]
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

        // [Serializable]
        public enum TestULongEnumNormal: ulong
        {
            First = 1,
            Second,
            Third,
        }

        // [Serializable]
        public enum TestLongEnumNormal: long
        {
            First = 1,
            Second,
            Third,
        }

        [SaintsSerialized, NonSerialized, EnumToggleButtons, OnValueChanged(":Debug.Log")] private TestLongEnumNormal _longNormal;
        [SaintsSerialized, NonSerialized, EnumToggleButtons, OnValueChanged(":Debug.Log")] private LongEnum _longFlags;
        [SaintsSerialized, NonSerialized, EnumToggleButtons, OnValueChanged(":Debug.Log")] private TestULongEnumNormal _uLongNormal;
        [SaintsSerialized, NonSerialized, EnumToggleButtons, OnValueChanged(":Debug.Log")] private TestULongEnum _uLongFlags;

        [SaintsSerialized, NonSerialized, OnValueChanged(":Debug.Log")] private TestULongEnumNormal ULongEnumNormalNoButton;
        [SaintsSerialized, NonSerialized, OnValueChanged(":Debug.Log")] private LongEnum LongEnumPub;
        [SaintsSerialized, NonSerialized, OnValueChanged(":Debug.Log")] private LongEnum[] LongEnumPubArr;

        [SaintsSerialized, NonSerialized, EnumToggleButtons] private TestULongEnum ULongEnumPub;
        [SaintsSerialized, NonSerialized, EnumToggleButtons] private TestULongEnum[] ULongEnumPubs;
        [SaintsSerialized, NonSerialized, EnumToggleButtons] private TestULongEnumNormal ULongEnumNormalPub;
        [SaintsSerialized, NonSerialized] private TestULongEnum _uLongEnumPri;
        [field: SaintsSerialized, NonSerialized] public TestULongEnum ULongEnumProp { get; private set; }

        [SaintsSerialized, NonSerialized] public TestULongEnum[] ULongEnumPubArr;
        [SaintsSerialized, NonSerialized] private TestULongEnum[] _uLongEnumPriArr;
        [field: SaintsSerialized, NonSerialized] public TestULongEnum[] ULongEnumPropArr { get; private set; }

        [SaintsSerialized, NonSerialized] public List<TestULongEnum> ULongEnumPubLis;
        [SaintsSerialized, NonSerialized] private List<TestULongEnum> _uLongEnumPriLis;
        public List<TestULongEnum> ULongEnumPropLis { get; private set; }

        [Serializable]
        public partial struct Nested1
        {
            [Serializable]
            public partial struct InsideClare
            {
                [SaintsSerialized, NonSerialized] public TestULongEnum Inside;
                [SaintsSerialized, NonSerialized] public TestULongEnum[] Insides;
            }

            [SaintsSerialized, NonSerialized] public TestULongEnum InNested1;
            [SaintsSerialized, NonSerialized] public TestULongEnum[] InNested1Arr;
            public InsideClare insideClare;
            public InsideClare[] insideClareArr;
        }

        public Nested1 nested1;

        [Serializable]
        public partial struct Nested2
        {
            [Serializable]
            public partial struct InsideClare
            {
                // [NonSerialized, SaintsSerialized, NonSerialized] public TestULongEnum Inside;
                [SaintsSerialized, NonSerialized] public TestULongEnum[] Insides;
            }

            // [NonSerialized, SaintsSerialized, NonSerialized] public TestULongEnum InNested1;
            // [NonSerialized, SaintsSerialized, NonSerialized] public TestULongEnum[] InNested1Arr;
            public InsideClare insideClare;
            public InsideClare[] insideClareArr;
        }

        public Nested2 nested2;
    }
}

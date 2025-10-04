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
            First = 1,
            Second,
            Third,
        }

        [HideInInspector, NonSerialized, SaintsSerialized] public TestULongEnumNormal ULongEnumNormalNoButton;

        [HideInInspector, NonSerialized, SaintsSerialized] public LongEnum LongEnumPub;

        [HideInInspector, NonSerialized, SaintsSerialized] public LongEnum[] LongEnumPubArr;

        [HideInInspector, NonSerialized, SaintsSerialized, EnumToggleButtons] public TestULongEnum ULongEnumPub;
        [HideInInspector, NonSerialized, SaintsSerialized, EnumToggleButtons] public TestULongEnum[] ULongEnumPubs;
        [HideInInspector, NonSerialized, SaintsSerialized, EnumToggleButtons] public TestULongEnumNormal ULongEnumNormalPub;
        [SaintsSerialized] private TestULongEnum _uLongEnumPri;
        [field: SaintsSerialized] public TestULongEnum ULongEnumProp { get; private set; }

        [HideInInspector, NonSerialized, SaintsSerialized] public TestULongEnum[] ULongEnumPubArr;
        [SaintsSerialized] private TestULongEnum[] _uLongEnumPriArr;
        [field: SaintsSerialized] public TestULongEnum[] ULongEnumPropArr { get; private set; }

        [HideInInspector, NonSerialized, SaintsSerialized] public List<TestULongEnum> ULongEnumPubLis;
        [SaintsSerialized] private List<TestULongEnum> _uLongEnumPriLis;
        public List<TestULongEnum> ULongEnumPropLis { get; private set; }

        [Serializable]
        public partial struct Nested1
        {
            [Serializable]
            public partial struct InsideClare
            {
                [HideInInspector, NonSerialized, SaintsSerialized] public TestULongEnum Inside;
                [HideInInspector, NonSerialized, SaintsSerialized] public TestULongEnum[] Insides;
            }

            [HideInInspector, NonSerialized, SaintsSerialized] public TestULongEnum InNested1;
            [HideInInspector, NonSerialized, SaintsSerialized] public TestULongEnum[] InNested1Arr;
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
                // [HideInInspector, NonSerialized, SaintsSerialized] public TestULongEnum Inside;
                [HideInInspector, NonSerialized, SaintsSerialized] public TestULongEnum[] Insides;
            }

            // [HideInInspector, NonSerialized, SaintsSerialized] public TestULongEnum InNested1;
            // [HideInInspector, NonSerialized, SaintsSerialized] public TestULongEnum[] InNested1Arr;
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

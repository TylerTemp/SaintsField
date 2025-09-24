using System;
using System.Collections.Generic;
using SaintsField.Playa;
using UnityEngine;

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
        }

        [Serializable, Flags]
        public enum TestULongEnum: ulong
        {
            None = 0,
            First = 1,
            Second = 1 << 1,
            Third = 1 << 2,
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


        [NonSerialized, SaintsSerialized]
        public LongEnum LongEnumPub;

        [NonSerialized, SaintsSerialized, EnumToggleButtons] public TestULongEnum ULongEnumPub;
        [NonSerialized, SaintsSerialized, EnumToggleButtons] public TestULongEnumNormal ULongEnumNormalPub;
        [NonSerialized, SaintsSerialized] private TestULongEnum _uLongEnumPri;
        [field: NonSerialized, SaintsSerialized] public TestULongEnum ULongEnumProp { get; private set; }

        [NonSerialized, SaintsSerialized] public TestULongEnum[] ULongEnumPubArr;
        [NonSerialized, SaintsSerialized] private TestULongEnum[] _uLongEnumPriArr;
        [field: NonSerialized, SaintsSerialized] public TestULongEnum[] ULongEnumPropArr { get; private set; }

        [NonSerialized, SaintsSerialized] public List<TestULongEnum> ULongEnumPubLis;
        [NonSerialized, SaintsSerialized] private List<TestULongEnum> _uLongEnumPriLis;
        [field: NonSerialized, SaintsSerialized] public List<TestULongEnum> ULongEnumPropLis { get; private set; }

        // [ShowInInspector] private string GenString => GeneratedStringSaintsField();
        // [ShowInInspector] private string GenString2 => ExampleSourceGenerated.ExampleSourceGenerated.GetTestText();

        // [Button]
        // private void InspectIt()
        // {
        //     Debug.Log(ULongEnumPubArr);
        //     Debug.Log(ULongEnumPubArr == null);
        // }
    }
}

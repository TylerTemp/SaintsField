using System;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class NewParserTest : SaintsMonoBehaviour
    {
#pragma warning disable CS0067 // Event is never used
        // ReSharper disable once InconsistentNaming
        public event Action myEventField;
#pragma warning restore CS0067 // Event is never used

        [Serializable]
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once UnusedTypeParameter
        public class Part1<U,
            // ReSharper disable once InconsistentNaming
            // ReSharper disable once UnusedTypeParameter
            V>
        {

            [Serializable]
            public class TestNestedStructForParse<T>
            {
                [Button]
                private void BtnInT() {}
                public string stringInT;
            }

            [Serializable]
            public class TestNestedStructForParse: TestNestedStructForParse<GameObject>
            {
                // public GameObject go;
                [Button]
                private void BtnInGo() {}
                public GameObject inGo;
            }

            public TestNestedStructForParse<int> testNestedStructForParse;
        }

        public Part1<int, int>.TestNestedStructForParse testNested;
    }
}

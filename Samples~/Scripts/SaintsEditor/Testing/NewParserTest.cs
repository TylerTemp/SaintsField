using System;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class NewParserTest : SaintsMonoBehaviour
    {
        public event Action myEventField;

        [Serializable]
        public class Part1<U, V>
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

using System;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsRowExamples
{
    public class SaintsRowListDrawerExample : MonoBehaviour
    {
        [Serializable]
        public struct MyData
        {
            public int myInt;
            public string myString;
            public GameObject myGameObject;
            public string[] myStrings;
        }

        [Serializable]
        public struct MyStruct
        {
            public string above;

            [ListDrawerSettings(
                searchable: true
                , numberOfItemsPerPage: 3
            ), PlayaRichLabel("<color=green><icon=star.png/><label/>")]
            public MyData[] myDataArr;

            public string below;
        }

        [SaintsRow] public MyStruct myStruct;
    }
}

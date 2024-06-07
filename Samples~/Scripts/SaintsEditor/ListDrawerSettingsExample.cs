using System;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public class ListDrawerSettingsExample : MonoBehaviour
    {
        [Serializable]
        public struct MyStruct
        {
            public int myInt;
            public string myString;
            public GameObject myGameObject;
        }

        [ListDrawerSettings(numberOfItemsPerPage: 5)]
        public MyStruct[] myStructArr;
    }
}

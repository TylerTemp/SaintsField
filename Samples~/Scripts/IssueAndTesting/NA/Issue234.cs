using System;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.NA
{
    public class Issue234 : MonoBehaviour
    {
        [Serializable]
        public struct MyStruct
        {
            public int structInt;
            public bool structBool;

            [Button]
            public void StructBtn()
            {
                Debug.Log("Call StructBtn");
            }

            [ShowInInspector] public static Color structStaticColor = Color.blue;
        }

        [SaintsRow]
        public MyStruct myStruct;

        [SaintsRow(inline: true)]
        public MyStruct myStructInline;
    }
}

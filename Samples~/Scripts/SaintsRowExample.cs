using System;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class SaintsRowExample : MonoBehaviour
    {
        [Serializable]
        public struct MyStruct
        {
            // public string normalField;

            [PlayaRichLabel("<color=green><icon=star.png/><label/>")]
            public string[] myStrings;

            [Button]
            private void OnButton()
            {
                Debug.Log("Button clicked");
            }

            [Button]
            private void OnButtonParams(UnityEngine.Object myObj, int myInt, string myStr = "hi")
            {
                Debug.Log($"{myObj}, {myInt}, {myStr}");
            }
        }

        [SaintsRow] public MyStruct myStruct;
        [SaintsRow(inline: true)] public MyStruct myStructInline;
    }
}

using System;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsRowExamples
{
    public class SaintsRowExample : MonoBehaviour
    {
        [Serializable]
        public struct MyStruct
        {
            // public string normalField;

            [PlayaRichLabel("<color=green><icon=star.png/><label/>"), PlayaInfoBox("Info box for array with long long long long long long long long long long text", groupBy: "above"), PlayaInfoBox("Info box for array", groupBy: "above")]
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

            [PlayaInfoBox("Above Box")]
            [PlayaBelowInfoBox("Below Box")]
            private void EmptyFunction()
            {
            }
        }

        [SaintsRow] public MyStruct myStruct;
        [SaintsRow(inline: true)] public MyStruct myStructInline;
    }
}

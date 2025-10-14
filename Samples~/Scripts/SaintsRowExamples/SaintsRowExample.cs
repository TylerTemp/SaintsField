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

            [LabelText("<color=green><icon=star.png/><label/>"), InfoBox("Info box for array with long long long long long long long long long long text", groupBy: "above"), InfoBox("Info box for array", groupBy: "above")]
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

            [InfoBox("Above Box")]
            [BelowInfoBox("Below Box")]
            private void EmptyFunction()
            {
            }

            public enum When
            {
                None,
                Any,
                All,
            }

            public When when;
        }

        [SaintsRow] public MyStruct myStruct;
        [SaintsRow(inline: true)] public MyStruct myStructInline;

        public MyStruct myStructOri;
    }
}

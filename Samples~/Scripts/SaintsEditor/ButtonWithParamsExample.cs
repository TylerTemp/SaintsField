using System.Collections.Generic;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public class ButtonWithParamsExample : SaintsMonoBehaviour
    {
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

        private interface IParam
        {
            int A { get; }
            int B { get; }
        }

        private struct StructPara: IParam
        {
            public int A { get; private set; }
            // public int B { get; private set; }
            public int BValue;
            public int B => BValue;
        }

        private class MyDummyClass : IDummy
        {
            private string _comment;

            public string GetComment() => _comment;

            public int MyInt { get; set; }
        }

        [Button]
        private void OnButtonStruct(IDummy myDummy, IParam myParam, Dictionary<string, int> myD)
        {
            Debug.Log($"{myDummy?.MyInt}, {myDummy?.GetComment()}/{myParam?.A}, {myParam?.B}");
            // ReSharper disable once InvertIf
            if(myD != null)
            {
                foreach (KeyValuePair<string, int> kv in myD)
                {
                    Debug.Log($"dict: {kv.Key}, {kv.Value}");
                }
            }
        }

        [Separator(10)]

        [Button]  // Display the returned value when clicked
        private int AddCalculator(int a, int b) => a + b;

        [GetComponentInChildren] public GameObject[] goLis;

        private class ResultClass
        {
            public GameObject Go;
        }

        [Separator(10)]

        [Button]  // A struct, class, UnityObject return type is supported too
        private ResultClass ReturnClass(int v) => new ResultClass
        {
            Go = goLis[v % goLis.Length]
        };

        [Separator(10)]

        [Button(hideReturnValue: true)]  // Hide the returned value
        private int ReturnIgnored() => Random.Range(0, 100);
    }
}

using System;
using System.Collections;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Testing
{
    public class MultipleButton : MonoBehaviour
    {
        [PostFieldButton(nameof(Click))]
        [PostFieldButton(nameof(Generate))]
        public string s;

        private void Click(string v) => Debug.Log(v);

        private IEnumerator Generate(string v)
        {
            int count = UnityEngine.Random.Range(50, 300);
            for (int i = 0; i < count; i++)
            {
                Debug.Log($"{v}: {i}");
                yield return null;
            }
        }

        [Serializable]
        public struct MyStruct
        {
            [PostFieldButton(nameof(C))]
            [PostFieldButton(nameof(G))]
            public string myString;

            private void C(string v)
            {
                Debug.Log(v);
            }

            private IEnumerator G(string v)
            {
                int count = UnityEngine.Random.Range(50, 300);
                for (int i = 0; i < count; i++)
                {
                    Debug.Log($"{v}: {i}");
                    yield return null;
                }
            }

        }

        [SaintsRow] public MyStruct myStruct;

        [SaintsRow] public MyStruct[] myStructs;
    }
}

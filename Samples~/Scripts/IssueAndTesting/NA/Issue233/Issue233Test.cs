using System;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.NA.Issue233
{
    public class Issue233Test : MonoBehaviour
    {
        [Serializable]
        public struct Nest2
        {
            public string s;
            [Button]
            private void Nest2Btn() => Debug.Log("Call Nest2Btn");
        }

        [Serializable]
        public struct Nest1
        {
            public string s;

            [Button]
            private void Nest1Btn() => Debug.Log("Call Nest1Btn");

            // [SaintsEditor]
            // public Nest2 n2;
        }

        [SaintsEditor]
        public Nest1 n1;
    }
}

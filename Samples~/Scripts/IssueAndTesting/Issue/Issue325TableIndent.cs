using System;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue325TableIndent : SaintsMonoBehaviour
    {
        [Serializable]
        public struct ColBase
        {
            public string c1;
            // public int c2;
        }

        [Serializable]
        public struct Container
        {
            [Table] public ColBase[] t1;
            [Table] public ColBase[] t2;
        }
        public Container container;

        [Button]
        private void ShowT1()
        {
            int index = 0;
            foreach (ColBase colBase in container.t1)
            {
                Debug.Log($"[{index}] {colBase.c1}/{colBase.c1 == null}");
                index++;
            }
            Debug.Log("done");
        }
    }
}

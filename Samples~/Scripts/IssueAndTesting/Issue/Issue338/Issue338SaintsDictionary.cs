using System;
using System.Collections.Generic;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue338
{
    public class Issue338SaintsDictionary : MonoBehaviour
    {
        [Serializable]
        public enum Context
        {
            C1,
            C2,
        }

        public SaintsDictionary<Context, ContextHintElement[]> sd;

        private void Awake()
        {
            foreach (KeyValuePair<Context, ContextHintElement[]> kv in sd)
            {
                Debug.Log($"dict {kv.Key}={string.Join<ContextHintElement>(",", kv.Value)}");
            }
        }
    }
}

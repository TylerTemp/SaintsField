using System;
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
    }
}

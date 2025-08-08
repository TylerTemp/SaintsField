using System;
using SaintsField.Samples.Scripts.SaintsEditor;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue279TableBg : MonoBehaviour
    {
        [Serializable]
        public struct Rule
        {
            public string name;
            public string[] conditions;
        }

        // [Table] public Rule[] rules;

        [Serializable]
        public struct Amount
        {
            // [Table]
            public Rule[] rules;
        }

        [Serializable]
        public struct BalancedUnit
        {
            public Amount amount;
        }

        [Serializable]
        public struct CrowdAttackScheduler
        {
            public BalancedUnit[] balancedUnits;
        }

        public CrowdAttackScheduler[] crowdAttackSchedulers;
    }
}

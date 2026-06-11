using System;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue279TableBg : SaintsMonoBehaviour
    {
        [Serializable]
        public struct Rule
        {
            public string name;
            [ListDrawerSettings]
            public string[] conditions;
        }

        // [Table] public Rule[] rules;

        [Serializable]
        public struct Amount
        {
            // [Table]
            [ListDrawerSettings]
            public Rule[] rules;
        }

        [Serializable]
        public struct BalancedUnit
        {
            public Amount amount;
            public string name;
        }

        [Serializable]
        public struct CrowdAttackScheduler
        {
            public BalancedUnit[] balancedUnits;
            public string fixedFocus;
            // public BalancedUnit balancedUnits;
        }

        [Table] public CrowdAttackScheduler[] crowdAttackSchedulers;
    }
}

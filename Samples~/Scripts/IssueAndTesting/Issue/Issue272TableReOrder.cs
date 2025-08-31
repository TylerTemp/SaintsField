using System;
using System.Collections.Generic;
using SaintsField.Samples.Scripts.SaintsEditor;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue272TableReOrder : SaintsMonoBehaviour
    {
        [Serializable]
        public class LevelMissionsToUnlock
        {
            [SerializeField] public int level;

            [Tooltip("Missions to unlock when the level is completed")] [SerializeField]
            public List<string> missionsToUnlock;
        }

        [Table, SerializeField] private List<LevelMissionsToUnlock> _levelsOrderAndCost;
    }
}

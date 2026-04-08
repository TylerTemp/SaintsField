using System;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue366NestedFoldoutBox : SaintsMonoBehaviour
    {
        [LayoutStart("various", ELayout.CollapseBox)]
        public string various;

        [Serializable]
        public struct SpawnByTime
        {
            public float time;
        }

        [LayoutStart("Spawner", ELayout.FoldoutBox | ELayout.Collapse)]
        public bool isActive;
        public bool isFriendly;


        [LayoutStart("Spawner by time", ELayout.FoldoutBox | ELayout.Collapse)]
        [Separator("Spawner by time (Always Active)")]
        [SaintsRow(inline: true)]
        public SpawnByTime spawnByTime;

        [LayoutStart("Spawner by time - when took one damage", ELayout.FoldoutBox | ELayout.Collapse)]
        [Separator("Spawner by time (Activated on Damage)")]
        [Tooltip("Bu SpawnByTime")]
        [ReadOnly]
        public bool hasTakenDamage;
    }
}

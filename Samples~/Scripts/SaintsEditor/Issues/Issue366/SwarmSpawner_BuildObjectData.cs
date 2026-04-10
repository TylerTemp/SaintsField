using System;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Issues.Issue366
{
    [Serializable]
    public class SwarmSpawner_BuildObjectData
    {
        [Serializable]
        public class SpawnByTimeConfig
        {
            public string s;
        }

        public bool isActive;
        public bool isFriendly;

        [LayoutStart("spawner by time", ELayout.FoldoutBox | ELayout.Collapse)]
        [Separator("Spawner by time (Always Active)")]
        [SaintsRow(true)]
        public SpawnByTimeConfig spawnByTime;

        [LayoutStart("Spawner by time - when took one damage", ELayout.FoldoutBox | ELayout.Collapse)]
        [Separator("Spawner by time (Activated on Damage)")]
        [Tooltip("Bu SpawnByTime")]
        [SaintsField.ReadOnly]
        public bool hasTakenDamage;

        [SaintsRow(true)] public SpawnByTimeConfig spawnByTime_WhenTookOneDamage;

        [LayoutStart("spawn on damage", ELayout.FoldoutBox | ELayout.Collapse)]
        public bool spawnOnDamage;

        public SwamAttackEntityCount spawnOnDamage_EntitiesToSpawn;

        public float spawnOnDamage_CooldownSeconds;
        public float spawnOnDamage_RemainingCooldown;
        public byte spawnOnDamage_damageCounter;


        [SaintsField.Separator("Spawn on Damage - Burst/Rest Cycle")]
        public Vector2Int spawnOnDamage_BurstCount;
        public Vector2Int spawnOnDamage_RestCount;
        public int spawnOnDamage_CurrentBurstRemaining;
        public int spawnOnDamage_CurrentRestRemaining;
        public int spawnOnDamage_ChainReactionRadius;

        [LayoutStart("positions",  ELayout.FoldoutBox | ELayout.Collapse)]
        public System.Collections.Generic.List<UnityEngine.Vector3Int> unitSpawnPositions;
        public System.Collections.Generic.List<UnityEngine.Vector3> unitSpawnPositions_ConvertedToVector3;

        public SwarmSpawner_BuildObjectData DeepClone()
        {
            return new SwarmSpawner_BuildObjectData();
        }

    }
}

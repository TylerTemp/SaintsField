using System;
using System.Collections;
using System.Collections.Generic;
using SaintsField.Playa;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue46
{
    public enum ALIANCETYPE
    {
        FRIEND,
        NEUTRAL,
        ENEMY
    }

    [CreateAssetMenu]
    public class MCTeam : SaintsScriptableObject
    {
        [Tooltip("for physics raycast, <everything> is fine but setting it can help optimize")]
        public LayerMask layers;

        public List<TeamCounter> aliances;
        [MinMaxSlider(-10, 10)] public Vector2 neutralRange = new Vector2(-1, 1);
#if UNITY_EDITOR
        void OnValidate()
        {
            for (var i = aliances.Count - 1; i >= 0; i--)
            {
                var a = aliances[i];
                a.neutralRange = neutralRange;
                if (a.team == this)
                {
                    Undo.RecordObject(this, name);
                    aliances.RemoveAt(i);
                }
                if (aliances.FindAll(tt => tt.team == a.team).Count > 1)
                {
                    Undo.RecordObject(this, name);
                    a.team = null;
                }
            }
        }
#endif
        public ALIANCETYPE GetAliance(MCTeam t) => aliances.Find(a => a.team == t).aliance;

        public ALIANCETYPE AddliancePoints(MCTeam t, int points)
        {
            TeamCounter teamCounter = aliances.Find(a => a.team == t);
            teamCounter.affinity += points;
            return teamCounter.aliance;
        }

        [Serializable]
        public class TeamCounter
        {
            [Layout("ream counter", ELayout.Horizontal)] [Required]
            public MCTeam team;

            [ProgressBar(-20, 20, step: 0.1f, colorCallback: nameof(FillColor))]
            public int affinity;

            public ALIANCETYPE aliance
            {
                get
                {
                    if (affinity > neutralRange.y) return ALIANCETYPE.FRIEND;
                    else if (affinity < neutralRange.x) return ALIANCETYPE.ENEMY;
                    else return ALIANCETYPE.NEUTRAL;
                }
            }

            [HideInInspector] public Vector2 neutralRange;
#if UNITY_EDITOR
            private EColor FillColor() => aliance == ALIANCETYPE.ENEMY
                ? EColor.Red
                : (aliance == ALIANCETYPE.FRIEND ? EColor.Green : EColor.Gray);
#endif
        }
    }

}

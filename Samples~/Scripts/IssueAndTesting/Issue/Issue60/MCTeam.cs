using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Playa;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue46
{
    public enum ALLIANCETYPE
    {
        SELF,
        FRIEND,
        NEUTRAL,
        ENEMY
    }

    [CreateAssetMenu]
    public class MCTeam : SaintsScriptableObject
    {
        [Tooltip("for physics raycast, <everything> is fine but setting it can help optimize")]
        public LayerMask layers;
        public List<TeamCounter> alliances;
        [MinMaxSlider(-10, 10)]
        public Vector2 neutralRange = new Vector2(-1, 1);
#if UNITY_EDITOR
        void OnValidate()
        {
            for (var i = alliances.Count - 1; i >= 0; i--) {
                var a = alliances[i];
                // Undo.RecordObject(this, name);
                a.neutralRange = neutralRange;
                if (a.team == this)
                {
                    alliances.RemoveAt(i);
                }
                if (alliances.FindAll(tt => tt.team == a.team).Count > 1)
                {
                    a.team = null;
                }
            }
        }
#endif
        public ALLIANCETYPE GetAlliance(MCTeam t) => alliances.Find(a => a.team == t).alliance;

        public (ALLIANCETYPE, float) ChangeAlliance(MCTeam t, float delta)
        {
            if (t == this) return (ALLIANCETYPE.SELF, 0);
            TeamCounter teamCounter = alliances.Find(a => a.team == t);
            teamCounter.affinity += delta;
            return (teamCounter.alliance, teamCounter.affinity);
        }

        [Serializable]
        public class TeamCounter
        {
            [Layout("ream counter", ELayout.Horizontal)]
            [Required]
            public MCTeam team;
#if UNITY_EDITOR
            [ProgressBar(-20, 20, step: 1f, colorCallback: nameof(FillColor))]
#endif
            public float affinity;
            public ALLIANCETYPE alliance
            {
                get {
                    if (affinity > neutralRange.y) return ALLIANCETYPE.FRIEND;
                    else if (affinity < neutralRange.x) return ALLIANCETYPE.ENEMY;
                    else return ALLIANCETYPE.NEUTRAL;
                }
            }
            [HideInInspector]
            public Vector2 neutralRange;
#if UNITY_EDITOR
            private EColor FillColor(float v)
            {
                Debug.Log($"{v}/{affinity}/{neutralRange}");
                return alliance == ALLIANCETYPE.ENEMY
                    ? EColor.Red
                    : (alliance == ALLIANCETYPE.FRIEND ? EColor.Green : EColor.Gray);
            }
#endif
        }
    }

}

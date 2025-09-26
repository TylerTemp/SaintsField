using System;
using System.Collections.Generic;
using SaintsField.Playa;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue46
{
    public enum ALLIANCETYPE { FRIEND,NEUTRAL,ENEMY,SELF }


    // [CreateAssetMenu]
    public class MCTeam: SaintsScriptableObject
    {
        [Serializable]
        public class TeamCounter
        {
            [Layout("team counter",ELayout.Horizontal)] [Required, Expandable]
            public MCTeam team;
            [ProgressBar(-20,20,step: 1f,colorCallback: nameof(FillColor))]   public float  affinity;
            public ALLIANCETYPE alliance
            {
                get {
                    if(affinity>neutralRange.y) return ALLIANCETYPE.FRIEND;
                    else
                    if(affinity<neutralRange.x) return ALLIANCETYPE.ENEMY;
                    else return ALLIANCETYPE.NEUTRAL;
                }
            }
            [HideInInspector] public Vector2 neutralRange;
            EColor FillColor() => alliance==ALLIANCETYPE.ENEMY ? EColor.Red : (alliance==ALLIANCETYPE.FRIEND ? EColor.Green : EColor.Gray);
        }

        [Layer]
        public int mainLayer, agentLayer;
        [Tooltip("for physics raycast, <everything> is fine but setting it can help optimize")] public LayerMask         layers;
        [ListDrawerSettings]
        public List<TeamCounter> alliances;
        [MinMaxSlider(-10,10)] //, free:true)]
        public Vector2 neutralRange = new Vector2(-1,1);
#if UNITY_EDITOR
        void OnValidate()
        {
            if(Application.isPlaying==false) {
                for (var i = alliances.Count-1;i>=0;i--) {
                    var a = alliances[i];
                    a.neutralRange = neutralRange;
                    if(a.team==this) {
                        Undo.RecordObject(this,name);
                        alliances.RemoveAt(i);
                    }
                    if(alliances.FindAll(tt => tt.team==a.team).Count>1) {
                        Undo.RecordObject(this,name);
                        a.team = null;
                    }
                }
            }
        }
#endif
        public ALLIANCETYPE GetAlliance(MCTeam t) => alliances.Find(a => a.team==t).alliance;

        public (ALLIANCETYPE,float) ChangeAlliance(MCTeam t,float delta)
        {
            if(t==this) return (ALLIANCETYPE.SELF,0);
            TeamCounter teamCounter = alliances.Find(a => a.team==t);
            teamCounter.affinity += delta;
            teamCounter.affinity = Mathf.Clamp(teamCounter.affinity,-20,20);
            return (teamCounter.alliance,teamCounter.affinity);
        }


    }

}

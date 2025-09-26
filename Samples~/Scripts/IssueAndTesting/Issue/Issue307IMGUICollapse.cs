using System;
using System.Collections.Generic;
using SaintsField.Playa;
using SaintsField.Samples.Scripts.IssueAndTesting.Issue46;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue307IMGUICollapse: SaintsMonoBehaviour
    {
        [Serializable]
        public class TeamCounter
        {
            // [Layout("team counter",ELayout.Horizontal)]
            [Required,Expandable] public MCTeam team;

            // [ProgressBar(-20,20,step: 1f,colorCallback: nameof(FillColor))]
            // public float  affinity;

            // public ALLIANCETYPE Alliance
            // {
            //     get {
            //         if(affinity>neutralRange.y) return ALLIANCETYPE.FRIEND;
            //         else
            //         if(affinity<neutralRange.x) return ALLIANCETYPE.ENEMY;
            //         else return ALLIANCETYPE.NEUTRAL;
            //     }
            // }
            //
            // [HideInInspector] public Vector2 neutralRange;
            //
            // private EColor FillColor() => Alliance==ALLIANCETYPE.ENEMY ? EColor.Red : (Alliance==ALLIANCETYPE.FRIEND ? EColor.Green : EColor.Gray);
        }

        public List<TeamCounter> alliances;
    }
}

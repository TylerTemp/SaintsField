using System;
#if UNITY_EDITOR
using SaintsField.Editor.Utils;
#endif

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue389
{
    [Serializable]
    public class BuildingAttackBrain
    {
        // [EnableIf(nameof(EnableIfDamage))]
        [EnableIf("..@as(BuildObject)")]
        [InfoBox("Only available in BuildObject type", show: "..@not-as(BuildObject)")]
        public int damage;
    }
}

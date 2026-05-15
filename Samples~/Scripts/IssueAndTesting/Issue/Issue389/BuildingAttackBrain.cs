using System;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue389
{
    [Serializable]
    public class BuildingAttackBrain
    {
        [DisableIf("../InMatchedType")]
        public int damage;
    }
}

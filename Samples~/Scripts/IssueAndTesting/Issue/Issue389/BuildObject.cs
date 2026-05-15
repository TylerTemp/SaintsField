using System;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue389
{
    [Serializable]
    public class BuildObject
    {
        [DefaultExpand]
        public BuildingAttackBrain buildingAttackBrain;

        // private bool InMatchedType => true;

        public override string ToString()
        {
            return $"<BuildObject {buildingAttackBrain} />";
        }
    }
}

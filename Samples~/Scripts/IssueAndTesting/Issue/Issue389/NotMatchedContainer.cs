using System;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue389
{
    [Serializable]
    public class NotMatchedContainer
    {
        [DefaultExpand]
        public BuildingAttackBrain buildingAttackBrain;

        private bool InMatchedType => false;
    }
}

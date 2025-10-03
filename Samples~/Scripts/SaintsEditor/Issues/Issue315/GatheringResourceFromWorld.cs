using System;

namespace SaintsField.Samples.Scripts.SaintsEditor.Issues.Issue315
{
    public class GatheringResourceFromWorld : SaintsMonoBehaviour
    {
        [Serializable]
        public class Gathering
        {
            public ValidCell cell;
        }

        [Table] public Gathering[] gatherings;
    }
}

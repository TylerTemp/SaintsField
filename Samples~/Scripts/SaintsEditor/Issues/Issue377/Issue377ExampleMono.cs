using System;
using SaintsField;

namespace Samples.Scripts.SaintsEditor.Issues.Issue377
{
    public class Issue377ExampleMono : SaintsMonoBehaviour
    {
        public BVC bvcField;
        public BVC[] bvcFields;

        [Serializable]
        public class BvcContainer
        {
            public BVC bvcField;
            public BVC[] bvcFields;
            public string otherField;
        }

        public BvcContainer bvcContainerField;
        public BvcContainer[] bvcContainerFieldArrry;
    }
}

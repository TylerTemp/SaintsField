using System;
using System.Collections.Generic;

namespace SaintsField.Samples.Scripts.SaintsEditor.Issues
{
    public class Issue321ListName : SaintsMonoBehaviour
    {
        [Serializable]
        public struct TestStruct
        {
            public string name;
            public int value;
        }

        public TestStruct[] fixedInNewVersion;

        [RichLabel("<field.name/>")]
        public TestStruct[] workaroundForOldVersion;
    }
}

using System;
using System.Collections.Generic;
using SaintsField.Samples.Scripts.SaintsEditor;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue157
{
    public class Issue157Main : SaintsMonoBehaviour
    {
        [Serializable]
        public class FloatEvent
        {
            public float value;
            public UltEventBase onEvent;
        }

        public FloatEvent floatEvent;

        [Table]
        public List<FloatEvent> FloatEvents;
    }
}

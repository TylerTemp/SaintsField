using System;
using SaintsField.Samples.Scripts.SaintsEditor;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Testing
{
    public class LeftToggleH : SaintsMonoBehaviour
    {
        [SaintsDictionary(numberOfItemsPerPage: 1), FieldDefaultExpand]
        public SaintsDictionary<bool, bool> toggleDict;

        [Serializable]
        public struct ToggleTable
        {
            public bool b1;
            public bool b2;
            public bool b3;
        }

        [Table] public ToggleTable[] toggleTable;
    }
}

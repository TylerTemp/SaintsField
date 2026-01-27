using System;
using System.Collections.Generic;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class TestUpWalk : SaintsMonoBehaviour
    {
        [Serializable]
        public struct Down
        {
            [Dropdown("../../" + nameof(options))]
            public string stringV;
            [MenuDropdown("../../" + nameof(options))]
            public string oldDownV;
        }

        [Serializable]
        public struct MyStruct
        {
            [Dropdown("../" + nameof(options))]
            public string stringV;
            public Down down;
        }

        public List<string> options;

        public MyStruct myStruct;
    }
}

using System;
using System.Collections.Generic;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class TestUpWalk : SaintsMonoBehaviour
    {
        [Serializable]
        public struct Down
        {
            [TreeDropdown("../../" + nameof(options))]
            public string stringV;
            [Dropdown("../../" + nameof(options))]
            public string oldDownV;
        }

        [Serializable]
        public struct MyStruct
        {
            [TreeDropdown("../" + nameof(options))]
            public string stringV;
            public Down down;
        }

        public List<string> options;

        public MyStruct myStruct;
    }
}

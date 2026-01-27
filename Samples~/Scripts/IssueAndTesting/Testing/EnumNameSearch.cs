using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Testing
{
    public class EnumNameSearch : SaintsMonoBehaviour
    {
        [Serializable]
        public enum MyEnum
        {
            [InspectorName("第一")] First,
            [InspectorName("第二")] Second,
            [InspectorName("第三")] Third,
        }

        [Dropdown] public MyEnum myEnum;
        public MyEnum myEnumAdv;
    }
}

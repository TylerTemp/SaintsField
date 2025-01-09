using System;
using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public class LayoutIndentBoxIssue : SaintsMonoBehaviour
    {
        [Serializable]
        public struct MyStruct
        {
            public string[] myStrings;
        }


        [LayoutStart("Test", ELayout.FoldoutBox)]
        public string myString;
        public string[] myStrings;
        public MyStruct myStruct;
    }
}

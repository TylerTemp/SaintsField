using System;
using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    [Searchable]
    public class SearchableMono : SaintsMonoBehaviour
    {
        public string myString;
        public int myInt;
        [ShowInInspector] private string MyInspectorString => "Non Ser Prop";
        [ShowInInspector] private string otherInspectorString = "Non Ser Field";
        public string otherString;
        public int otherInt;

        [ListDrawerSettings(searchable: true)]
        public string[] myArray;

        [Serializable]
        public struct MyStruct
        {
            public string MyStructString;
        }

        [Table] public MyStruct[] myTable;
    }
}

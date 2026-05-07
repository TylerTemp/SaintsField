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
#pragma warning disable CS0414 // Field is assigned but its value is never used
        // ReSharper disable once InconsistentNaming
        [ShowInInspector] private string otherInspectorString = "Non Ser Field";
#pragma warning restore CS0414 // Field is assigned but its value is never used
        public string otherString;
        public int otherInt;

        [ListDrawerSettings(searchable: true)]
        public string[] myArray;

        [Serializable]
        public struct MyStruct
        {
            // ReSharper disable once InconsistentNaming
            // ReSharper disable once UnusedMember.Global
            public string MyStructString;
        }

        [Table] public MyStruct[] myTable;
    }
}

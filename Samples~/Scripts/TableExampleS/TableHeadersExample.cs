using System;
using SaintsField.Playa;
using SaintsField.Samples.Scripts.SaintsEditor;

namespace SaintsField.Samples.Scripts.TableExampleS
{
    public class TableHeadersExample : SaintsMonoBehaviour
    {
        [Serializable]
        public struct TableHeaderStruct
        {
            public int i1;
            [TableColumn("Custom Header")] public int i2;
            [TableColumn("Custom Header")] [Button] private void D() {}
            public string str1;
            public string str2;

            [TableColumn("String")] public string str3;
            [TableColumn("String")] public string str4;
        }

        [Table] public TableHeaderStruct[] tableStruct;
    }
}

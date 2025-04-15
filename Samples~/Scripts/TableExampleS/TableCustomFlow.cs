using System;
using SaintsField.Playa;
using SaintsField.Samples.Scripts.SaintsEditor;

namespace SaintsField.Samples.Scripts.TableExampleS
{
    public class TableCustomFlow: SaintsMonoBehaviour
    {

        [Serializable]
        public struct MyKeyStruct
        {
            public string key;

            [PlayaSeparator("Key List")]
            public string[] ks;
        }

        [Serializable]
        public struct Struct1
        {
            public string s1;
        }

        [Serializable]
        public struct Struct2
        {
            public string s2;
        }

        [Serializable]
        public struct MyValueStruct
        {
            public MyKeyStruct myKeyStruct;

            [TableColumn("Structs")]
            public Struct1 struct1;
            [TableColumn("Structs")]
            public Struct2 struct2;

            [TableColumn("Buttons")]
            [Button("Ok")]
            public void BtnOk() {}

            [TableColumn("Buttons")]
            [Button("Cancel")]
            public void BtnCancel() {}

            [ShowInInspector] private int _showI;
        }

        [Table, DefaultExpand]
        public MyValueStruct[] myStructs;

        [Table, DefaultExpand]
        // [GetScriptableObject]
        public Scriptable[] scriptableArray;
    }
}

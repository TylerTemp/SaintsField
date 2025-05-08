using System;
using System.Collections.Generic;
using SaintsField.Playa;
using SaintsField.Samples.Scripts.SaintsEditor;
using UnityEngine;

namespace SaintsField.Samples.Scripts.TableExampleS
{
    public class TableExampleGeneral : SaintsMonoBehaviour
    {
        [Serializable]
        public struct SubStruct
        {
            public string sub;
        }

        [Serializable]
        public struct MyStruct
        {
            public int myInt;

            [TableColumn("Value")]
            public string myString;
            [TableColumn("Value")]
            public ScriptableObject myObject;
            [TableColumn("Value")]
            public SubStruct subStruct;

            [TableHide] public int hideMeInTable;

            [TableColumn("HideGroup"), TableHide]
            public int hideMeGroup1;

            [TableColumn("HideGroup")] [ShowInInspector]
            private const int HideMeGroup2 = 2;

            [TableColumn("")]
            [Button] private void B1() {}
            [TableColumn("")]
            [Button] private void B2() {}
        }

        [Table]
        public List<MyStruct> myStructs;

        [Button]
        public void Add()
        {
            myStructs.Add(new MyStruct
            {
                myInt = UnityEngine.Random.Range(0, 100),
            });
        }

        private void SizeChanged(List<MyStruct> newLis) => Debug.Log(newLis.Count);

        [ArrayDefaultExpand, Table] public MyStruct[] defaultExpanded2;
        [ArrayDefaultExpand, Table(hideAddButton: true, hideRemoveButton: true)] public MyStruct[] hideButtons;
    }
}

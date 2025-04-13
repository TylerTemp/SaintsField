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
        public struct MyStruct
        {
            public int myInt;

            [TableColumn("Value"), AboveRichLabel]
            public string myString;
            [TableColumn("Value"), AboveRichLabel]
            public GameObject myObject;
        }

        [Table]
        [OnArraySizeChanged(nameof(SizeChanged))]
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

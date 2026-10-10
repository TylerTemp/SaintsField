using System;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public class TablePaging : SaintsMonoBehaviour
    {
        [Serializable]
        public struct MyStruct
        {
            [PropRange(0, 5)]
            public int myInt;
            public string myString;
        }

        [Table(numberOfItemsPerPage: 5)] public MyStruct[] numbers;

        [ShowInInspector, Table(searchable: true, numberOfItemsPerPage: 3)]
        private MyStruct[] ShowNumbers
        {
            get => numbers;
            set => numbers = value;
        }

        [Button, Table]
        private MyStruct[] LogNumbers([Table(searchable: true, numberOfItemsPerPage: 3)] MyStruct[] rows)
        {
            Debug.Log(rows == null ? "No rows" : $"Rows: {rows.Length}");
            return rows;
        }
    }
}

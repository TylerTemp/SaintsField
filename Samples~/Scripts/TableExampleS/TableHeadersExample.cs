using System;
using System.Collections.Generic;
using SaintsField.Playa;
using SaintsField.Samples.Scripts.SaintsEditor;
using UnityEngine;

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

            public string str5;
            public string str6;
        }

        [Table]
        [TableHeaders(
            nameof(TableHeaderStruct.i1),  // directly name
            "Custom Header",  // directly custom name
            "$" + nameof(showTableHeader),  // callback of a single name
            "$" + nameof(ShowTableHeaders))  // callback of mutiple names
        ]
        public TableHeaderStruct[] tableStruct;

        [Table]
        [TableHeadersHide(
                nameof(TableHeaderStruct.i1),  // directly name
                "Custom Header",  // directly custom name
                "$" + nameof(showTableHeader),  // callback of a single name
                "$" + nameof(ShowTableHeaders))  // callback of mutiple names
        ]
        public TableHeaderStruct[] tableHideStruct;

        [Space]
        public string showTableHeader = nameof(TableHeaderStruct.str2);

        protected virtual IEnumerable<string> ShowTableHeaders() => new[]
        {
            nameof(TableHeaderStruct.str5),
            nameof(TableHeaderStruct.str6),
        };
    }
}

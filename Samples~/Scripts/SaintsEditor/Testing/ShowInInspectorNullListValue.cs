using System;
using System.Collections.Generic;
using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class ShowInInspectorNullListValue : SaintsMonoBehaviour
    {
        [Serializable]
        public struct MyStruct
        {
            public string name;
        }

        private static List<MyStruct> sharedList = new List<MyStruct>
        {
            new MyStruct
            {
                name = "N1",
            },
            new MyStruct
            {
                name = "N2",
            },
            new MyStruct
            {
                name = "N3",
            },
            new MyStruct
            {
                name = "N4",
            },
            new MyStruct
            {
                name = "N5",
            },
            new MyStruct
            {
                name = "N6",
            },
            new MyStruct
            {
                name = "N7",
            },
        };

        [ShowInInspector]
        private List<MyStruct> Nothing
        {
            get => sharedList;
            set => sharedList = value;
        }

        [ShowInInspector, ListDrawerSettings]
        private List<MyStruct> SearchOnly
        {
            get => sharedList;
            set => sharedList = value;
        }

        [ShowInInspector, ListDrawerSettings(searchable: false, numberOfItemsPerPage: 5)]
        private List<MyStruct> PageOnly
        {
            get => sharedList;
            set => sharedList = value;
        }

        [ShowInInspector, ListDrawerSettings(numberOfItemsPerPage: 5)]
        private List<MyStruct> FullFeatures
        {
            get => sharedList;
            set => sharedList = value;
        }

        [ShowInInspector, ListDrawerSettings(extraSearch: nameof(ExtraSearch))]
        private List<MyStruct> CustomSearch
        {
            get => sharedList;
            set => sharedList = value;
        }

        // Just a test: always includes N1
        private bool ExtraSearch(MyStruct item, IReadOnlyList<ListSearchToken> searchToken)
        {
            if (item.name == "N1")
            {
                return true;
            }

            return false;
        }
    }
}

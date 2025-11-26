using System.Collections.Generic;
using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public class ShowInInspectorDictionary : SaintsMonoBehaviour
    {
        private static Dictionary<int, string> _plainDict = new Dictionary<int, string>
        {
            {1, "A1"},
            {2, "B1"},
            {3, "C3"},
            {4, "D4"},
            {5, "E5"},
            {6, "F6"},
            {7, "G7"},
            {8, "H8"},
            {9, "I9"},
            {10, "J10"},
            {11, "K11"},
            {12, "L12"},
            {13, "M13"},
        };

        [SaintsDictionary(numberOfItemsPerPage: 5)]
        public SaintsDictionary<int, string> _saintsDict = (SaintsDictionary<int, string>)_plainDict;

        [ShowInInspector]
        private Dictionary<int, string> Plain
        {
            get => _plainDict;
            set => _plainDict = value;
        }

        [ShowInInspector, SaintsDictionary]
        private Dictionary<int, string> SearchOnly
        {
            get => _plainDict;
            set => _plainDict = value;
        }

        [ShowInInspector, SaintsDictionary(searchable: false, numberOfItemsPerPage: 5)]
        private Dictionary<int, string> PagingOnly
        {
            get => _plainDict;
            set => _plainDict = value;
        }

        [ShowInInspector, SaintsDictionary(numberOfItemsPerPage: 5)]
        private Dictionary<int, string> FullFeature
        {
            get => _plainDict;
            set => _plainDict = value;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Playa;
using UnityEngine;
using UnityEngine.Serialization;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class ListDrawRefactory : SaintsMonoBehaviour
    {
        [LayoutStart("Base", ELayout.TitleBox)]
        public int[] plainArray = { 10, 20, 30, 40, 50 };
        public List<string> plainList = new List<string> { "alpha", "beta", "gamma" };
        public int[] emptyArray = Array.Empty<int>();

        [Range(0, 10)]
        public int[] rangedArray = { 2, 5, 8 };

        public GameObject[] objectReferences = Array.Empty<GameObject>();

        [LayoutStart("Conf", ELayout.TitleBox)]
        [ListDrawerSettings(numberOfItemsPerPage: 2, extraSearch: nameof(MatchLast))]
        public Entry[] pagedEntries =
        {
            new Entry { id = "item-10", number = 10, tags = new[] { "red", "small" } },
            new Entry { id = "item-20", number = 20, tags = new[] { "blue" } },
            new Entry { id = "item-30", number = 30, tags = new[] { "green" } },
            new Entry { id = "item-40", number = 40, tags = Array.Empty<string>() },
            new Entry { id = "item-50", number = 50, tags = new[] { "last-row" } },
        };

        [Serializable]
        public class Entry
        {
            public string id;
            public int number;
            public string[] tags;
        }

        private bool MatchLast(Entry value, int index, IEnumerable<ListSearchToken> tokens)
            => value != null && index == pagedEntries.Length - 1 && tokens.Any(token => token.Token == "last");

        [LayoutEnd, LayoutStart("ArraySize", ELayout.TitleBox)]
        [ArraySize(min: 1, max: 3)]
        public int[] limitedFallback = { 11, 22 };

        [ListDrawerSettings, ArraySize(min: 1, max: 3)]
        public int[] limitedExplicit = { 11, 22 };

        [ListDrawerSettings(searchable: false), ArraySize(0)]
        public int[] fixedEmpty = Array.Empty<int>();

        [FormerlySerializedAs("references")]
        [LayoutEnd, LayoutStart("Managed reference", ELayout.TitleBox)]
        [SerializeReference]
        public ReferenceItem[] referenceItems = { new ReferenceItem { name = "original", number = 9 } };

        [Serializable]
        public class ReferenceItem
        {
            public string name;
            public int number;
        }

        [LayoutEnd, LayoutStart("Cell", ELayout.TitleBox)]
        public SaintsArray<int[]> arrayCells = new SaintsArray<int[]>(new[] { new[] { 101, 102 }, new[] { 201, 202 } });
        public SaintsList<List<int>> listCells = new SaintsList<List<int>>(
            new[] { new List<int> { 301, 302 }, new List<int> { 401, 402 } });
        public SaintsDictionary<string, int[]> dictionaryCells = new SaintsDictionary<string, int[]>(
            new Dictionary<string, int[]> { { "first", new[] { 501, 502 } }, { "second", new[] { 601, 602 } } });
        public SaintsArray2DR<int[]> gridCells = new SaintsArray2DR<int[]>(new int[1, 2][]
        {
            { new[] { 701, 702 }, new[] { 801, 802 } },
        });

        [LayoutEnd]
        [Button("External Add")]
        private void AppendFromCode()
        {
#if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(this, "Append list refactor sample item");
#endif
            plainArray = plainArray.Concat(new[] { plainArray.Length == 0 ? 10 : plainArray.Max() + 10 }).ToArray();
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}

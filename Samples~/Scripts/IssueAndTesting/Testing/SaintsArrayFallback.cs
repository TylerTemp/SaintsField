using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Testing
{
    public class SaintsArrayFallback : SaintsMonoBehaviour
    {
        public int[] plainArray = { 10, 20, 30, 40, 50 };
        public List<string> plainList = new List<string> { "one", "two" };

        [Range(0, 10)]
        public int[] rangedArray = { 2 };

        [ListDrawerSettings(extraSearch: nameof(MatchAlias))]
        public string[] aliases = { "first", "second" };

        private bool MatchAlias(string value, int index, IEnumerable<ListSearchToken> tokens)
            => value == "second" && index == 1 && tokens.Any(token => token.Token == "last");

        [ListDrawerSettings(numberOfItemsPerPage: 2)]
        public int[] pagedArray = { 10, 20, 30, 40, 50 };

        [ListDrawerSettings, ArraySize(min: 1, max: 3)]
        public int[] limitedArray = { 1, 2 };

        public SaintsArray<int[]> arrayCells = new SaintsArray<int[]>(new[] { new[] { 1, 2 } });
        public SaintsList<List<int>> listCells = new SaintsList<List<int>>(new[] { new List<int> { 3, 4 } });
        public SaintsDictionary<string, int[]> dictionaryCells = new SaintsDictionary<string, int[]>(
            new Dictionary<string, int[]> { { "row", new[] { 5, 6 } } });
        public SaintsArray2DR<int[]> gridCells = new SaintsArray2DR<int[]>(new int[1, 1][] { { new[] { 7, 8 } } });

        [Serializable]
        public class ReferenceItem
        {
            public int number;
        }

        [SerializeReference]
        public ReferenceItem[] references = { new ReferenceItem { number = 9 } };
    }
}

using System;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Issues.I250
{
    public class Issue250 : SaintsMonoBehaviour
    {
        [Serializable]
        public class SearchListItem
        {
            [SerializeField, Expandable] private SearchObj1 _searchObj;
        }

        [ListDrawerSettings(searchable: true), RichLabel("<field._searchObj.name /> <color=gray><index=[{0:D3}]/>")] public SearchListItem[] loopedRefs;

    }
}

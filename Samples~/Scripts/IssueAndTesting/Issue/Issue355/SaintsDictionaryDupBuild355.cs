using System;
using System.Collections.Generic;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue355
{
    [Serializable]
    public enum Rarity
    {
        Common,
        Rare,
        Epic,
        Legendary,
    }

    public class SaintsDictionaryDupBuild355 : SaintsMonoBehaviour
    {
        public SaintsDictionary<Rarity, GameObject> _rarities;

        [ShowInInspector] private IDictionary<Rarity, GameObject> d => _rarities;
    }
}

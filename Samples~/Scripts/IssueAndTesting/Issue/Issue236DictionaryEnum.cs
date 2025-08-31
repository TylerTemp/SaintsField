using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue236DictionaryEnum : MonoBehaviour
    {
        [Serializable]
        public enum CollectableType
        {
            None = 0,
            Missile = 1,
            Coin = 2,
            Shield = 3,
        }

        public SaintsDictionary<CollectableType, Color> colors;
    }
}

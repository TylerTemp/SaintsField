using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Issues
{
    public class ChipInputExample : SaintsMonoBehaviour
    {
        [Serializable]
        public enum Equipment
        {
            Special,

            [InspectorName("Weapon/Sword")]
            Sword,

            [InspectorName("Weapon/Bow")]
            Bow,

            [InspectorName("Armor/Shield")]
            Shield,
        }

        public Equipment[] defaultList;

        [Separator(20)]

        [Chips] public Equipment[] normalList;
        // duplicated options are disabled
        [Chips(EUnique.Disable)] public Equipment[] disableList;
        // duplicated options are removed
        [Chips(EUnique.Remove)] public Equipment[] removeList;

        [Chips(nameof(GetTags), slashAsSub = false)]  // callback
        public List<string> tags;

        private IEnumerable<string> GetTags() => new[]
        {
            "Player",
            "Enemy/Boss",
            "Environment",
        };

        [Chips(nameof(GetEnvAsync))]  // async
        public List<int> envLayer;

        private IEnumerator GetEnvAsync()
        {
            yield return new WaitForSeconds(2);

            yield return new Dropdown<int>()
            {
                { "Basic", 1 },
                { "Advanced/MiniBoss", 2 },
                { "Advanced/Boss", 2 },
                { "Final/Boss", 3 },
            };
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using SaintsField.Playa;
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

        [LayoutStart("x", ELayout.CollapseBox)]

        [ListDrawerSettings(numberOfItemsPerPage: 2)]
        public Equipment[] defaultList;

        // [Separator(20)]

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

        [Separator(20)]

        // [AboveText("This is a long driver for people have nothing to think about")]
        [AboveText("<label/>")]
        [LabelText(null)]
        [Chips(nameof(GetLongList))]
        public List<string> longList;

        private IEnumerable<string> GetLongList()
        {
            for (int index = 1; index <= 100; index++)
            {
                yield return $"Long List Item {index:000}";
            }
        }

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

        [Chips(nameof(GetGo))]
        public List<Component> dragNDrop;

        private IEnumerable<Component> GetGo()
        {
            return GetComponentsInChildren<Component>(true);
        }

        [LayoutEnd]

        // icon
        [Chips(nameof(GetIcons))]
        public List<int> withIcons;

        private Dropdown<int> GetIcons()
        {
            return new Dropdown<int>
            {
                { "Search", 1, false, "search.png", Color.brown },
                { "Play", 2, true, "play.png" },
                { "Star", 3, false, "star.png", EColor.Gold.GetColor() },
                { "Pencil", 4, false, "pencil.png" },
            };
        }
    }
}

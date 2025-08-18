using System;
using System.Collections.Generic;
using SaintsField.Playa;
using SaintsField.Utils;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public class ListDrawerSettingsCustomSearch : SaintsMonoBehaviour
    {
        [Serializable]
        public enum WeaponType
        {
            Sword,
            Arch,
            Hammer,
        }

        [Serializable]
        public struct Weapon
        {
            public WeaponType weaponType;
            public string description;
        }

        private bool ExtraSearch(Weapon weapon, int _, IReadOnlyList<ListSearchToken> tokens)
        {
            string searchName = new Dictionary<WeaponType, string>
            {
                { WeaponType.Arch , "弓箭 双手" },
                { WeaponType.Sword , "刀剑 单手" },
                { WeaponType.Hammer, "大锤 双手" },
            }[weapon.weaponType];
            return RuntimeUtil.SimpleSearch(searchName, tokens);
        }

        [ListDrawerSettings(extraSearch: nameof(ExtraSearch))]
        public Weapon[] weapons;
    }
}

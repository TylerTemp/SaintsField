using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Issues
{
// #if SAINTSFIELD_DEBUG
//     [CreateAssetMenu]
// #endif
    public class Issue316Order : SaintsScriptableObject
    {
        public string name;
        public Sprite unitSprite;
        public string description;
        public int unitType;
        public int brainType;
        public short maxHealth;

        [ShowIf(nameof(brainType), 1)] public int crowdCombatData;
    }
}

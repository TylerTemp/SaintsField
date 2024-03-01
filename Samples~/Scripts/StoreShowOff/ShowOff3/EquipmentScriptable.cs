using System;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.StoreShowOff.ShowOff3
{
    [CreateAssetMenu(fileName = "Equipment", menuName = "ScriptableObjects/Equipment", order = 0)]
    public class EquipmentScriptable : ScriptableObject
    {
        [Serializable]
        public enum ElementType
        {
            Fire = 1,
            Water = 1 << 1,
            Earth = 1 << 2,
            Wind = 1 << 3,
        }

        [Layout("Basic", ELayout.Background | ELayout.Title | ELayout.TitleOut)]
        public string name;
        [Layout("Basic")]
        [Required]
        public Sprite icon;
        [Layout("Basic")]
        [Required, BelowImage(nameof(icon), maxHeight: 40, groupBy: "image"), AssetPreview(groupBy: "image", height: 40)]
        public GameObject modelPrefab;

        [Layout("Detail", ELayout.Background | ELayout.Tab | ELayout.Title | ELayout.TitleOut)]
        [Layout("Detail/Skill")]
        public string skillName;
        [Layout("Detail/Skill")]
        [SepTitle("Skill Details", EColor.Gray)]
        [EnumFlags]
        public ElementType elementType;
        [Layout("Detail/Skill")]
        public ParticleSystem effect;

        [Layout("Detail/Store Info")]
        [ResizableTextArea]
        public string description;

        [Layout("Detail/Store Info")]
        [MinValue(0), MaxValue(1000), OverlayRichLabel("<color=yellow><icon=Assets/SaintsField/Samples/Scripts/StoreShowOff/Images/coin.png />")]
        public int price;
        [Layout("Detail/Store Info")]
        [InfoBox("Can only be obtained on battle", show: nameof(canBuy))]
        [LeftToggle]
        public bool canBuy;
    }
}

using System;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class LayoutUIToolkitStyle : SaintsMonoBehaviour
    {
        [Serializable]
        public enum SkillName
        {
            Skill1,
            Skill2,
            Skill3,
        }


        [LayoutStart("LabelFieldSimple", ELayout.LabelField)]
        [InfoBox("BOX!", EMessageType.None)]
        [NoLabel] public int myI;

        [Separator(20)]

        [LayoutStart("LabelFieldToggle", ELayout.LabelField)]
        [LeftToggle] public bool enableMe;
        [NoLabel, EnableIf(nameof(enableMe))] public int enableInt;

        [Separator(20)]

        [LayoutStart("LabelSkill", ELayout.LabelField)]
        [LayoutStart("./MyLabels")]  // we create a new sub layout to make multiple fields as "label"
        [LeftToggle] public bool hasSkill;
        [ShowIf(nameof(hasSkill)), NoLabel] public SkillName skill;

        [LayoutStart("..")]  // close the "label" layout here
        [ProgressBar(0, 100), NoLabel, EnableIf(nameof(hasSkill))] public int skillMp;
        [PropRange(0, 100), EnableIf(nameof(hasSkill))] public float skillDamage;

        // We can create more layout
        [LayoutShowIf(nameof(skill), SkillName.Skill2, nameof(hasSkill), true)]
        [LayoutStart("./My Other Fields", ELayout.Horizontal)]
        [OptionsDropdown("A", "B", "C")]
        public string level;
        public string desc;



    }
}

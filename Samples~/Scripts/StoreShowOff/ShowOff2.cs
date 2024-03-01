using System;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Samples.Scripts.StoreShowOff
{
    public class ShowOff2 : MonoBehaviour
    {
        public abstract class Role
        {
            public int roleId;
            [BelowImage(maxWidth: 32, align: EAlign.End)] public Sprite icon;
        }

        public class Thief : Role
        {
            public string leftSword;
            public string rightSword;
        }

        public class Knight : Role
        {
            public string sword;
            public string shield;
        }

        [Serializable]
        public struct Character
        {
            public string name;
            [Rate(1, 5), RichLabel(null)] public int stars;
            [SerializeReference, ReferencePicker]
            public Role role;

            [ProgressBar(0, 100, step: 1, color: EColor.Brown)]
            public int hp;

            [MinMaxSlider(0, 100, step: 0.2f)] public Vector2 mpSkillRange;

#if UNITY_EDITOR
            [RichLabel(null)]
            [ProgressBar(0, 100, step: 0.2f, colorCallback: nameof(MpColor), titleCallback: nameof(MpTitle))]
#endif
            public float mp;

#if UNITY_EDITOR
            private Color MpColor()
            {
                if (mp < mpSkillRange.x)
                {
                    return EColor.Brown.GetColor();
                }

                if (mp < mpSkillRange.y)
                {
                    return EColor.OceanicSlate.GetColor();
                }

                ColorUtility.TryParseHtmlString("#ad8100", out Color color);
                return color;
            }

            private string MpTitle(float curValue, float min, float max, string label)
            {
                if (mp < mpSkillRange.x)
                {
                    return $"MP [Low]: {curValue/max:P1}";
                }

                if (mp < mpSkillRange.y)
                {
                    return $"MP [Mid]: {curValue/max:P1}";
                }

                return $"MP [Hig]: {curValue/max:P1}";
            }
#endif
        }

        [RichLabel(nameof(LabelCharacterName), isCallback: true)]
        public Character[] characters;

        public string LabelCharacterName(int index)
        {
            Character character = characters[index];
            EColor color = EColor.White;
            switch (character.role)
            {
                case Thief _:
                    color = EColor.Lime;
                    break;
                case Knight _:
                    color = EColor.Brown;
                    break;
            }

            return $"<color={color}><icon={AssetDatabase.GetAssetPath(character.role.icon)}/></color>{character.name}";
        }
    }
}

using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public class LayoutDoc2 : SaintsMonoBehaviour
    {
        [LayoutStart("Left Hand", ELayout.FoldoutBox)]
        public GameObject leftEquipment;
        public int leftAttack;
        [Button]
        public void SetLeftHand() {}

        [LayoutStart("Right Hand", ELayout.FoldoutBox)]
        public GameObject rightEquipment;
        public int rightAttack;
        [Button]
        public void SetRightHand() {}

        [LayoutEnd]
        public int hp;
        public int mp;

    }
}

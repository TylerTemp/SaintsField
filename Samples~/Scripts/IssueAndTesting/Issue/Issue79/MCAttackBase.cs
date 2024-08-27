using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue79
{
    // [DisallowMultipleComponent]
    public abstract class MCAttackBase: MCDamageProvider
    {
        public bool noPanda;

        [LayoutGroup("ATTACK", ELayout.Title | ELayout.Background)]
        [SerializeField]
        [MinMaxSlider(0, 20f)]
        private Vector2 range = new Vector2(2, 10);

        public virtual Vector2 Range => range;
    }
}

using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Issues
{
    public class Issue312ShowIf : SaintsMonoBehaviour
    {
        public                                                     bool          boidCollision;
        [ShowIf(nameof(boidCollision))] public                     Transform     boidCollisionCenter;
        [ShowIf(nameof(boidCollision))] public                     float         projectileRadiusForBoid     = .5f;
    }
}

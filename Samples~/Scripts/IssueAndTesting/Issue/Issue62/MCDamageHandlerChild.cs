using SaintsField.Playa;
using SaintsField.Samples.Scripts.IssueAndTesting.Issue46;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue62
{
    public class MCDamageHandlerChild : MCDamageHandler
    {
        public override MCDamageHandler realSelf => parent;
        public override bool            isRelay  => true;
        // [LayoutGroup(" ------ CHILD -----",ELayout.Title|ELayout.Background),GetComponentInParents(excludeSelf: true),HideIf(nameof(_parentOverride))]
        // [HideIf(nameof(_parentOverride))]
        [LayoutGroup(" ------ CHILD -----",ELayout.Title|ELayout.Background)]
        [GetComponentInParents(excludeSelf: true)]
        [HideIf(nameof(_parentOverride))]
        public MCDamageHandler _parent;
        // [Comment("set this to override the auto parent")]
        public MCDamageHandler _parentOverride;
        public          MCDamageHandler parent => _parentOverride ? _parentOverride : _parent;
        public override MCTeam          team   => parent?.team;
        // 0 = totally immune to damage
        public float damageRatio = 1;

        // public override float HandleDamageProvider(HVRDamageProvider damageProvider,float damage,Vector3 hitPoint,Vector3 direction)
        // {
        //     return parent.HandleDamageProvider(damageProvider,damage,hitPoint,direction);
        // }
        //
        // public override void HandleRayCastHit(HVRDamageProvider damageProvider,RaycastHit hit)
        // {
        //     parent.HandleRayCastHit(damageProvider,hit);
        // }
    }
}

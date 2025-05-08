using SaintsField.Spine;
using Spine.Unity;
using UnityEngine;

namespace SaintsField.Samples.Spine.Scripts.Issue
{
    public class Issue230 : MonoBehaviour
    {
        public GameObject VisualsPrefab = null;

        [SpineAnimationPicker(skeletonTarget: nameof(VisualsPrefab))]
        public string SpineAnimationNameIndirect;

        [AnimatorState(animator: nameof(VisualsPrefab))]
        public string AnimatorStateNameIndirect;

        [SpineAnimationPicker(skeletonTarget: nameof(VisualsPrefabSpineSkeletonAnimationDirect))]
        public string SpineAnimationNameDirect;

        [AnimatorState(animator: nameof(VisualsPrefabAnimatorDirect))]
        public string AnimatorStateNameDirect;

        private SkeletonAnimation VisualsPrefabSpineSkeletonAnimationDirect => VisualsPrefab != null && VisualsPrefab.TryGetComponent<SkeletonAnimation>(out var a) ? a : null;

        private Animator VisualsPrefabAnimatorDirect => VisualsPrefab != null && VisualsPrefab.TryGetComponent<Animator>(out var a) ? a : null;
    }
}

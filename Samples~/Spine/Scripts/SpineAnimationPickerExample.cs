using SaintsField.Spine;
using Spine.Unity;
using UnityEngine;

namespace SaintsField.Samples.Spine.Scripts
{
    public class SpineAnimationPickerExample : MonoBehaviour
    {
        [SpineAnimationPicker] public string animationName;

        [GetComponent] public SkeletonAnimation spine;

        [SpineAnimationPicker(nameof(spine))] public AnimationReferenceAsset animationReference;
    }
}

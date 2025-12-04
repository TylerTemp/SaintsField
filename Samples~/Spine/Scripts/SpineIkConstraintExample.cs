using SaintsField.Spine;
using Spine.Unity;
using UnityEngine;

namespace SaintsField.Samples.Spine.Scripts
{
    public class SpineIkConstraintExample : MonoBehaviour
    {
        [SpineIkConstraint] public string spineIKConstraint;
        [SpineIkConstraintPicker] public string spineIKConstraintPicker;
    }
}

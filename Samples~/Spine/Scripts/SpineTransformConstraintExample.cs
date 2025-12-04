using SaintsField.Spine;
using Spine.Unity;
using UnityEngine;

namespace SaintsField.Samples.Spine.Scripts
{
    public class SpineTransformConstraintExample : MonoBehaviour
    {
        [SpineTransformConstraint] public string spineTransformConstraint;
        [SpineTransformConstraintPicker] public string spineTransformConstraintPicker;
    }
}

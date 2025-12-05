using SaintsField.Spine;
using Spine.Unity;
using UnityEngine;

namespace SaintsField.Samples.Spine.Scripts
{
    public class SpinePathConstraintExample : MonoBehaviour
    {
        [SpinePathConstraint] public string spinePackConstraint;
        [SpinePathConstraintPicker] public string spinePackConstraintPicker;
    }
}

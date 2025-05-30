using SaintsField.Samples.Scripts.Interface;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class SceneViewPickerExample : MonoBehaviour
    {
        [SceneViewPicker] public Collider myCollider;
        // works with SaintsInterface
        [SceneViewPicker] public SaintsObjInterface<IInterface1> interf;

        // a notice will diplay if no target is found
        [SceneViewPicker] public NoThisInScene noSuch;
        // works for list elements too
        [SceneViewPicker] public Object[] anything;
    }
}

using SaintsField.Samples.Scripts.Interface;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class SceneViewPickerExample : MonoBehaviour
    {
        [SceneViewPicker] public Transform trans;
        [SceneViewPicker] public SaintsObjInterface<IInterface1> interf;

        [SceneViewPicker] public NoThisInScene noSuch;
        [SceneViewPicker] public Object anything;

        [SceneViewPicker] public MonoBehaviour[] lis;
    }
}

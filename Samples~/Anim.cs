using ExtInspector.Standalone;
using UnityEngine;

namespace ExtInspectorUnity.Samples
{
    public class Anim : MonoBehaviour
    {
        [field: SerializeField] public Animator Animator { get; private set; }
        [field: SerializeField, AnimStateSelector(nameof(Animator))] public AnimState AnimState { get; private set; }
    }
}

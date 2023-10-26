using ExtInspector;
using ExtInspector.Standalone;
using UnityEngine;

namespace ExtInspectorUnity.Samples
{
    public class Anim : MonoBehaviour
    {
        [field: SerializeField] public Animator Animator { get; private set; }
        [field: SerializeField, AnimState(nameof(Animator)), RichLabel("<color=green><label/></color>")] public AnimState AnimState { get; private set; }
        [field: SerializeField, AnimState(nameof(Animator))] public string AnimStateName { get; private set; }
    }
}

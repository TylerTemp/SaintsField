using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class ParticlePlayExample : MonoBehaviour
    {
        [ParticlePlay] public ParticleSystem particle;
        // It also works if the field target has a particleSystem component
        [ParticlePlay, FieldType(typeof(ParticleSystem), false)] public GameObject particle2;
    }
}

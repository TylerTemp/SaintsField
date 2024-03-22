using UnityEngine;

namespace SaintsField.Samples.Scripts.FieldInterfaceExample
{
    public class FieldInterfaceMono: MonoBehaviour
    {
        [FieldInterface(typeof(IMyInterface))] public SpriteRenderer interSr;
    }
}

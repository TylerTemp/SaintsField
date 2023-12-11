using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class AddComponentExample: MonoBehaviour
    {
        [AddComponent(typeof(Dummy)), GetComponent] public Dummy dummy;
    }
}

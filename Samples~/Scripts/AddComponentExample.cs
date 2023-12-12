using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class AddComponentExample: MonoBehaviour
    {
        [AddComponent, GetComponent] public Dummy dummy;
        [AddComponent(typeof(BoxCollider)), GetComponent] public GameObject thisObj;
    }
}

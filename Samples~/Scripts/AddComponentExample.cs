using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class AddComponentExample: MonoBehaviour
    {
        [AddComponent, GetComponent] public Dummy myDummy;
        [AddComponent(typeof(BoxCollider)), GetComponent] public GameObject myObj;
        public string fallbackStyle;
    }
}

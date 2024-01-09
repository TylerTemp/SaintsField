using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class FindComponentExample: MonoBehaviour
    {
        [FindComponent("sub/dummy")] public Dummy myDummy;
        [FindComponent("sub/dummy")] public GameObject myDummyGo;
        [FindComponent("sub/noSuch", "sub/dummy")] public Transform myDummyTrans;
    }
}

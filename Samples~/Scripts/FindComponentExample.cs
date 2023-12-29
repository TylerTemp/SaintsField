using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class FindComponentExample: MonoBehaviour
    {
        [FindComponent("sub/dummy")] public Dummy subDummy;
        [FindComponent("sub/dummy")] public GameObject subDummyGo;
        [FindComponent("sub/noSuch", "sub/dummy")] public Transform subDummyTrans;
    }
}

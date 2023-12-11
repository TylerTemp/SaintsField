using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class GetComponentInSceneExample: MonoBehaviour
    {
        [GetComponentInScene] public Dummy dummy;
        [GetComponentInScene(compType: typeof(Dummy))] public GameObject dummyGo;
        [GetComponentInScene(compType: typeof(Dummy))] public Transform dummyTrans;
    }
}

using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class GetComponentInSceneExample: MonoBehaviour
    {
        [GetComponentInScene] public Dummy dummy;
        // by setting compType, you can sign it as a different type
        [GetComponentInScene(compType: typeof(Dummy))] public RectTransform dummyTrans;
        // and GameObject type works too
        [GetComponentInScene(compType: typeof(Dummy))] public GameObject dummyGo;
    }
}

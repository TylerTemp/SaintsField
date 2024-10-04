using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class GetComponentInSceneExample: MonoBehaviour
    {
        [GetComponentInScene] public Dummy myDummy;
        // by setting compType, you can sign it as a different type
        [GetComponentInScene(compType: typeof(Dummy))] public Transform myDummyTrans;
        // and GameObject type works too
        [GetComponentInScene(compType: typeof(Dummy))] public GameObject myDummyGo;

        [Separator("XPath")]
        [GetByXPath("scene:://*")] public Dummy myDummyXPath;
        [GetByXPath("scene:://*[@{GetComponent(Dummy)}]")] public Transform myDummyTransXPath;
        [GetByXPath("scene:://*[@{GetComponent(Dummy)}]")] public GameObject myDummyGoXPath;
    }
}

using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class GetComponentInSceneExample: MonoBehaviour
    {
        [FindObjectsByType] public Dummy myDummy;
        // by setting compType, you can sign it as a different type
        [FindObjectsByType(type: typeof(Dummy))] public Transform myDummyTrans;
        // and GameObject type works too
        [FindObjectsByType(type: typeof(Dummy))] public GameObject myDummyGo;

        [FieldSeparator("XPath")]
        [GetByXPath("scene:://*")] public Dummy myDummyXPath;
        [GetByXPath("scene:://*[@{GetComponent(Dummy)}]")] public Transform myDummyTransXPath;
        [GetByXPath("scene:://*[@{GetComponent(Dummy)}]")] public GameObject myDummyGoXPath;
    }
}

using UnityEngine;

namespace SaintsField.Samples.Scripts.GetByXPathExamples
{
    public class GetByXPathAssetExample : MonoBehaviour
    {
        [GetByXPath("asset::/SaintsField/Samples/RawResources/Toggle.mat")]
        public Material toggleMat;

        [GetByXPath("asset:://RawResources/*.png")]
        public Sprite[] searchPngs;

        [GetByXPath("resources:://*")]
        public GameObject[] resourcePrefabs;

        [GetByXPath(EXP.Picker,  "asset:://*.mat"), Required]
        public Material pickMat;
    }
}

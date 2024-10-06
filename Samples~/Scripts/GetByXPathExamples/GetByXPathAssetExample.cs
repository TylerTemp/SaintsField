using SaintsField.Samples.Scripts.SaintsEditor;
using UnityEngine;

namespace SaintsField.Samples.Scripts.GetByXPathExamples
{
    public class GetByXPathAssetExample : SaintsMonoBehaviour
    {
        [GetByXPath("asset::/SaintsField/Samples/RawResources/Toggle.mat")]
        public Material toggleMat;

        [GetByXPath("asset:://RawResources/*.png")]
        public Sprite[] searchPngs;

        [GetByXPath("resources:://*")]
        public GameObject[] resourcePrefabs;

        [GetByXPath(EXP.JustPicker,  "asset:://*.mat"), Required]
        public Material pickMat;

        [GetByXPath(EXP.Silent, "scene:://noSuchObject", "asset:://*.prefab")]
        [GetByXPath("scene:://Rate", "asset:://NoSuchItemInAsset")]
        public GameObject[] multiGetXPaths;
    }
}

using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class ResourcePathExample : MonoBehaviour
    {
        [ResourcePath(typeof(Dummy), typeof(BoxCollider))]
        [InfoBox(nameof(myResource), true)]
        public string myResource;

        [Space]
        [ResourcePath(EStr.AssetDatabase, typeof(Dummy), typeof(BoxCollider))]
        [InfoBox(nameof(myAssetPath), true)]
        public string myAssetPath;

        [Space]
        [ResourcePath(EStr.Guid, typeof(Dummy), typeof(BoxCollider))]
        [InfoBox(nameof(myGuid), true)]
        public string myGuid;
    }
}

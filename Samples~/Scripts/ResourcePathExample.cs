using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class ResourcePathExample : MonoBehaviour
    {
        [ResourcePath(EStr.Resource, false, true, typeof(Dummy), typeof(BoxCollider))]
        [BelowRichLabel(nameof(myResource), true)]
        public string myResource;

        [Space]
        [ResourcePath(EStr.AssetDatabase, false, true, typeof(Dummy), typeof(BoxCollider))]
        [BelowRichLabel(nameof(myAssetPath), true)]
        public string myAssetPath;

        [Space]
        [ResourcePath(EStr.Guid, false, true, typeof(Dummy), typeof(BoxCollider))]
        [BelowRichLabel(nameof(myGuid), true)]
        public string myGuid;
    }
}

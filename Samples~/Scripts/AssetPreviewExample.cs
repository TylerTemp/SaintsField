using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class AssetPreviewExample: MonoBehaviour
    {
        [AssetPreview] public GameObject go;
        [AssetPreview] public Dummy dummyCube;
        [AssetPreview] public GameObject fbx;
        [AssetPreview] public Material drawMaterial;

        [Space]
        [AssetPreview(20, 100)] public Texture2D drawTexture2D;
        [AssetPreview(above: true, align: EAlign.Center)] public Sprite sprite;
        [AssetPreview(above: true)] public Sprite[] sprites;

        [AssetPreview] public Sprite multi1;
        [AssetPreview] public Sprite multi2;

        // [AssetPreview] public Address
    }
}

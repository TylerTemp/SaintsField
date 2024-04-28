using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class AssetPreviewExample: MonoBehaviour
    {
        [AssetPreview(int.MaxValue, 500, align: EAlign.FieldStart)] public GameObject go;
        [AssetPreview(20, 100)] public Texture2D drawTexture2D;
        [AssetPreview(above: true, align: EAlign.Center)] public Sprite sprite;
        [AssetPreview(above: true)] public Sprite[] sprites;
    }
}

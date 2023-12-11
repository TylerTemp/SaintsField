using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class AssetPreviewExample: MonoBehaviour
    {
        [AssetPreview(20, 100)] public Texture2D _texture2D;
        [AssetPreview(50, align: EAlign.FieldStart)] public GameObject _go;
        [AssetPreview(above: true)] public Sprite _sprite;
        [AssetPreview(above: true)] public Sprite[] sprites;
    }
}

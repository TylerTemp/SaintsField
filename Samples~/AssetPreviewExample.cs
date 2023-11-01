using UnityEngine;

namespace SaintsField.Samples
{
    public class AssetPreviewExample: MonoBehaviour
    {
        [SerializeField, AssetPreview(20, 100)] private Texture2D _texture2D;
        [SerializeField, AssetPreview(50)] private GameObject _go;
        [SerializeField, AssetPreview(above: true)] private Sprite _sprite;
    }
}

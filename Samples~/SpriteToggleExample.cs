using UnityEngine;
using UnityEngine.UI;

namespace ExtInspector.Samples
{
    public class SpriteToggleExample : MonoBehaviour
    {
        [field: SerializeField] private Image _image;
        [field: SerializeField] private SpriteRenderer _sprite;

        [SerializeField
         , SpriteToggle(nameof(_image))
         , SpriteToggle(nameof(_sprite))
        ] private Sprite _sprite1;
        [SerializeField
         , SpriteToggle(nameof(_image))
         , SpriteToggle(nameof(_sprite))
        ] private Sprite _sprite2;
    }
}

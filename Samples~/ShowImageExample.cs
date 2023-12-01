using UnityEngine;

namespace SaintsField.Samples
{
    public class ShowImageExample: MonoBehaviour
    {
        [BelowImage(nameof(spriteField), maxWidth: 50)]
        [AboveImage(nameof(spriteField))]
        public Sprite spriteField;
    }
}

using UnityEngine;
using UnityEngine.UI;

namespace SaintsField.Samples
{
    public class ShowImageExample: MonoBehaviour
    {
        [AboveImage(nameof(spriteField))]
        // size and group
        [BelowImage(nameof(spriteField), maxWidth: 25, groupBy: "Below1")]
        [BelowImage(nameof(spriteField), maxHeight: 20, align: EAlign.End, groupBy: "Below1")]
        public Sprite spriteField;

        // align
        [BelowImage(nameof(spriteField), maxWidth: 20, align: EAlign.FieldStart)]
        [BelowImage(nameof(spriteField), maxWidth: 20, align: EAlign.Start)]
        [BelowImage(nameof(spriteField), maxWidth: 20, align: EAlign.Center)]
        [BelowImage(nameof(spriteField), maxWidth: 20, align: EAlign.End)]
        public string alignField;

        [Space]
        [BelowImage(nameof(rawImageField), maxWidth: 20, align: EAlign.End)]
        public RawImage rawImageField;
        [BelowImage(nameof(imageField), maxWidth: 20, align: EAlign.End)]
        public Image imageField;
        [BelowImage(nameof(buttonField), maxWidth: 20, align: EAlign.End)]
        public Button buttonField;
    }
}

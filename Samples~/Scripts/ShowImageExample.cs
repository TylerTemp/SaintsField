using UnityEngine;
using UnityEngine.UI;

namespace SaintsField.Samples.Scripts
{
    public class ShowImageExample: MonoBehaviour
    {
        [AboveImage]
        // size and group
        [BelowImage(maxWidth: 20, groupBy: "Below1")]
        [BelowImage(maxHeight: 20, align: EAlign.End, groupBy: "Below1")]
        public Sprite spriteField;

        // align
        [BelowImage(nameof(spriteField), maxWidth: 20, align: EAlign.FieldStart)]
        [BelowImage(nameof(spriteField), maxWidth: 20, align: EAlign.Start)]
        [BelowImage(nameof(spriteField), maxWidth: 20, align: EAlign.Center)]
        [BelowImage(nameof(spriteField), maxWidth: 20, align: EAlign.End)]
        public string alignField;

        [Space]
        [BelowImage(maxWidth: 20, align: EAlign.End)]
        public RawImage rawImageField;
        [BelowImage(maxWidth: 20, align: EAlign.End)]
        public Image imageField;
        [BelowImage(maxWidth: 20, align: EAlign.End)]
        public Button buttonField;

        [BelowImage] public RawImage emptyImage;
    }
}

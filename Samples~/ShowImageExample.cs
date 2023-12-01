using UnityEngine;

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
    }
}

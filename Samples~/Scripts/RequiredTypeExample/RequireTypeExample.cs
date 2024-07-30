using UnityEngine;

namespace SaintsField.Samples.Scripts.RequiredTypeExample
{
    public class RequireTypeExample: MonoBehaviour
    {
        [RequireType(typeof(IMyInterface))] public SpriteRenderer interSr;
        [RequireType(typeof(IMyInterface), typeof(SpriteRenderer))] public GameObject interfaceGo;

        [RequireType(true, typeof(IMyInterface))] public SpriteRenderer srNoPickerFreeSign;
        [RequireType(true, typeof(IMyInterface))] public GameObject goNoPickerFreeSign;

        // working with FieldType. You need to add the swapped type in RequireType too
        [RequireType(typeof(IMyInterface), typeof(SpriteRenderer)),
         FieldType(typeof(SpriteRenderer), false)] public GameObject fieldGo;

        [ReadOnly]
        [RequireType(typeof(IMyInterface), typeof(SpriteRenderer)),
         FieldType(typeof(SpriteRenderer), false)] public GameObject fieldGoDisable;

        [RequireType(typeof(IMyInterface))] public SpriteRenderer[] interSrs;
    }
}

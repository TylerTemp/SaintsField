using SaintsField;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public class GetInSiblingsExample : SaintsMonoBehaviour
    {
        [GetInSiblings] public SpriteRenderer sr;
        [GetInSiblings] public SpriteRenderer[] srArray;
    }
}

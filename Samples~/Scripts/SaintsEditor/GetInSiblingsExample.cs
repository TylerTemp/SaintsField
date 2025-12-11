using SaintsField;
using UnityEngine;

namespace Saintsfield.Samples.Scripts.SaintsEditor
{
    public class GetInSiblingsExample : SaintsMonoBehaviour
    {
        [GetInSiblings] public SpriteRenderer sr;
        [GetInSiblings] public SpriteRenderer[] srArray;
    }
}

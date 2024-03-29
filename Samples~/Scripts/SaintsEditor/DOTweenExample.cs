using UnityEngine;
using SaintsField.Playa;
#if SAINTSFIELD_DOTWEEN
using DG.Tweening;
#endif

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    // ReSharper disable once InconsistentNaming
    public class DOTweenExample : MonoBehaviour
    {
        [InfoBox("Note: Enable SaintsEditor & DOTween supports to see this example.", EMessageType.Info, above: true)]
        [GetComponent]
        public SpriteRenderer spriteRenderer;

        [Button("Tween under me")]
        private void Nothing1() {}

#if SAINTSFIELD_DOTWEEN
        [DOTweenPlay]
        private Sequence PlayColor()
        {
            return DOTween.Sequence()
                .Append(spriteRenderer.DOColor(Color.red, 1f))
                .Append(spriteRenderer.DOColor(Color.green, 1f))
                .Append(spriteRenderer.DOColor(Color.blue, 1f))
                .SetLoops(-1);
        }
#endif

        [Button("Tween above me")]
        private void Nothing2() {}

#if SAINTSFIELD_DOTWEEN
        [DOTweenPlay("Position")]
        private Sequence PlayTween2()
        {
            return DOTween.Sequence()
                .Append(spriteRenderer.transform.DOMove(Vector3.up, 1f))
                .Append(spriteRenderer.transform.DOMove(Vector3.right, 1f))
                .Append(spriteRenderer.transform.DOMove(Vector3.down, 1f))
                .Append(spriteRenderer.transform.DOMove(Vector3.left, 1f))
                .Append(spriteRenderer.transform.DOMove(Vector3.zero, 1f))
            ;
        }
#endif
    }
}

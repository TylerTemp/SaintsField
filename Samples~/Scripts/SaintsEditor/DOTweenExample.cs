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
        [GetComponent]
        public SpriteRenderer spriteRenderer;

#if SAINTSFIELD_DOTWEEN
        [Button("Tween under me")]
        private void Nothing1() {}

        [DOTweenPlay]
        private Sequence PlayTween()
        {
            return DOTween.Sequence()
                .Append(spriteRenderer.DOColor(Color.red, 1f))
                .Append(spriteRenderer.DOColor(Color.green, 1f))
                .Append(spriteRenderer.DOColor(Color.blue, 1f));
        }

        [Button("Tween above me")]
        private void Nothing2() {}

        [DOTweenPlay]
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

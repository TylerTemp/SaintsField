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


        [Button("Tween under me")]
        private void Nothing1() {}

#if SAINTSFIELD_DOTWEEN
        [DOTweenPlay]
        private Sequence PlayTween()
        {
            return DOTween.Sequence()
                .Append(spriteRenderer.DOColor(Color.red, 1f))
                .Append(spriteRenderer.DOColor(Color.green, 1f))
                .Append(spriteRenderer.DOColor(Color.blue, 1f));
        }
#endif

        [Button("Tween above me")]
        private void Nothing2() {}

#if SAINTSFIELD_DOTWEEN
        [DOTweenPlay]
        private Sequence PlayTween2()
        {
            return DOTween.Sequence()
                .Append(spriteRenderer.DOColor(Color.yellow, 1f));
        }
#endif
    }
}

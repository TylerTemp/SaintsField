using UnityEngine;
using SaintsField.Playa;
#if DOTWEEN && !SAINTSFIELD_DOTWEEN_DISABLED
using DG.Tweening;
#endif

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    // ReSharper disable once InconsistentNaming
    public class DOTweenExample : SaintsMonoBehaviour
    {
        [GetComponent]
        public SpriteRenderer spriteRenderer;

        [Button("Tween under me"), Ordered]
        private void Nothing1() {}

        [Layout("Color", ELayout.Foldout | ELayout.Background | ELayout.TitleOut), Ordered]
        [ShowInInspector]
        public const string Title = "This is Color Tween";

#if DOTWEEN && !SAINTSFIELD_DOTWEEN_DISABLED
        [DOTweenPlayGroup(groupBy: "Color"), Ordered]
        private Sequence PlayColor()
        {
            return DOTween.Sequence()
                .Append(spriteRenderer.DOColor(Color.red, 1f))
                .Append(spriteRenderer.DOColor(Color.green, 1f))
                .Append(spriteRenderer.DOColor(Color.blue, 1f))
                .SetLoops(-1);
        }

        [Ordered]
        private Sequence PlayColor2()
        {
            return DOTween.Sequence()
                .Append(spriteRenderer.DOColor(Color.cyan, 1f))
                .Append(spriteRenderer.DOColor(Color.magenta, 1f))
                .Append(spriteRenderer.DOColor(Color.yellow, 1f))
                .SetLoops(-1);
        }

        [Ordered]
        private Sequence PlayColor3()
        {
            return DOTween.Sequence()
                .Append(spriteRenderer.DOColor(Color.yellow, 1f))
                .Append(spriteRenderer.DOColor(Color.magenta, 1f))
                .Append(spriteRenderer.DOColor(Color.cyan, 1f))
                .SetLoops(-1);
        }

        [DOTweenPlayEnd("Color"), Ordered]
        public void DoNotIncludeMe() {}
#endif

        [Button("Tween above me"), Ordered]
        private void Nothing2() {}

#if DOTWEEN && !SAINTSFIELD_DOTWEEN_DISABLED
        [DOTweenPlay("Position"), Ordered]
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

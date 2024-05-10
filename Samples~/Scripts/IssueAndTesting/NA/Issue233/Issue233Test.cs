using System;
using SaintsField.Playa;
using UnityEngine;
#if DOTWEEN && !SAINTSFIELD_DOTWEEN_DISABLED
using DG.Tweening;
#endif


namespace SaintsField.Samples.Scripts.IssueAndTesting.NA.Issue233
{
    public class Issue233Test : MonoBehaviour
    {
        [Serializable]
        public struct Nest2
        {
            public string nest2Str;
            [Button]
            private void Nest2Btn() => Debug.Log("Call Nest2Btn");
            [ShowInInspector] public static Color StaticColor => Color.cyan;
            [ShowInInspector] public const float Pi = 3.14f;

            [BelowImage(maxWidth: 25)] public SpriteRenderer spriteRenderer;

#if DOTWEEN && !SAINTSFIELD_DOTWEEN_DISABLED
            [DOTweenPlay]
            private Sequence PlayColor()
            {
                return DOTween.Sequence()
                    .Append(spriteRenderer.DOColor(Color.red, 1f))
                    .Append(spriteRenderer.DOColor(Color.green, 1f))
                    .Append(spriteRenderer.DOColor(Color.blue, 1f))
                    .SetLoops(-1);
            }
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

        [Serializable]
        public struct Nest1
        {
            [Button, Ordered]
            private void Nest1Btn() => Debug.Log("Call Nest1Btn");

            [SaintsRow, Ordered]
            public Nest2 n2;
            [SaintsRow, Ordered]
            public Nest2[] n2Array;

            [field: SerializeField, Ordered]
            public int MyProperty { get; set; }

            [Ordered]
            public string myString;
        }

        [SaintsRow]
        public Nest1 n1;

        [SaintsRow]
        public Nest2 n;
    }
}

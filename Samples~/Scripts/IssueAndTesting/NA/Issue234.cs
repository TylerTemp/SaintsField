using System;
#if DOTWEEN && !SAINTSFIELD_DOTWEEN_DISABLED
using DG.Tweening;
#endif
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.NA
{
    public class Issue234 : MonoBehaviour
    {
        [Serializable]
        public struct MyStruct
        {
            public int structInt;
            public bool structBool;

            [Button]
            public void StructBtn()
            {
                Debug.Log("Call StructBtn");
            }

            [ShowInInspector] public static Color structStaticColor = Color.blue;

            public SpriteRenderer spriteRenderer;

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

        [SaintsRow(inline: true)]
        public MyStruct myStructInline;

        public string normalStringField;

        [SaintsRow]
        public MyStruct myStruct;
    }
}

using System.Collections;
using SaintsField.ComponentHeader;
using SaintsField.Playa;
using SaintsField.Samples.Scripts.SaintsEditor;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SaintsField.Samples.Scripts.HeaderComponentExample
{
    public class HeaderDrawExample : SaintsMonoBehaviour
    {
#if UNITY_EDITOR
        [HeaderButton("B")]
        private void BeforeBotton()
        {
            _started = false;
        }

        [HeaderDraw("group1")]
        private HeaderUsed HeaderDrawRight1G1(HeaderArea headerArea)
        {
            Rect useRect = new Rect(headerArea.MakeXWidthRect(headerArea.GroupStartX - 40, 40))
            {
                height = headerArea.Height / 3,
            };
            Rect progressRect = new Rect(useRect)
            {
                width = range1 * useRect.width,
            };

            EditorGUI.DrawRect(useRect, Color.gray);
            EditorGUI.DrawRect(progressRect, Color.red);

            return new HeaderUsed(useRect);
        }

        [HeaderDraw("group1")]
        private HeaderUsed HeaderDrawRight1G2(HeaderArea headerArea)
        {
            Rect useRect = new Rect(headerArea.MakeXWidthRect(headerArea.GroupStartX - 40, 40))
            {
                y = headerArea.Y + headerArea.Height / 3,
                height = headerArea.Height / 3,
            };
            Rect progressRect = new Rect(useRect)
            {
                width = range2 * useRect.width,
            };

            EditorGUI.DrawRect(useRect, Color.gray);
            EditorGUI.DrawRect(progressRect, Color.yellow);

            return new HeaderUsed(useRect);
        }

        [HeaderDraw("group1")]
        private HeaderUsed HeaderDrawRight1G3(HeaderArea headerArea)
        {
            Rect useRect = new Rect(headerArea.MakeXWidthRect(headerArea.GroupStartX - 40, 40))
            {
                y = headerArea.Y + headerArea.Height / 3 * 2,
                height = headerArea.Height / 3,
            };
            Rect progressRect = new Rect(useRect)
            {
                width = range3 * useRect.width,
            };

            EditorGUI.DrawRect(useRect, Color.gray);
            EditorGUI.DrawRect(progressRect, Color.cyan);

            return new HeaderUsed(useRect);
        }

        [HeaderButton("A")]
        private void AfterBotton()
        {
            _started = false;
        }
#endif

        [Range(0f, 1f)] public float range1;
        [Range(0f, 1f)] public float range2;
        [Range(0f, 1f)] public float range3;

        private bool _started;

        [Button]
        private IEnumerator Dance()
        {
            _started = true;
            while (_started)
            {
                range1 = (range1 + 0.01f) % 1;
                range2 = (range2 + 0.03f) % 1;
                range3 = (range3 + 0.02f) % 1;
                yield return null;
            }
            // ReSharper disable once IteratorNeverReturns
        }
    }
}

using System.Collections;
using SaintsField.ComponentHeader;
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
        private bool _started;

        [HeaderGhostButton("<icon=play.png/>")]
        private IEnumerator BeforeBotton()
        {
            _started = true;
            while (_started)
            {
                range1 = (range1 + 0.0005f) % 1;
                range2 = (range2 + 0.0009f) % 1;
                range3 = (range3 + 0.0007f) % 1;
                yield return null;
            }
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

        [HeaderGhostButton("<icon=pause.png/>")]
        private void AfterBotton()
        {
            _started = false;
        }
#endif

        [Range(0f, 1f)] public float range1;
        [Range(0f, 1f)] public float range2;
        [Range(0f, 1f)] public float range3;
    }
}

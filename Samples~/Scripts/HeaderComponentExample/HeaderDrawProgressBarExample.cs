using SaintsField.ComponentHeader;
using SaintsField.Samples.Scripts.SaintsEditor;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace SaintsField.Samples.Scripts.HeaderComponentExample
{
    public class HeaderDrawProgressBarExample : SaintsMonoBehaviour
    {
#if UNITY_EDITOR

        [HeaderDraw]
        private HeaderUsed HeaderDrawRight1G1(HeaderArea headerArea)
        {
            Rect useRect = new Rect(headerArea.MakeXWidthRect(headerArea.GroupStartX - 100, 100))
            {
                y = headerArea.Y + 2,
                height = headerArea.Height - 4,
            };
            Rect progressRect = new Rect(useRect)
            {
                width = range1 * useRect.width,
            };

            EditorGUI.DrawRect(useRect, Color.gray);
            EditorGUI.DrawRect(progressRect, Color.red);

            return new HeaderUsed(useRect);
        }
#endif

        [Range(0f, 1f)] public float range1;
    }
}

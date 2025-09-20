using SaintsField.ComponentHeader;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Samples.Scripts.HeaderComponentExample.Issues.Issue300
{
    public class Issue300InheDupParent : SaintsMonoBehaviour
    {
        [Range(0, 1)] public float normalizedHealth;

#if UNITY_EDITOR
        [HeaderDraw]
        private HeaderUsed HeaderDrawRight1G1(HeaderArea headerArea)
        {
            // this is drawing from right to left, so we need to backward the rect space
            // if(isRelay) return new HeaderUsed();

            Rect useRect = new Rect(headerArea.MakeXWidthRect(headerArea.GroupStartX-50,50)) { y = headerArea.Y+2,height = headerArea.Height-4, };
            Rect progressRect = new Rect(useRect) { width = normalizedHealth*useRect.width, };
            EditorGUI.DrawRect(useRect,Color.gray);
            EditorGUI.DrawRect(progressRect,Color.yellow);
            return new HeaderUsed(useRect);
        }

        [Button("DAMAGE <color=red>X"), HeaderButton("<color=red> X ", "Damaga")]
        private void TestDamage(float damage = 1)
        {
        }
#endif
    }
}

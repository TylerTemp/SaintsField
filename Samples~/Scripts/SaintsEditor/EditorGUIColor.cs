using System;
using SaintsField.Playa;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public class EditorGUIColor : SaintsMonoBehaviour
    {
        [GUIColor(EColor.Gold)]
        [InfoBox("This is colored gold using <b><u><color=white>GUIColor</color></u></b> attribute.")]
        [Button]
        private void ButtonGold(){}

        [ShowInInspector]
        [GUIColor("#00FF00")]
        private int _greenInt = 42;

        [ShowInInspector]
        [GUIColor(EColor.Burlywood)]
        private int Calc([PropRange(0,  10)] int v) => v + Random.Range(1, 9) * 100;

        [ShowInInspector]
        [GUIColor("$" + nameof(GetColor))]
        [InfoBox("Dynamic callback")]
        private DateTime _dt;

        private byte _color255;

        private Color GetColor()
        {
            _color255 = (byte)((_color255 + 10) % 256);
            byte r = (byte)(_color255 * 1 % 256);
            byte g = (byte)((_color255 * 2 + 100) % 256);
            byte b = (byte)((_color255 * 3 + 100) % 256);
            return new Color32(r, g, b, 255);
        }
    }
}

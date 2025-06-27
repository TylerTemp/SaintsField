using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaintsField.Editor.ColorPalette
{
#if SAINTSFIELD_DEBUG
    [CreateAssetMenu(fileName = "ColorPalette", menuName = "Saints/Color Palette NEW")]
#endif
    public class ColorPaletteArray: ScriptableObject, IReadOnlyList<ColorPaletteArray.ColorInfo>
    {
        [Serializable]
        public struct ColorInfo
        {
            public Color color;
            public string[] labels;
        }

        public ColorInfo[] colorInfoArray;
        public IEnumerator<ColorInfo> GetEnumerator() => ((IEnumerable<ColorInfo>)colorInfoArray).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => colorInfoArray.Length;

        public ColorInfo this[int index] => colorInfoArray[index];
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace SaintsField
{
#if SAINTSFIELD_DEBUG
    [CreateAssetMenu(fileName = "ColorPalette", menuName = "Saints/Color Palette")]
#endif
    public class ColorPalette: ScriptableObject
    {
        [Required]
        public string displayName = "";

        [Serializable]
        public struct ColorEntry
        {
            public Color color;
            public string displayName;
        }

        [ArraySize(min: 1)]
        public List<ColorEntry> colors = new List<ColorEntry>();
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace SaintsField.Editor.ColorPalette
{
    public class ColorPaletteEditing
    {
        public enum ColorChangeStatus
        {
            New,
            NoChange,
            Changed,
        }

        public Color Color;
        public Color NewColor;
        public ColorChangeStatus ColorStatus;

        public List<string> Labels = new List<string>();
    }
}

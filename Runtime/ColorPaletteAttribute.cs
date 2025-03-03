using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SaintsField.Interfaces;
using SaintsField.Utils;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = true)]
    public class ColorPaletteAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy => "";

        public struct ColorPaletteSource
        {
            public string Name;
            public bool IsCallback;
        }

        public readonly IReadOnlyList<ColorPaletteSource> ColorPaletteSources;

        public ColorPaletteAttribute(params string[] names)
        {
            ColorPaletteSources = names
                .Select(each =>
                {
                    (string content, bool isCallback) = RuntimeUtil.ParseCallback(each);
                    return new ColorPaletteSource
                    {
                        Name = content,
                        IsCallback = isCallback,
                    };
                })
                .ToArray();
        }
    }
}

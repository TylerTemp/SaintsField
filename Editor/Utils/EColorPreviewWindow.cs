using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Playa;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Utils
{
    public class EColorPreviewWindow: SaintsEditorWindow
    {
// #if SAINTSFIELD_DEBUG
//         [MenuItem("Saints/EColor Preview")]
// #endif
        public static void Open()
        {
            GetWindow<EColorPreviewWindow>("EColor").Show();
        }

        [Serializable]
        public struct EColorInfo
        {
            [HideIf(true)]
            public EColor name;

            [LayoutStart("Color", ELayout.Horizontal)]
            [NoLabel] public Color color;
            [NoLabel] public string hex;

            [LayoutStart("RGB", ELayout.TitleBox)]

            [LayoutStart("./256", ELayout.Horizontal)]
            [NoLabel] public byte r256;
            [NoLabel] public byte g256;
            [NoLabel] public byte b256;

            [LayoutStart("../RGB 0-1", ELayout.Horizontal | ELayout.TitleOut)]
            [NoLabel] public float r;
            [NoLabel] public float g;
            [NoLabel] public float b;
        }

        [
            DefaultExpand,
            FieldLabelText("$" + nameof(EColorInfoLabel)),
            ListDrawerSettings(searchable: true, overrideSearch: nameof(EColorInfoSearch)),
        ]
        public EColorInfo[] eColorInfos;

        private string EColorInfoLabel(EColorInfo eColorInfo) =>
            $"<color={eColorInfo.name}>██ EColor ██</color>.{eColorInfo.name}";

        private bool EColorInfoSearch(EColorInfo eColorInfo, int _, IReadOnlyList<ListSearchToken> tokens)
        {
            return RuntimeUtil.SimpleSearch(eColorInfo.name.ToString(), tokens);
        }

        public override void OnEditorEnable()
        {
            List<EColor> eColorEnums = Enum.GetValues(typeof(EColor)).Cast<EColor>().ToList();
            eColorEnums.Sort((a, b) => string.Compare(a.ToString(), b.ToString(), StringComparison.Ordinal));

            eColorInfos = eColorEnums
                .Select(each =>
                {
                    Color color = each.GetColor();
                    Color32 color32 = color;
                    return new EColorInfo
                    {
                        // name = $"<color={each.ToString().ToLower()}>██ EColor.{each}</color>",
                        name = each,
                        color = color,
                        hex = ColorUtility.ToHtmlStringRGB(color),
                        r = color.r,
                        g = color.g,
                        b = color.b,
                        r256 = color32.r,
                        g256 = color32.g,
                        b256 = color32.b,
                    };
                })
                .ToArray();
        }
    }
}

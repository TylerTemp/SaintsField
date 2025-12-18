using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Playa;
using SaintsField.Utils;
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
            [NoLabel] public string displayName;
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

        private class EColorInfoWarmComparer : IComparer<EColorInfo>, IComparer
        {
            private readonly bool _reverse;

            public EColorInfoWarmComparer(bool reverse = false)
            {
                _reverse = reverse;
            }

            public int Compare(EColorInfo x, EColorInfo y)
            {
                int comp = Score(x).CompareTo(Score(y));
                return _reverse ? -comp : comp;
            }

            // private const float WarmRefRad = 30f * Mathf.Deg2Rad; // 30 degrees is the warm reference

            private static float Score(EColorInfo e)
            {
                Color.RGBToHSV(e.color, out float h, out float s, out float v);
                // float angle = h * 2f * Mathf.PI;
                // // cosine centered on WarmRefRad: values near that angle are warmest, opposite are coldest
                // float warmness = Mathf.Cos(angle - WarmRefRad); // -1..1
                // // weight by saturation and brightness so vivid bright colors rank stronger
                // return warmness * (0.5f + 0.5f * s) * v;
                // return h;

                // Convert hue to degrees (0–360)
                float hue = h * 360f;

                // Distance from red (0°)
                float warmth = Mathf.Min(
                    Mathf.Abs(hue),
                    Mathf.Abs(hue - 360f)
                );

                return warmth;
            }

            public int Compare(object x, object y)
            {
                if (x is EColorInfo xM && y is EColorInfo yM)
                {
                    return Compare(xM, yM);
                }

                return 0;
            }
        }

        private class EColorInfoHueComparer : IComparer<EColorInfo>, IComparer
        {
            private readonly bool _reverse;

            public EColorInfoHueComparer(bool reverse = false)
            {
                _reverse = reverse;
            }

            public int Compare(EColorInfo x, EColorInfo y)
            {
                int comp = NormalCompare(x, y);
                return _reverse ? -comp : comp;
            }

            private static int NormalCompare(EColorInfo x, EColorInfo y)
            {
                Color xColor = x.color;
                Color yColor = y.color;
                Color.RGBToHSV(xColor, out float xH, out float xS, out float xV);
                Color.RGBToHSV(yColor, out float yH, out float yS, out float yV);

                int hComp = xH.CompareTo(yH);
                if (hComp != 0) return hComp;
                int sComp = xS.CompareTo(yS);
                if (sComp != 0) return sComp;
                return xV.CompareTo(yV);
            }

            public int Compare(object x, object y)
            {
                if (x is EColorInfo xM && y is EColorInfo yM)
                {
                    return Compare(xM, yM);
                }

                return 0;
            }
        }

        public enum SortBy
        {
            Name,
            Hue,
            Warm,
        }

        // [LayoutStart("Sort", ELayout.Horizontal)]
        [ValueButtons, OnValueChanged(nameof(SortByChanged))] public SortBy sortBy;
        [ValueButtons, OnValueChanged(nameof(DescChanged))] public bool desc;
        // [LayoutEnd]
        [
            DefaultExpand,
            FieldLabelText("$" + nameof(EColorInfoLabel)),
            ListDrawerSettings(searchable: true, overrideSearch: nameof(EColorInfoSearch)),
        ]
        public List<EColorInfo> eColorInfos;

        private void SortByChanged(SortBy by) => SortChanged(by, desc);
        private void DescChanged(bool descending) => SortChanged(sortBy, descending);
        private void SortChanged(SortBy by, bool descending)
        {
            List<EColorInfo> list = CreateList();
            switch (by)
            {
                case SortBy.Name:
                    // list.Sort((a, b) => string.Compare(a.name.ToString(), b.name.ToString(), StringComparison.Ordinal));
                    if (descending)
                    {
                        list.Reverse();
                    }
                    break;
                case SortBy.Hue:
                    list.Sort(new EColorInfoHueComparer(descending));
                    break;
                case SortBy.Warm:
                    list.Sort(new EColorInfoWarmComparer(descending));
                    break;

            }

            eColorInfos = list;
        }

        private string EColorInfoLabel(EColorInfo eColorInfo) =>
            $"<color={eColorInfo.name}>██ EColor ██</color>.{eColorInfo.name}";

        private bool EColorInfoSearch(EColorInfo eColorInfo, int _, IReadOnlyList<ListSearchToken> tokens) =>
            RuntimeUtil.SimpleSearch(eColorInfo.name.ToString(), tokens);

        public override void OnEditorEnable()
        {
            SortChanged(sortBy, desc);
        }

        private static List<EColorInfo> CreateList()
        {
            List<EColor> eColorEnums = Enum.GetValues(typeof(EColor)).Cast<EColor>().ToList();
            eColorEnums.Sort((a, b) => string.Compare(a.ToString(), b.ToString(), StringComparison.Ordinal));

            return eColorEnums
                .Select(each =>
                {
                    Color color = each.GetColor();
                    Color32 color32 = color;
                    return new EColorInfo
                    {
                        // name = $"<color={each.ToString().ToLower()}>██ EColor.{each}</color>",
                        name = each,
                        displayName = each.ToString(),
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
                .ToList();
        }
    }
}

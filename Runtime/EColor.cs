using SaintsField.Utils;
using UnityEngine;

namespace SaintsField
{
    public enum EColor
    {
        // https://docs.unity3d.com/Packages/com.unity.ugui@1.0/manual/StyledText.html#ColorNames
        // this this rich text
        Aqua,
        Black,
        Blue,
        Brown,
        Cyan,
        DarkBlue,
        Fuchsia,
        Green,
        Gray,
        Grey,
        LightBlue,
        Lime,
        Magenta,
        Maroon,
        Navy,
        Olive,
        Orange,
        Purple,
        Red,
        Silver,
        Teal,
        White,
        Yellow,

        // this is extended from [NaughtyAttributes](https://github.com/dbrizov/NaughtyAttributes/blob/master/Assets/NaughtyAttributes/Scripts/Core/Utility/EColor.cs)
        Clear,
        Pink,
        Indigo,
        Violet,

        // this is extended from Unity UI Toolkit: ProgressBar
        CharcoalGray,  // progressBar background
        OceanicSlate,  // progressBar fill
        MidnightAsh,  // box border

#if UNITY_EDITOR
        // this is dynamic color
        EditorSeparator,
        EditorEmphasized,
        EditorButtonHover,
#endif

        // Unity 6k
        AliceBlue,
        AntiqueWhite,
        Aquamarine,
        Azure,
        Beige,
        Bisque,
        BlanchedAlmond,
        BlueViolet,
        Burlywood,
        CadetBlue,
        Chartreuse,
        Chocolate,
        Coral,
        CornflowerBlue,
        Cornsilk,
        Crimson,
        DarkCyan,
        DarkGoldenRod,
        DarkGray,
        DarkGreen,
        DarkKhaki,
        DarkMagenta,
        DarkOliveGreen,
        DarkOrange,
        DarkOrchid,
        DarkRed,
        DarkSalmon,
        DarkSeaGreen,
        DarkSlateBlue,
        DarkSlateGray,
        DarkTurquoise,
        DarkViolet,
        DeepPink,
        DeepSkyBlue,
        DimGray,
        DodgerBlue,
        Firebrick,
        FloralWhite,
        ForestGreen,
        Gainsboro,
        GhostWhite,
        Gold,
        GoldenRod,
        Gray1,
        Gray2,
        Gray3,
        Gray4,
        Gray5,
        Gray6,
        Gray7,
        Gray8,
        Gray9,
        GreenYellow,
        Honeydew,
        HotPink,
        IndianRed,
        Ivory,
        Khaki,
        Lavender,
        LavenderBlush,
        LawnGreen,
        LemonChiffon,
        LightCoral,
        LightCyan,
        LightGoldenRod,
        LightGoldenRodYellow,
        LightGray,
        LightGreen,
        LightPink,
        LightSalmon,
        LightSeaGreen,
        LightSkyBlue,
        LightSlateBlue,
        LightSlateGray,
        LightSteelBlue,
        LightYellow,
        LimeGreen,
        Linen,
        MediumAquamarine,
        MediumBlue,
        MediumOrchid,
        MediumPurple,
        MediumSeaGreen,
        MediumSlateBlue,
        MediumSpringGreen,
        MediumTurquoise,
        MediumVioletRed,
        MidnightBlue,
        MintCream,
        MistyRose,
        Moccasin,
        NavajoWhite,
        NavyBlue,
        OldLace,
        OliveDrab,
        OrangeRed,
        Orchid,
        PaleGoldenRod,
        PaleGreen,
        PaleTurquoise,
        PaleVioletRed,
        PapayaWhip,
        PeachPuff,
        Peru,
        Plum,
        PowderBlue,
        RebeccaPurple,
        RosyBrown,
        RoyalBlue,
        SaddleBrown,
        Salmon,
        SandyBrown,
        SeaGreen,
        Seashell,
        Sienna,
        SkyBlue,
        SlateBlue,
        SlateGray,
        Snow,
        SoftRed,
        SoftBlue,
        SoftGreen,
        SoftYellow,
        SpringGreen,
        SteelBlue,
        Tan,
        Thistle,
        Tomato,
        Turquoise,
        VioletRed,
        Wheat,
        WhiteSmoke,
        YellowGreen,
        YellowNice,

    }

    public static class EColorExtensions
    {
        public static Color GetColor(this EColor color)
        {
            switch (color)
            {
                case EColor.Aqua:
                case EColor.Cyan:
                    return Color.cyan;
                case EColor.Black:
                    return Color.black;
                case EColor.Blue:
                    return Color.blue;
                case EColor.Brown:
                    return Colors.GetColorByName("brown");
                case EColor.DarkBlue:
                    return Colors.GetColorByName("darkblue");
                case EColor.Fuchsia:
                    return Color.magenta;
                case EColor.Green:
                    return Colors.GetColorByName("green");
                case EColor.Gray:
                case EColor.Grey:
                    return Color.gray;
                case EColor.LightBlue:
                    return Colors.GetColorByName("lightblue");
                case EColor.Lime:
                    return Color.green;
                case EColor.Magenta:
                    return Color.magenta;
                case EColor.Maroon:
                    return Colors.GetColorByName("maroon");
                case EColor.Navy:
                    return Colors.GetColorByName("navy");
                case EColor.Olive:
                    return Colors.GetColorByName("olive");
                case EColor.Orange:
                    return Colors.GetColorByName("orange");
                case EColor.Purple:
                    return Colors.GetColorByName("purple");
                case EColor.Red:
                    return Color.red;
                case EColor.Silver:
                    return Colors.GetColorByName("silver");
                case EColor.Teal:
                    return Colors.GetColorByName("teal");
                case EColor.White:
                    return Color.white;
                case EColor.Yellow:
                    return Color.yellow;

                case EColor.Clear:
                    return Color.clear;

                case EColor.Pink:
                    return Colors.GetColorByName("pink");
                case EColor.Indigo:
                    return Colors.GetColorByName("indigo");
                case EColor.Violet:
                    return Colors.GetColorByName("violet");
                case EColor.CharcoalGray:
                    return Colors.GetColorByName("charcoalgray");
                case EColor.OceanicSlate:
                    return Colors.GetColorByName("oceanicslate");
                case EColor.MidnightAsh:
                    return Colors.GetColorByName("midnightash");
#if UNITY_EDITOR
                case EColor.EditorSeparator:
                    return Colors.GetColorByName("editorseparator");
                case EColor.EditorEmphasized:
                    return Colors.GetColorByName("editoremphasized");
                case EColor.EditorButtonHover:
                    return Colors.GetColorByName("editorbuttonhover");
#endif

                case EColor.AliceBlue:
                    return Colors.GetColorByName("aliceblue");
                case EColor.AntiqueWhite:
                    return Colors.GetColorByName("antiquewhite");
                case EColor.Aquamarine:
                    return Colors.GetColorByName("aquamarine");
                case EColor.Azure:
                    return Colors.GetColorByName("azure");
                case EColor.Beige:
                    return Colors.GetColorByName("beige");
                case EColor.Bisque:
                    return Colors.GetColorByName("bisque");
                case EColor.BlanchedAlmond:
                    return Colors.GetColorByName("blanchedalmond");
                case EColor.BlueViolet:
                    return Colors.GetColorByName("blueviolet");
                case EColor.Burlywood:
                    return Colors.GetColorByName("burlywood");
                case EColor.CadetBlue:
                    return Colors.GetColorByName("cadetblue");
                case EColor.Chartreuse:
                    return Colors.GetColorByName("chartreuse");
                case EColor.Chocolate:
                    return Colors.GetColorByName("chocolate");
                case EColor.Coral:
                    return Colors.GetColorByName("coral");
                case EColor.CornflowerBlue:
                    return Colors.GetColorByName("cornflowerblue");
                case EColor.Cornsilk:
                    return Colors.GetColorByName("cornsilk");
                case EColor.Crimson:
                    return Colors.GetColorByName("crimson");
                case EColor.DarkCyan:
                    return Colors.GetColorByName("darkcyan");
                case EColor.DarkGoldenRod:
                    return Colors.GetColorByName("darkgoldenrod");
                case EColor.DarkGray:
                    return Colors.GetColorByName("darkgray");
                case EColor.DarkGreen:
                    return Colors.GetColorByName("darkgreen");
                case EColor.DarkKhaki:
                    return Colors.GetColorByName("darkkhaki");
                case EColor.DarkMagenta:
                    return Colors.GetColorByName("darkmagenta");
                case EColor.DarkOliveGreen:
                    return Colors.GetColorByName("darkolivegreen");
                case EColor.DarkOrange:
                    return Colors.GetColorByName("darkorange");
                case EColor.DarkOrchid:
                    return Colors.GetColorByName("darkorchid");
                case EColor.DarkRed:
                    return Colors.GetColorByName("darkred");
                case EColor.DarkSalmon:
                    return Colors.GetColorByName("darksalmon");
                case EColor.DarkSeaGreen:
                    return Colors.GetColorByName("darkseagreen");
                case EColor.DarkSlateBlue:
                    return Colors.GetColorByName("darkslateblue");
                case EColor.DarkSlateGray:
                    return Colors.GetColorByName("darkslategray");
                case EColor.DarkTurquoise:
                    return Colors.GetColorByName("darkturquoise");
                case EColor.DarkViolet:
                    return Colors.GetColorByName("darkviolet");
                case EColor.DeepPink:
                    return Colors.GetColorByName("deeppink");
                case EColor.DeepSkyBlue:
                    return Colors.GetColorByName("deepskyblue");
                case EColor.DimGray:
                    return Colors.GetColorByName("dimgray");
                case EColor.DodgerBlue:
                    return Colors.GetColorByName("dodgerblue");
                case EColor.Firebrick:
                    return Colors.GetColorByName("firebrick");
                case EColor.FloralWhite:
                    return Colors.GetColorByName("floralwhite");
                case EColor.ForestGreen:
                    return Colors.GetColorByName("forestgreen");
                case EColor.Gainsboro:
                    return Colors.GetColorByName("gainsboro");
                case EColor.GhostWhite:
                    return Colors.GetColorByName("ghostwhite");
                case EColor.Gold:
                    return Colors.GetColorByName("gold");
                case EColor.GoldenRod:
                    return Colors.GetColorByName("goldenrod");
                case EColor.Gray1:
                    return Colors.GetColorByName("gray1");
                case EColor.Gray2:
                    return Colors.GetColorByName("gray2");
                case EColor.Gray3:
                    return Colors.GetColorByName("gray3");
                case EColor.Gray4:
                    return Colors.GetColorByName("gray4");
                case EColor.Gray5:
                    return Colors.GetColorByName("gray5");
                case EColor.Gray6:
                    return Colors.GetColorByName("gray6");
                case EColor.Gray7:
                    return Colors.GetColorByName("gray7");
                case EColor.Gray8:
                    return Colors.GetColorByName("gray8");
                case EColor.Gray9:
                    return Colors.GetColorByName("gray9");
                case EColor.GreenYellow:
                    return Colors.GetColorByName("greenyellow");
                case EColor.Honeydew:
                    return Colors.GetColorByName("honeydew");
                case EColor.HotPink:
                    return Colors.GetColorByName("hotpink");
                case EColor.IndianRed:
                    return Colors.GetColorByName("indianred");
                case EColor.Ivory:
                    return Colors.GetColorByName("ivory");
                case EColor.Khaki:
                    return Colors.GetColorByName("khaki");
                case EColor.Lavender:
                    return Colors.GetColorByName("lavender");
                case EColor.LavenderBlush:
                    return Colors.GetColorByName("lavenderblush");
                case EColor.LawnGreen:
                    return Colors.GetColorByName("lawngreen");
                case EColor.LemonChiffon:
                    return Colors.GetColorByName("lemonchiffon");
                case EColor.LightCoral:
                    return Colors.GetColorByName("lightcoral");
                case EColor.LightCyan:
                    return Colors.GetColorByName("lightcyan");
                case EColor.LightGoldenRod:
                    return Colors.GetColorByName("lightgoldenrod");
                case EColor.LightGoldenRodYellow:
                    return Colors.GetColorByName("lightgoldenrodyellow");
                case EColor.LightGray:
                    return Colors.GetColorByName("lightgray");
                case EColor.LightGreen:
                    return Colors.GetColorByName("lightgreen");
                case EColor.LightPink:
                    return Colors.GetColorByName("lightpink");
                case EColor.LightSalmon:
                    return Colors.GetColorByName("lightsalmon");
                case EColor.LightSeaGreen:
                    return Colors.GetColorByName("lightseagreen");
                case EColor.LightSkyBlue:
                    return Colors.GetColorByName("lightskyblue");
                case EColor.LightSlateBlue:
                    return Colors.GetColorByName("lightslateblue");
                case EColor.LightSlateGray:
                    return Colors.GetColorByName("lightslategray");
                case EColor.LightSteelBlue:
                    return Colors.GetColorByName("lightsteelblue");
                case EColor.LightYellow:
                    return Colors.GetColorByName("lightyellow");
                case EColor.LimeGreen:
                    return Colors.GetColorByName("limegreen");
                case EColor.Linen:
                    return Colors.GetColorByName("linen");
                case EColor.MediumAquamarine:
                    return Colors.GetColorByName("mediumaquamarine");
                case EColor.MediumBlue:
                    return Colors.GetColorByName("mediumblue");
                case EColor.MediumOrchid:
                    return Colors.GetColorByName("mediumorchid");
                case EColor.MediumPurple:
                    return Colors.GetColorByName("mediumpurple");
                case EColor.MediumSeaGreen:
                    return Colors.GetColorByName("mediumseagreen");
                case EColor.MediumSlateBlue:
                    return Colors.GetColorByName("mediumslateblue");
                case EColor.MediumSpringGreen:
                    return Colors.GetColorByName("mediumspringgreen");
                case EColor.MediumTurquoise:
                    return Colors.GetColorByName("mediumturquoise");
                case EColor.MediumVioletRed:
                    return Colors.GetColorByName("mediumvioletred");
                case EColor.MidnightBlue:
                    return Colors.GetColorByName("midnightblue");
                case EColor.MintCream:
                    return Colors.GetColorByName("mintcream");
                case EColor.MistyRose:
                    return Colors.GetColorByName("mistyrose");
                case EColor.Moccasin:
                    return Colors.GetColorByName("moccasin");
                case EColor.NavajoWhite:
                    return Colors.GetColorByName("navajowhite");
                case EColor.NavyBlue:
                    return Colors.GetColorByName("navyblue");
                case EColor.OldLace:
                    return Colors.GetColorByName("oldlace");
                case EColor.OliveDrab:
                    return Colors.GetColorByName("olivedrab");
                case EColor.OrangeRed:
                    return Colors.GetColorByName("orangered");
                case EColor.Orchid:
                    return Colors.GetColorByName("orchid");
                case EColor.PaleGoldenRod:
                    return Colors.GetColorByName("palegoldenrod");
                case EColor.PaleGreen:
                    return Colors.GetColorByName("palegreen");
                case EColor.PaleTurquoise:
                    return Colors.GetColorByName("paleturquoise");
                case EColor.PaleVioletRed:
                    return Colors.GetColorByName("palevioletred");
                case EColor.PapayaWhip:
                    return Colors.GetColorByName("papayawhip");
                case EColor.PeachPuff:
                    return Colors.GetColorByName("peachpuff");
                case EColor.Peru:
                    return Colors.GetColorByName("peru");
                case EColor.Plum:
                    return Colors.GetColorByName("plum");
                case EColor.PowderBlue:
                    return Colors.GetColorByName("powderblue");
                case EColor.RebeccaPurple:
                    return Colors.GetColorByName("rebeccapurple");
                case EColor.RosyBrown:
                    return Colors.GetColorByName("rosybrown");
                case EColor.RoyalBlue:
                    return Colors.GetColorByName("royalblue");
                case EColor.SaddleBrown:
                    return Colors.GetColorByName("saddlebrown");
                case EColor.Salmon:
                    return Colors.GetColorByName("salmon");
                case EColor.SandyBrown:
                    return Colors.GetColorByName("sandybrown");
                case EColor.SeaGreen:
                    return Colors.GetColorByName("seagreen");
                case EColor.Seashell:
                    return Colors.GetColorByName("seashell");
                case EColor.Sienna:
                    return Colors.GetColorByName("sienna");
                case EColor.SkyBlue:
                    return Colors.GetColorByName("skyblue");
                case EColor.SlateBlue:
                    return Colors.GetColorByName("slateblue");
                case EColor.SlateGray:
                    return Colors.GetColorByName("slategray");
                case EColor.Snow:
                    return Colors.GetColorByName("snow");
                case EColor.SoftRed:
                    return Colors.GetColorByName("softred");
                case EColor.SoftBlue:
                    return Colors.GetColorByName("softblue");
                case EColor.SoftGreen:
                    return Colors.GetColorByName("softgreen");
                case EColor.SoftYellow:
                    return Colors.GetColorByName("softyellow");
                case EColor.SpringGreen:
                    return Colors.GetColorByName("springgreen");
                case EColor.SteelBlue:
                    return Colors.GetColorByName("steelblue");
                case EColor.Tan:
                    return Colors.GetColorByName("tan");
                case EColor.Thistle:
                    return Colors.GetColorByName("thistle");
                case EColor.Tomato:
                    return Colors.GetColorByName("tomato");
                case EColor.Turquoise:
                    return Colors.GetColorByName("turquoise");
                case EColor.VioletRed:
                    return Colors.GetColorByName("violetred");
                case EColor.Wheat:
                    return Colors.GetColorByName("wheat");
                case EColor.WhiteSmoke:
                    return Colors.GetColorByName("whitesmoke");
                case EColor.YellowGreen:
                    return Colors.GetColorByName("yellowgreen");
                case EColor.YellowNice:
                    return Colors.GetColorByName("yellownice");


                default:
                    return Color.white;
            };
        }


    }
}

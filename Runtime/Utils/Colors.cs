#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace SaintsField.Utils
{
    public static class Colors
    {
        public static Color GetColorByStringPresent(string name)
        {
            if (name == null)
            {
                return Color.white;
            }

            if(ColorUtility.TryParseHtmlString(name, out Color color))
            {
                return color;
            }

            // object reflectValue = typeof(Color)
            //     .GetProperties(BindingFlags.Public | BindingFlags.Static)
            //     .Where(p => p.PropertyType == typeof(Color))
            //     .FirstOrDefault(p => p.Name == name)
            //     ?.GetValue(null);
            //
            // return reflectValue != null
            //     ? (Color32)reflectValue
            //     : Color.white;

            // Debug.Log($"colorName={name}");

            return GetColorByName(name);
        }

        public static Color GetColorByName(string name)
        {
            switch (name.ToLower())
            {
                case "aliceblue":
                    return new Color(0.9411765f, 0.9725491f, 1f, 1f);
                case "antiquewhite":
                    return new Color(0.9803922f, 0.9215687f, 0.8431373f, 1f);
                case "aquamarine":
                    return new Color(0.4980392f, 1f, 0.8313726f, 1f);
                case "red":
                    return Color.red;
                case "green":  // this is not error:  https://docs.unity3d.com/Packages/com.unity.ugui@1.0/manual/StyledText.html#ColorNames
                    return new Color32(0, 128, 0, 255);
                case "blue":
                    return Color.blue;
                case "white":
                    return Color.white;
                case "black":
                    return Color.black;
                case "yellow":
                    return Color.yellow;
                case "aqua":
                case "cyan":
                    return Color.cyan;
                case "fuchsia":
                case "magenta":
                    return Color.magenta;
                case "gray":
                case "grey":
                    return Color.grey;
                // ReSharper disable once StringLiteralTypo
                case "charcoalgray":
                    return new Color32(48, 48, 48, 255);
                case "clear":
                    return Color.clear;
                case "pink":
                    return new Color32(255, 152, 203, 255);
                case "orange":
                    return new Color32(255, 165, 0, 255);
                case "indigo":
                    return new Color32(75, 0, 130, 255);
                case "violet":
                    return new Color32(128, 0, 255, 255);
                case "brown":
                    return new Color32(165, 42, 42, 255);
                // ReSharper disable once StringLiteralTypo
                case "darkblue":
                    return new Color32(0, 0, 160, 255);
                // ReSharper disable once StringLiteralTypo
                case "lightblue":
                    return new Color32(173, 216, 230, 255);
                case "lime":  // this is not error:  https://docs.unity3d.com/Packages/com.unity.ugui@1.0/manual/StyledText.html#ColorNames
                    return Color.green;
                case "maroon":
                    return new Color32(128, 0, 0, 255);
                case "navy":
                    return new Color32(0, 0, 128, 255);
                case "olive":
                    return new Color32(128, 128, 0, 255);
                case "purple":
                    return new Color32(128, 0, 128, 255);
                case "silver":
                    return new Color32(192, 192, 192, 0);
                case "teal":
                    return new Color32(0, 128, 128, 255);

                // ReSharper disable once StringLiteralTypo
                case "oceanicslate":
                    return new Color32(44, 93, 135, 255);

                case "midnightash":
                    return new Color32(35, 35, 35, 255);

#if UNITY_EDITOR
                case "editorseparator":
                {
                    float c = EditorGUIUtility.isProSkin ? 0.45f : 0.4f;
                    return new Color(c, c, c);
                }

                case "editoremphasized":
                {
                    if (EditorGUIUtility.isProSkin)
                    {
                        return EColor.CharcoalGray.GetColor();
                    }

                    const float c = 155f / 255f;
                    return new Color(c, c, c);
                }
                case "editorbuttonhover":
                {
                    if (EditorGUIUtility.isProSkin)
                    {
                        const int p = 80;
                        return new Color32(p, p, p, 255);
                    }

                    const float c = 155f / 255f;
                    return new Color(c, c, c);
                }
#endif
                case "azure":
                    return new Color(0.9411765f, 1f, 1f, 1f);
                case "beige":
                    return new Color(0.9607844f, 0.9607844f, 0.8627452f, 1f);
                case "bisque":
                    return new Color(1f, 0.8941177f, 0.7686275f, 1f);
                case "blanchedalmond":
                    return new Color(1f, 0.9215687f, 0.8039216f, 1f);
                case "blueviolet":
                    return new Color(0.5411765f, 0.1686275f, 0.8862746f, 1f);
                case "burlywood":
                    return new Color(0.8705883f, 0.7215686f, 0.5294118f, 1f);
                case "cadetblue":
                    return new Color(0.372549f, 0.6196079f, 0.627451f, 1f);
                case "chartreuse":
                    return new Color(0.4980392f, 1f, 0.0f, 1f);
                case "chocolate":
                    return new Color(0.8235295f, 0.4117647f, 0.1176471f, 1f);
                case "coral":
                    return new Color(1f, 0.4980392f, 0.3137255f, 1f);
                case "cornflowerblue":
                    return new Color(0.3921569f, 0.5843138f, 0.9294118f, 1f);
                case "cornsilk":
                    return new Color(1f, 0.9725491f, 0.8627452f, 1f);
                case "crimson":
                    return new Color(0.8627452f, 0.07843138f, 0.2352941f, 1f);
                case "darkcyan":
                    return new Color(0.0f, 0.5450981f, 0.5450981f, 1f);
                case "darkgoldenrod":
                    return new Color(0.7215686f, 0.5254902f, 0.04313726f, 1f);
                case "darkgray":
                    return new Color(0.6627451f, 0.6627451f, 0.6627451f, 1f);
                case "darkgreen":
                    return new Color(0.0f, 0.3921569f, 0.0f, 1f);
                case "darkkhaki":
                    return new Color(0.7411765f, 0.7176471f, 0.4196079f, 1f);
                case "darkmagenta":
                    return new Color(0.5450981f, 0.0f, 0.5450981f, 1f);
                case "darkolivegreen":
                    return new Color(0.3333333f, 0.4196079f, 0.1843137f, 1f);
                case "darkorange":
                    return new Color(1f, 0.5490196f, 0.0f, 1f);
                case "darkorchid":
                    return new Color(0.6f, 0.1960784f, 0.8000001f, 1f);
                case "darkred":
                    return new Color(0.5450981f, 0.0f, 0.0f, 1f);
                case "darksalmon":
                    return new Color(0.9137256f, 0.5882353f, 0.4784314f, 1f);
                case "darkseagreen":
                    return new Color(0.5607843f, 0.7372549f, 0.5607843f, 1f);
                case "darkslateblue":
                    return new Color(0.282353f, 0.2392157f, 0.5450981f, 1f);
                case "darkslategray":
                    return new Color(0.1843137f, 0.3098039f, 0.3098039f, 1f);
                case "darkturquoise":
                    return new Color(0.0f, 0.8078432f, 0.8196079f, 1f);
                case "darkviolet":
                    return new Color(0.5803922f, 0.0f, 0.8274511f, 1f);
                case "deeppink":
                    return new Color(1f, 0.07843138f, 0.5764706f, 1f);
                case "deepskyblue":
                    return new Color(0.0f, 0.7490196f, 1f, 1f);
                case "dimgray":
                    return new Color(0.4117647f, 0.4117647f, 0.4117647f, 1f);
                case "dodgerblue":
                    return new Color(0.1176471f, 0.5647059f, 1f, 1f);
                case "firebrick":
                    return new Color(0.6980392f, 0.1333333f, 0.1333333f, 1f);
                case "floralwhite":
                    return new Color(1f, 0.9803922f, 0.9411765f, 1f);
                case "forestgreen":
                    return new Color(0.1333333f, 0.5450981f, 0.1333333f, 1f);
                case "gainsboro":
                    return new Color(0.8627452f, 0.8627452f, 0.8627452f, 1f);
                case "ghostwhite":
                    return new Color(0.9725491f, 0.9725491f, 1f, 1f);
                case "gold":
                    return new Color(1f, 0.8431373f, 0.0f, 1f);
                case "goldenrod":
                    return new Color(0.854902f, 0.6470588f, 0.1254902f, 1f);
                case "gray1":
                    return new Color(0.1f, 0.1f, 0.1f, 1f);
                case "gray2":
                    return new Color(0.2f, 0.2f, 0.2f, 1f);
                case "gray3":
                    return new Color(0.3f, 0.3f, 0.3f, 1f);
                case "gray4":
                    return new Color(0.4f, 0.4f, 0.4f, 1f);
                case "gray5":
                    return new Color(0.5f, 0.5f, 0.5f, 1f);
                case "gray6":
                    return new Color(0.6f, 0.6f, 0.6f, 1f);
                case "gray7":
                    return new Color(0.7f, 0.7f, 0.7f, 1f);
                case "gray8":
                    return new Color(0.8f, 0.8f, 0.8f, 1f);
                case "gray9":
                    return new Color(0.9f, 0.9f, 0.9f, 1f);
                case "greenyellow":
                    return new Color(0.6784314f, 1f, 0.1843137f, 1f);
                case "honeydew":
                    return new Color(0.9411765f, 1f, 0.9411765f, 1f);
                case "hotpink":
                    return new Color(1f, 0.4117647f, 0.7058824f, 1f);
                case "indianred":
                    return new Color(0.8039216f, 0.3607843f, 0.3607843f, 1f);
                case "ivory":
                    return new Color(1f, 1f, 0.9411765f, 1f);
                case "khaki":
                    return new Color(0.9411765f, 0.9019608f, 0.5490196f, 1f);
                case "lavender":
                    return new Color(0.9019608f, 0.9019608f, 0.9803922f, 1f);
                case "lavenderblush":
                    return new Color(1f, 0.9411765f, 0.9607844f, 1f);
                case "lawngreen":
                    return new Color(0.4862745f, 0.9882354f, 0.0f, 1f);
                case "lemonchiffon":
                    return new Color(1f, 0.9803922f, 0.8039216f, 1f);
                case "lightcoral":
                    return new Color(0.9411765f, 0.5019608f, 0.5019608f, 1f);
                case "lightcyan":
                    return new Color(0.8784314f, 1f, 1f, 1f);
                case "lightgoldenrod":
                    return new Color(0.9333334f, 0.8666667f, 0.509804f, 1f);
                case "lightgoldenrodyellow":
                    return new Color(0.9803922f, 0.9803922f, 0.8235295f, 1f);
                case "lightgray":
                    return new Color(0.8274511f, 0.8274511f, 0.8274511f, 1f);
                case "lightgreen":
                    return new Color(0.5647059f, 0.9333334f, 0.5647059f, 1f);
                case "lightpink":
                    return new Color(1f, 0.7137255f, 0.7568628f, 1f);
                case "lightsalmon":
                    return new Color(1f, 0.627451f, 0.4784314f, 1f);
                case "lightseagreen":
                    return new Color(0.1254902f, 0.6980392f, 0.6666667f, 1f);
                case "lightskyblue":
                    return new Color(0.5294118f, 0.8078432f, 0.9803922f, 1f);
                case "lightslateblue":
                    return new Color(0.5176471f, 0.4392157f, 1f, 1f);
                case "lightslategray":
                    return new Color(0.4666667f, 0.5333334f, 0.6f, 1f);
                case "lightsteelblue":
                    return new Color(0.6901961f, 0.7686275f, 0.8705883f, 1f);
                case "lightyellow":
                    return new Color(1f, 1f, 0.8784314f, 1f);
                case "limegreen":
                    return new Color(0.1960784f, 0.8039216f, 0.1960784f, 1f);
                case "linen":
                    return new Color(0.9803922f, 0.9411765f, 0.9019608f, 1f);
                case "mediumaquamarine":
                    return new Color(0.4f, 0.8039216f, 0.6666667f, 1f);
                case "mediumblue":
                    return new Color(0.0f, 0.0f, 0.8039216f, 1f);
                case "mediumorchid":
                    return new Color(0.7294118f, 0.3333333f, 0.8274511f, 1f);
                case "mediumpurple":
                    return new Color(0.5764706f, 0.4392157f, 0.8588236f, 1f);
                case "mediumseagreen":
                    return new Color(0.2352941f, 0.7019608f, 0.4431373f, 1f);
                case "mediumslateblue":
                    return new Color(0.482353f, 0.4078432f, 0.9333334f, 1f);
                case "mediumspringgreen":
                    return new Color(0.0f, 0.9803922f, 0.6039216f, 1f);
                case "mediumturquoise":
                    return new Color(0.282353f, 0.8196079f, 0.8000001f, 1f);
                case "mediumvioletred":
                    return new Color(0.7803922f, 0.08235294f, 0.5215687f, 1f);
                case "midnightblue":
                    return new Color(0.09803922f, 0.09803922f, 0.4392157f, 1f);
                case "mintcream":
                    return new Color(0.9607844f, 1f, 0.9803922f, 1f);
                case "mistyrose":
                    return new Color(1f, 0.8941177f, 0.882353f, 1f);
                case "moccasin":
                    return new Color(1f, 0.8941177f, 0.7098039f, 1f);
                case "navajowhite":
                    return new Color(1f, 0.8705883f, 0.6784314f, 1f);
                case "navyblue":
                    return new Color(0.0f, 0.0f, 0.5019608f, 1f);
                case "oldlace":
                    return new Color(0.9921569f, 0.9607844f, 0.9019608f, 1f);
                case "olivedrab":
                    return new Color(0.4196079f, 0.5568628f, 0.1372549f, 1f);
                case "orangered":
                    return new Color(1f, 0.2705882f, 0.0f, 1f);
                case "orchid":
                    return new Color(0.854902f, 0.4392157f, 0.8392158f, 1f);
                case "palegoldenrod":
                    return new Color(0.9333334f, 0.909804f, 0.6666667f, 1f);
                case "palegreen":
                    return new Color(0.5960785f, 0.9843138f, 0.5960785f, 1f);
                case "paleturquoise":
                    return new Color(0.6862745f, 0.9333334f, 0.9333334f, 1f);
                case "palevioletred":
                    return new Color(0.8588236f, 0.4392157f, 0.5764706f, 1f);
                case "papayawhip":
                    return new Color(1f, 0.937255f, 0.8352942f, 1f);
                case "peachpuff":
                    return new Color(1f, 0.854902f, 0.7254902f, 1f);
                case "peru":
                    return new Color(0.8039216f, 0.5215687f, 0.2470588f, 1f);
                case "plum":
                    return new Color(0.8666667f, 0.627451f, 0.8666667f, 1f);
                case "powderblue":
                    return new Color(0.6901961f, 0.8784314f, 0.9019608f, 1f);
                case "rebeccapurple":
                    return new Color(0.4f, 0.2f, 0.6f, 1f);
                case "rosybrown":
                    return new Color(0.7372549f, 0.5607843f, 0.5607843f, 1f);
                case "royalblue":
                    return new Color(0.254902f, 0.4117647f, 0.882353f, 1f);
                case "saddlebrown":
                    return new Color(0.5450981f, 0.2705882f, 0.07450981f, 1f);
                case "salmon":
                    return new Color(0.9803922f, 0.5019608f, 0.4470589f, 1f);
                case "sandybrown":
                    return new Color(0.9568628f, 0.6431373f, 0.3764706f, 1f);
                case "seagreen":
                    return new Color(0.1803922f, 0.5450981f, 0.3411765f, 1f);
                case "seashell":
                    return new Color(1f, 0.9607844f, 0.9333334f, 1f);
                case "sienna":
                    return new Color(0.627451f, 0.3215686f, 0.1764706f, 1f);
                case "skyblue":
                    return new Color(0.5294118f, 0.8078432f, 0.9215687f, 1f);
                case "slateblue":
                    return new Color(0.4156863f, 0.3529412f, 0.8039216f, 1f);
                case "slategray":
                    return new Color(0.4392157f, 0.5019608f, 0.5647059f, 1f);
                case "snow":
                    return new Color(1f, 0.9803922f, 0.9803922f, 1f);
                case "softred":
                    return new Color(0.8627452f, 0.1921569f, 0.1960784f, 1f);
                case "softblue":
                    return new Color(0.1882353f, 0.682353f, 0.7490196f, 1f);
                case "softgreen":
                    return new Color(0.5490196f, 0.7882354f, 0.1411765f, 1f);
                case "softyellow":
                    return new Color(1f, 0.9333334f, 0.5490196f, 1f);
                case "springgreen":
                    return new Color(0.0f, 1f, 0.4980392f, 1f);
                case "steelblue":
                    return new Color(0.2745098f, 0.509804f, 0.7058824f, 1f);
                case "tan":
                    return new Color(0.8235295f, 0.7058824f, 0.5490196f, 1f);
                case "thistle":
                    return new Color(0.8470589f, 0.7490196f, 0.8470589f, 1f);
                case "tomato":
                    return new Color(1f, 0.3882353f, 0.2784314f, 1f);
                case "turquoise":
                    return new Color(0.2509804f, 0.8784314f, 0.8156863f, 1f);
                case "violetred":
                    return new Color(0.8156863f, 0.1254902f, 0.5647059f, 1f);
                case "wheat":
                    return new Color(0.9607844f, 0.8705883f, 0.7019608f, 1f);
                case "whitesmoke":
                    return new Color(1f, 0.92156863f, 0.015686275f, 1f);
                case "yellowgreen":
                    return new Color(0.6039216f, 0.8039216f, 0.1960784f, 1f);
                case "yellownice":
                    return new Color(1f, 0.92156863f, 0.015686275f, 1f);


                default:
                    return Color.white;
            }
        }

        // public static bool ColorNameSupported(string name)
        // {
        //     switch (name.ToLower())
        //     {
        //         case "red":
        //         case "green":
        //         case "blue":
        //         case "white":
        //         case "black":
        //         case "yellow":
        //         case "cyan":
        //         case "magenta":
        //         case "gray":
        //         case "grey":
        //         case "clear":
        //             return true;
        //         default:
        //             return false;
        //     }
        // }

        public static string ToHtmlHexString(Color color)
        {
            // Convert each color channel to its hexadecimal representation
            int r = Mathf.RoundToInt(color.r * 255);
            int g = Mathf.RoundToInt(color.g * 255);
            int b = Mathf.RoundToInt(color.b * 255);
            int a = Mathf.RoundToInt(color.a * 255);

            // Create the HTML hex string
            string htmlHex = $"#{r:X2}{g:X2}{b:X2}{a:X2}";

            return htmlHex;
        }
    }
}

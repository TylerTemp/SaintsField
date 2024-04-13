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
#endif


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

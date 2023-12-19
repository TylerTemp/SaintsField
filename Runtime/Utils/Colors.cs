using UnityEngine;

namespace SaintsField.Utils
{
    public static class Colors
    {
        public static Color GetColorByStringPresent(string name)
        {
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

            return GetColorByName(name);
        }

        public static Color GetColorByName(string name)
        {
            switch (name)
            {
                case "red":
                    return Color.red;
                case "green":
                    return Color.green;
                case "blue":
                    return Color.blue;
                case "white":
                    return Color.white;
                case "black":
                    return Color.black;
                case "yellow":
                    return Color.yellow;
                case "cyan":
                    return Color.cyan;
                case "magenta":
                    return Color.magenta;
                case "gray":
                    return Color.gray;
                case "grey":
                    return Color.grey;
                case "clear":
                    return Color.clear;
                case "pink":
                    return new Color32(255, 152, 203, 255);
                case "orange":
                    return new Color32(255, 128, 0, 255);
                case "indigo":
                    return new Color32(75, 0, 130, 255);
                case "violet":
                    return new Color32(128, 0, 255, 255);
                default:
                    return Color.white;
            }
        }

        public static bool ColorNameSupported(string name)
        {
            switch (name)
            {
                case "red":
                case "green":
                case "blue":
                case "white":
                case "black":
                case "yellow":
                case "cyan":
                case "magenta":
                case "gray":
                case "grey":
                case "clear":
                    return true;
                default:
                    return false;
            }
        }

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

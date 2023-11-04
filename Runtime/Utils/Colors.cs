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

        public static Color GetColorByName(string name) => name switch
        {
            "red" => Color.red,
            "green" => Color.green,
            "blue" => Color.blue,
            "white" => Color.white,
            "black" => Color.black,
            "yellow" => Color.yellow,
            "cyan" => Color.cyan,
            "magenta" => Color.magenta,
            "gray" => Color.gray,
            "grey" => Color.grey,
            "clear" => Color.clear,
            "pink" => new Color32(255, 152, 203, 255),
            "orange" => new Color32(255, 128, 0, 255),
            "indigo" => new Color32(75, 0, 130, 255),
            "violet" => new Color32(128, 0, 255, 255),
            _ => Color.white,
        };

        public static bool ColorNameSupported(string name) => name switch
        {
            "red" => true,
            "green" => true,
            "blue" => true,
            "white" => true,
            "black" => true,
            "yellow" => true,
            "cyan" => true,
            "magenta" => true,
            "gray" => true,
            "grey" => true,
            "clear" => true,
            _ => false,
        };

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

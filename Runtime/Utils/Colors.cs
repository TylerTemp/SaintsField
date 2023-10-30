using UnityEngine;

namespace ExtInspector.Utils
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
    }
}

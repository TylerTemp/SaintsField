using SaintsField.Utils;
using UnityEngine;

namespace SaintsField
{
    public enum EColor
    {
        Default,
        Clear,
        White,
        Black,
        Gray,
        Red,
        Pink,
        Orange,
        Yellow,
        Green,
        Blue,
        Indigo,
        Violet,
    }

    public static class EColorExtensions
    {
        public static Color GetColor(this EColor color)
        {
            return color switch
            {
                EColor.Clear => Color.clear,
                EColor.White => Color.white,
                EColor.Black => Color.black,
                EColor.Gray => Color.gray,
                EColor.Red => Color.red,
                EColor.Pink => Colors.GetColorByName("pink"),
                EColor.Orange => Colors.GetColorByName("orange"),
                EColor.Yellow => Color.yellow,
                EColor.Green => Color.green,
                EColor.Blue => Color.blue,
                EColor.Indigo => Colors.GetColorByName("indigo"),
                EColor.Violet => Colors.GetColorByName("violet"),
                _ => Color.white,
            };
        }


    }
}

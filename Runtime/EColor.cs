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
            switch (color)
            {
                case EColor.Clear:
                    return Color.clear;
                case EColor.White:
                    return Color.white;
                case EColor.Black:
                    return Color.black;
                case EColor.Gray:
                    return Color.gray;
                case EColor.Red:
                    return Color.red;
                case EColor.Pink:
                    return Colors.GetColorByName("pink");
                case EColor.Orange:
                    return Colors.GetColorByName("orange");
                case EColor.Yellow:
                    return Color.yellow;
                case EColor.Green:
                    return Color.green;
                case EColor.Blue:
                    return Color.blue;
                case EColor.Indigo:
                    return Colors.GetColorByName("indigo");
                case EColor.Violet:
                    return Colors.GetColorByName("violet");
                default:
                    return Color.white;
            };
        }


    }
}

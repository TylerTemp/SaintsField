using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class OptionsValueButtonsExample : MonoBehaviour
    {
        [OptionsValueButtons(0.5f, 1f, 1.5f, 2f, 2.5f, 3f)]
        public float floatOpt;

        public enum Direction
        {
            None,
            Left,
            Right,
            Up,
            Down,
        }

        [PairsValueButtons(
                "<icon=d_scrollleft/>", Direction.Left,
                "<icon=d_scrollup/>", Direction.Up,
                "<icon=d_scrollright/>", Direction.Right,
                "<icon=d_scrolldown/>", Direction.Down
            )]
        public Direction direct;

        [PairsValueButtons(
            "<color=brown>Broken", 0,
            "<color=green>Normal", 1,
            "<color=blue>Rare", 2,
            "<color=yellow>Legend", 3
        )]
        public int quality;
    }
}

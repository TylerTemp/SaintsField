using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class PairsDropdownExample : MonoBehaviour
    {
        [PairsDropdown("negative/1", -1, "negative/2", 2, "negative/3", -3, "zero", 0, "positive/1", 1, "positive/2", 2, "positive/3", 3)]
        public int intOpt;

        public enum Direction
        {
            None,
            Left,
            Right,
            Up,
            Down,
            Center,
        }

        // useful if you don't want the entire enum
        [PairsDropdown(EUnique.Disable, "<-", Direction.Left, "->", Direction.Right, "↑", Direction.Up, "↓", Direction.Down)]
        public Direction[] direOpt;
    }
}

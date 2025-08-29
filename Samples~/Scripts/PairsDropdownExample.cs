using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class PairsDropdownExample : MonoBehaviour
    {
        public enum Direction
        {
            None,
            Left,
            Right,
            Up,
            Down,
            Center,
        }

        [PairsDropdown("negative/1", -1, "negative/2", 2, "negative/3", -3, "zero", 0, "positive/1", 1, "positive/2", 2, "positive/3", 3)]
        public int intOpt;

        // useful if you don't want the entire enum
        [PairsDropdown(EUnique.Disable, "<-", Direction.Left, "->", Direction.Right, "↑", Direction.Up, "↓", Direction.Down)]
        public Direction[] direOpt;

        [PairsTreeDropdown("negative/1", -1, "negative/2", 2, "negative/3", -3, "zero", 0, "positive/1", 1, "positive/2", 2, "positive/3", 3)]
        public int treeIntOpt;

        // useful if you don't want the entire enum
        [PairsTreeDropdown(EUnique.Disable, "Hor/<-", Direction.Left, "Hor/->", Direction.Right, "Vert/↑", Direction.Up, "Vert/↓", Direction.Down)]
        public Direction[] treeDireOpt;
    }
}

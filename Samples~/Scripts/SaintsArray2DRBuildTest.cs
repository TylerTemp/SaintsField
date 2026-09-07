using System.Text;
using TMPro;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class SaintsArray2DRBuildTest : MonoBehaviour
    {
        public SaintsArray2DR<bool> array2Dr;
        [SerializeField, GetComponent] private TMP_Text _text;

        private void Awake()
        {
            StringBuilder text = new StringBuilder();
            int rows = array2Dr.GetLength(0);
            int columns = array2Dr.GetLength(1);
            for (int row = 0; row < rows; row++)
            {
                if (row > 0)
                {
                    text.Append(',');
                    text.Append('\n');
                }

                for (int column = 0; column < columns; column++)
                {
                    if (column > 0)
                    {
                        text.Append(' ');
                    }

                    text.Append(array2Dr[row, column] ? "true" : "false");
                }
            }

            _text.text = text.ToString();
        }
    }
}

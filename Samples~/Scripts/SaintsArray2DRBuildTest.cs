using System.Text;
using SaintsField.Playa;
using TMPro;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public partial class SaintsArray2DRBuildTest : SaintsMonoBehaviour
    {
        public SaintsArray2DR<bool> array2Dr;
        [SaintsSerialized] public bool[,] array2DrS;
        [SerializeField, GetComponent] private TMP_Text _text;

        private void Awake()
        {
            StringBuilder text = new StringBuilder();

            Render(text, array2Dr);

            text.Append("\n------------\n");

            Render(text, array2DrS);

            _text.text = text.ToString();
        }

        private static void Render(StringBuilder text, bool[,] source)
        {
            int rows = source.GetLength(0);
            int columns = source.GetLength(1);
            for (int row = 0; row < rows; row++)
            {
                if (row > 0)
                {
                    text.Append(",\n");
                }

                for (int column = 0; column < columns; column++)
                {
                    if (column > 0)
                    {
                        text.Append(' ');
                    }

                    text.Append(source[row, column] ? "true" : "false");
                }
            }
        }
    }
}

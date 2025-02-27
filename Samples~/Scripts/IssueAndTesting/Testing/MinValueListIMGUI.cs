using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Testing
{
    public class MinValueListIMGUI : MonoBehaviour
    {
        [MinValue(nameof(SelectiveError))] public int[] number;

        private object SelectiveError(int _, int index)
        {
            if (index % 2 == 0)
            {
                return 2;
            }

            return "return not a number";
        }
    }
}

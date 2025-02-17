using System.Collections;
using System.Linq;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class IEButtons : MonoBehaviour
    {
        [AboveButton(nameof(IncrValue))]
        [BelowButton(nameof(IncrValue))]
        [PostFieldButton(nameof(IncrValue), "P")]
        public int value;

        private IEnumerator IncrValue()
        {
            foreach (int num in Enumerable.Range(0, 200))
            {
                value = num;
                yield return null;
            }
        }
    }
}

using System.Collections;
using System.Linq;
using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public class IEButton : SaintsMonoBehaviour
    {
        public int value;

        [Button]
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

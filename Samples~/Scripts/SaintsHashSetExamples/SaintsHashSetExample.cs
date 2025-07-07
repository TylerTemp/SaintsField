using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsHashSetExamples
{
    public class SaintsHashSetExample : MonoBehaviour
    {
        public SaintsHashSet<string> stringHashSet;

        [SaintsHashSet(numberOfItemsPerPage: 5)]
        public SaintsHashSet<int> integerHashSet = new SaintsHashSet<int>
        {
            1, 2, 3, 4, 5, 6, 7, 8, 9, 0,
        };
    }
}

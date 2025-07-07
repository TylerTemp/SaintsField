using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsHashSetExamples
{
    public class SaintsHashSetExample : MonoBehaviour
    {
        public SaintsHashSet<int> intHashSet = new SaintsHashSet<int>
        {
            1, 2, 3, 4, 5,
        };
    }
}

using UnityEngine;

namespace SaintsField.Samples.Scripts.Interface
{
    public class MultipleMono : MonoBehaviour, IMultiple
    {
        public string displayName;

        public override string ToString()
        {
            return displayName;
        }
    }
}

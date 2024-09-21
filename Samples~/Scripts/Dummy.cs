using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class Dummy: MonoBehaviour, IDummy
    {
        public string comment;
        public string GetComment() => comment;

        [SerializeField]
        private Transform targetTransform;

        private Transform GetTargetTransform() => targetTransform;
    }
}

using UnityEditor;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class Dummy: MonoBehaviour, IDummy
    {
        public string comment;
        public string GetComment() => comment;

        [SerializeField]
        [BelowButton(nameof(ReadCopy))]
        private Transform targetTransform;

        private Transform GetTargetTransform() => targetTransform;

        public override string ToString()
        {
            return $"<Dummy {comment}/>";
        }

        private void ReadCopy()
        {
            string clipboardData = EditorGUIUtility.systemCopyBuffer;
            Debug.Log("Clipboard Data: " + clipboardData);
        }
    }
}

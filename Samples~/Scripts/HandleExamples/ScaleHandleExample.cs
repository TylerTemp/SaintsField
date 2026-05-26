using UnityEngine;

namespace SaintsField.Samples.Scripts.HandleExamples
{
    public class ScaleHandleExample : MonoBehaviour
    {
        // [ScaleHandle] public Vector3 localScale3 = Vector3.one;
        // [ScaleHandle] public Vector2 localScale2 = Vector2.one;
        // [ScaleHandle] public Vector3Int localScale3Int = Vector3Int.one;
        // [ScaleHandle] public float uniformScale = 1f;

        public Transform root;

        // use callback target as handle parent space
        [ScaleHandle(nameof(root))]
        [OnValueChanged(nameof(ApplyScaleToChild))]
        public Vector3 scaleInside = Vector3.one;

        public Transform applyTo;
        private void ApplyScaleToChild(Vector3 scale)
        {
            applyTo.transform.localScale = scale;
        }
    }
}

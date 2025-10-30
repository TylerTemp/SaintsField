using SaintsField.Playa;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Issues
{
    public class Issue317Layout : SaintsMonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("Axis the button will travel on in local space.")]
        public int Axis;
        [Tooltip("Rigidbody this button will connect to with a joint.")]
        public Rigidbody ConnectedBody;
        [Tooltip("Spring value of the joint")]
        public float Spring = 1000f;
        [Tooltip("Damper of the joint")]
        public float Damper = 50f;
        [Header("Button Positions")]
        [Tooltip("How far the button must travel to become pressed.")]
        public float DownThreshold;
        [Tooltip("Threshold to hit on the return to allow the button to be pressed again.")]
        public float ResetThreshold;
        [Tooltip("The resting position of the button")]
        public Vector3 StartPosition;
        [Tooltip("Furthest position the button can travel")]
        public Vector3 EndPosition;
        [Header("SFX")]
        public AudioClip SFXButtonDown;
        public AudioClip        SFXButtonUp;
        public string ButtonDown;
        public string ButtonUp;
        [Header("Debug")]
        public bool IsPressed;
        public bool       InvokeButtonDown;
        public bool       UpdateSpring;
        public Rigidbody  Rigidbody {get;private set;}
        Vector3           _axis;
        ConfigurableJoint _joint;
        ConfigurableJoint _limitJoint;
        [Header("MC Detection For Safety")]
        public Vector3 safetyDetectionOffset;
        public float safetyDetectionRadius = .5f;

#if UNITY_EDITOR
        [InfoBox("1. Choose the local axis the button will move on.2. Save the start position of the button.3. Save the end position of the button.4. Save the down and reset positions. 5. Return the transform to start by pressing the return button.6. If the Connected Body is left blank, the button will be jointed to the world and cannot be moved.")]
        // [ShowInInspector]
        Vector3 dir => default;

        [LayoutStart("start",ELayout.Horizontal)]
        [Button]
        public void SaveStart() {}

        [Button]
        public void GotoStart()
        {
            transform.localPosition = default;
            EditorUtility.SetDirty(transform); //only one needed to restore prefab to proper position
        }

        [LayoutStart("end",ELayout.Horizontal)]
        [Button]
        public void SaveEnd()
        {
        }

        [Button]
        public void GotoEnd()
        {
        }

        [LayoutStart("threshold down",ELayout.Horizontal)]
        [Button]
        public void SaveDown() {}

        [Button]
        public void GotoDown()
        {
        }

        [LayoutStart("threshold reset",ELayout.Horizontal)]
        [Button]
        public void SaveReset() {}

        [Button]
        public void GotoReset() {}
#endif
    }
}

// ReSharper disable UnusedMember.Global
using UnityEngine;

namespace SaintsField.Samples.Scripts.HandleExamples.RotateHandleExample
{
    public class RotationHandleTargetVExample : MonoBehaviour
    {
        [RotationHandle]  // make a target always show rotation handle by default
        [GetInChildren]
        public GameObject go;

        // make a vector3 rotation as a local rotation inside go
        [RotationHandle(nameof(go), posYOffset: 2)]
        [OnValueChanged(nameof(ApplyRotToChild))]  // let's see the changes in real time
        public Quaternion rotationInside;

        public Transform applyTo;
        private void ApplyRotToChild(Quaternion rotation)
        {
            // Debug.Log(rotation);
            applyTo.transform.localRotation = rotation;
        }

        [RotationHandle(posXOffset: 1)] public float rotationZFor2D;  // in 2D game usually only z axis works

        // lets lock x, z to 0 using MinValue/MaxValue
        [MinValue(position0: 0, position2: 0)]
        [MaxValue(position0: 0, position2: 0)]
        [RotationHandle(posXOffset: 1, posYOffset: 1)]
        public Quaternion quaternionLock;
    }
}

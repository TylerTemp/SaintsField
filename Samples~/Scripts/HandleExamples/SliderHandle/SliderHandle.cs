using UnityEngine;

namespace SaintsField.Samples.Scripts.HandleExamples.SliderHandle
{
    public class SliderHandle : MonoBehaviour
    {
        [SliderHandle] public float defaultLength = 1f;

        [Space]
        [GetInChildren] public GameObject anotherSpace;

        // Make it in local space of another object
        // and change the direction too.
        // `vector` types will get all axis the same value
        [SliderHandle(space: nameof(anotherSpace), directionX: 1, directionY: 1, eColor: EColor.Green)] public Vector2 upShot;
    }
}

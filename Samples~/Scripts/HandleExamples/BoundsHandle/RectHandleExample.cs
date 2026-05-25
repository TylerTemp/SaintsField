using UnityEngine;

namespace SaintsField.Samples.Scripts.HandleExamples.BoundsHandle
{
    public class RectHandleExample : MonoBehaviour
    {
        [PrimitiveBoundsHandle] public Rect rect;
        [PrimitiveBoundsHandle(eColor: EColor.Aqua)] public RectInt rectInt;
    }
}

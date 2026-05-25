using UnityEngine;

namespace SaintsField.Samples.Scripts.HandleExamples.BoundsHandle
{
    public class BoundsHandleExample : MonoBehaviour
    {
        [PrimitiveBoundsHandle] public Bounds bounds;
        [PrimitiveBoundsHandle(eColor: EColor.Aqua)] public BoundsInt boundsInt;
    }
}

using UnityEngine;

namespace SaintsField.Samples.Scripts.HandleExamples.RadiusHandle
{
    public class RadiusWorldParent : MonoBehaviour
    {
        [RadiusHandle(space: null, eColor: EColor.Brown)] public float floatWorldRadius = 1;
        [RadiusHandle] public float floatLocalRadius = 1;
    }
}

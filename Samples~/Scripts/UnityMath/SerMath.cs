using SaintsField;
using SaintsField.Playa;
using Unity.Mathematics;


namespace Samples.Scripts.UnityMath
{
    public class SerMath : SaintsMonoBehaviour
    {
        public float2 f2;

        [ShowInInspector] private float2 _f2 => f2;

        public float3 f3;

        public float2x2 f2x2;
        public float3x4 f3x4;
        public float4x4 f4x4;

        public quaternion quat;
    }
}

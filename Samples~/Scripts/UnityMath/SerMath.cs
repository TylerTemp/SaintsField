using SaintsField;
using SaintsField.Playa;
using Unity.Mathematics;
// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming

namespace Samples.Scripts.UnityMath
{
    public class SerMath : SaintsMonoBehaviour
    {
        [LayoutStart("bool", ELayout.CollapseBox)]

        public bool2 b2;

        [ShowInInspector]
        private bool2 B2
        {
            get => b2;
            set => b2 = value;
        }

        public bool3 b3;

        [ShowInInspector]
        private bool3 B3
        {
            get => b3;
            set => b3 = value;
        }

        public bool4 b4;

        [ShowInInspector]
        private bool4 B4
        {
            get => b4;
            set => b4 = value;
        }

        [LayoutStart("bool x", ELayout.CollapseBox)]

        public bool2x2 b2x2;
        [ShowInInspector]
        private bool2x2 B2x2
        {
            get => b2x2;
            set => b2x2 = value;
        }

        public bool2x3 b2x3;
        [ShowInInspector]
        private bool2x3 B2x3
        {
            get => b2x3;
            set => b2x3 = value;
        }

        public bool2x4 b2x4;
        [ShowInInspector]
        private bool2x4 B2x4
        {
            get => b2x4;
            set => b2x4 = value;
        }

        public bool3x2 b3x2;
        [ShowInInspector]
        private bool3x2 B3x2
        {
            get => b3x2;
            set => b3x2 = value;
        }

        public bool3x3 b3x3;
        [ShowInInspector]
        private bool3x3 B3x3
        {
            get => b3x3;
            set => b3x3 = value;
        }

        public bool3x4 b3x4;
        [ShowInInspector]
        private bool3x4 B3x4
        {
            get => b3x4;
            set => b3x4 = value;
        }

        public bool4x2 b4x2;
        [ShowInInspector]
        private bool4x2 B4x2
        {
            get => b4x2;
            set => b4x2 = value;
        }

        public bool4x3 b4x3;
        [ShowInInspector]
        private bool4x3 B4x3
        {
            get => b4x3;
            set => b4x3 = value;
        }

        public bool4x4 b4x4;
        [ShowInInspector]
        private bool4x4 B4x4
        {
            get => b4x4;
            set => b4x4 = value;
        }

        [LayoutStart("double", ELayout.CollapseBox)]

        public double2 d2;
        [ShowInInspector]
        private double2 D2
        {
            get => d2;
            set => d2 = value;
        }

        public double3 d3;
        [ShowInInspector]
        private double3 D3
        {
            get => d3;
            set => d3 = value;
        }

        public double4 d4;
        [ShowInInspector]
        private double4 D4
        {
            get => d4;
            set => d4 = value;
        }

        [LayoutStart("double x", ELayout.CollapseBox)]

        public double2x2 d2x2;
        [ShowInInspector]
        private double2x2 D2x2
        {
            get => d2x2;
            set => d2x2 = value;
        }

        public double2x3 d2x3;
        [ShowInInspector]
        private double2x3 D2x3
        {
            get => d2x3;
            set => d2x3 = value;
        }

        public double2x4 d2x4;
        [ShowInInspector]
        private double2x4 D2x4
        {
            get => d2x4;
            set => d2x4 = value;
        }

        public double3x2 d3x2;
        [ShowInInspector]
        private double3x2 D3x2
        {
            get => d3x2;
            set => d3x2 = value;
        }

        public double3x3 d3x3;
        [ShowInInspector]
        private double3x3 D3x3
        {
            get => d3x3;
            set => d3x3 = value;
        }

        public double3x4 d3x4;
        [ShowInInspector]
        private double3x4 D3x4
        {
            get => d3x4;
            set => d3x4 = value;
        }

        public double4x2 d4x2;
        [ShowInInspector]
        private double4x2 D4x2
        {
            get => d4x2;
            set => d4x2 = value;
        }

        public double4x3 d4x3;
        [ShowInInspector]
        private double4x3 D4x3
        {
            get => d4x3;
            set => d4x3 = value;
        }

        public double4x4 d4x4;
        [ShowInInspector]
        private double4x4 D4x4
        {
            get => d4x4;
            set => d4x4 = value;
        }

        [LayoutStart("float", ELayout.CollapseBox)]

        public float2 f2;

        [ShowInInspector] private float2 F2
        {
            get => f2;
            set => f2 = value;
        }

        public float3 f3;

        [ShowInInspector]
        private float3 F3
        {
            get => f3;
            set => f3 = value;
        }

        public float4 f4;

        [ShowInInspector]
        private float4 F4
        {
            get => f4;
            set => f4 = value;
        }

        [LayoutStart("float x", ELayout.CollapseBox)]

        public float2x2 f2x2;

        [ShowInInspector]

        private float2x2 F2x2
        {
            get => f2x2;
            set => f2x2 = value;
        }


        public float2x3 f2x3;

        [ShowInInspector]

        private float2x3 F2x3
        {
            get => f2x3;
            set => f2x3 = value;
        }


        public float2x4 f2x4;

        [ShowInInspector]

        private float2x4 F2x4
        {
            get => f2x4;
            set => f2x4 = value;
        }


        public float3x2 f3x2;

        [ShowInInspector]

        private float3x2 F3x2
        {
            get => f3x2;
            set => f3x2 = value;
        }


        public float3x3 f3x3;

        [ShowInInspector]

        private float3x3 F3x3
        {
            get => f3x3;
            set => f3x3 = value;
        }


        public float3x4 f3x4;

        [ShowInInspector]

        private float3x4 F3x4
        {
            get => f3x4;
            set => f3x4 = value;
        }


        public float4x2 f4x2;

        [ShowInInspector]

        private float4x2 F4x2
        {
            get => f4x2;
            set => f4x2 = value;
        }


        public float4x3 f4x3;

        [ShowInInspector]

        private float4x3 F4x3
        {
            get => f4x3;
            set => f4x3 = value;
        }


        public float4x4 f4x4;

        [ShowInInspector]

        private float4x4 F4x4
        {
            get => f4x4;
            set => f4x4 = value;
        }

        [LayoutStart("half", ELayout.FoldoutBox)]

        public half h;

        [ShowInInspector]
        private half H
        {
            get => h;
            set => h = value;
        }

        public half2 h2;

        [ShowInInspector]
        private half2 H2
        {
            get => h2;
            set => h2 = value;
        }

        [Button]
        private void Rand()
        {
            h2 = new half2(UnityEngine.Random.Range(0f, 1f));
        }

        public half3 h3;

        [ShowInInspector]
        private half3 H3
        {
            get => h3;
            set => h3 = value;
        }

        public half4 h4;

        [ShowInInspector]
        private half4 H4
        {
            get => h4;
            set => h4 = value;
        }
    }
}

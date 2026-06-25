using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing.LabelTextForRawFallback
{
    public class LabelTextForRawFallbackExample : SaintsMonoBehaviour
    {
        [Serializable]
        public enum RawEnum
        {
            None,
            One,
            Two,
        }

        [Serializable]
        public struct RawStruct
        {
            public string text;
            public int number;
            public GameObject go;
        }

        [Serializable]
        public class RawManagedBase
        {
            public string baseText;
        }

        [Serializable]
        public class RawManagedChild : RawManagedBase
        {
            public int childNumber;
        }

        [Serializable]
        public class RawImGuiType
        {
            public string text;
            public int number;
        }

        [LabelText("<color=brown>cusType<icon=star.png/>")]
        public RawImGuiType cusType;

        [LabelText("<color=brown>arr<icon=star.png/>")]
        public string[] arr;

        [LabelText("<color=brown>string<icon=star.png/>")]
        public string text;

        [LabelText("<color=brown>struct<icon=star.png/>")]
        public RawStruct rawStruct;

        [LabelText("<color=brown>custom IMGUI drawer<icon=star.png/>")]
        public RawImGuiType rawImGuiType = new RawImGuiType();

        [SerializeReference, LabelText("<color=brown>managed reference<icon=star.png/>")]
        public RawManagedBase managedReference = new RawManagedChild();

        [LabelText("<color=brown>int<icon=star.png/>")]
        public int intValue;

        [LabelText("<color=brown>byte<icon=star.png/>")]
        public byte byteValue = 1;

        [LabelText("<color=brown>sbyte<icon=star.png/>")]
        public sbyte sbyteValue = -1;

        [LabelText("<color=brown>short<icon=star.png/>")]
        public short shortValue = -5;

        [LabelText("<color=brown>ushort<icon=star.png/>")]
        public ushort ushortValue = 5;

        [LabelText("<color=brown>uint<icon=star.png/>")]
        public uint uintValue = 5;

        [LabelText("<color=brown>long<icon=star.png/>")]
        public long longValue = long.MaxValue;

        [LabelText("<color=brown>ulong<icon=star.png/>")]
        public ulong ulongValue = ulong.MaxValue;

        [LabelText("<color=brown>bool<icon=star.png/>")]
        public bool boolValue;

        [LabelText("<color=brown>float<icon=star.png/>")]
        public float floatValue = 0.1234f;

        [LabelText("<color=brown>double<icon=star.png/>")]
        public double doubleValue = 0.1234d;

        [LabelText("<color=brown>char<icon=star.png/>")]
        public char charValue = 'c';

        [LabelText("<color=brown>color<icon=star.png/>")]
        public Color colorValue = Color.red;

        [LabelText("<color=brown>object reference<icon=star.png/>")]
        public GameObject objectReference;

        // [LabelText("<color=brown>exposed reference<icon=star.png/>")]
        // public ExposedReference<GameObject> exposedReference;

        [LabelText("<color=brown>layer mask<icon=star.png/>")]
        public LayerMask layerMask;

        [LabelText("<color=brown>enum<icon=star.png/>")]
        public RawEnum enumValue;

        [LabelText("<color=brown>Vector2<icon=star.png/>")]
        public Vector2 vector2Value = Vector2.one;

        [LabelText("<color=brown>Vector3<icon=star.png/>")]
        public Vector3 vector3Value = Vector3.one;

        [LabelText("<color=brown>Vector4<icon=star.png/>")]
        public Vector4 vector4Value = Vector4.one;

        [LabelText("<color=brown>Quaternion<icon=star.png/>")]
        public Quaternion quaternionValue = Quaternion.identity;

        [LabelText("<color=brown>Vector2Int<icon=star.png/>")]
        public Vector2Int vector2IntValue = Vector2Int.one;

        [LabelText("<color=brown>Vector3Int<icon=star.png/>")]
        public Vector3Int vector3IntValue = Vector3Int.one;

        [LabelText("<color=brown>Rect<icon=star.png/>")]
        public Rect rectValue = new Rect(0, 0, 1, 1);

        [LabelText("<color=brown>RectInt<icon=star.png/>")]
        public RectInt rectIntValue = new RectInt(0, 0, 1, 1);

        [LabelText("<color=brown>Bounds<icon=star.png/>")]
        public Bounds boundsValue = new Bounds(Vector3.one, Vector3.up);

        [LabelText("<color=brown>BoundsInt<icon=star.png/>")]
        public BoundsInt boundsIntValue = new BoundsInt(Vector3Int.one, Vector3Int.up);

        [LabelText("<color=brown>AnimationCurve<icon=star.png/>")]
        public AnimationCurve animationCurveValue = new AnimationCurve();

        [LabelText("<color=brown>Gradient<icon=star.png/>")]
        public Gradient gradientValue = new Gradient();

        [LabelText("<color=brown>Hash128<icon=star.png/>")]
        public Hash128 hash128Value = new Hash128(1, 2, 3, 4);
    }
}


using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue412TooltipFallDraw : SaintsMonoBehaviour
    {
        [Tooltip("Arr")]
        public int[] arr;

        public interface IMyInter
        {
            public string name { get; set; }
        }

        [Serializable]
        public class MyInter : IMyInter
        {
            [field: SerializeField]
            public string name { get; set; }
        }

#if UNITY_2021_3_OR_NEWER
        [Tooltip("ManagedReference")]
        [SerializeReference] public IMyInter serRef;
#endif

        [Tooltip("Generic")]
        public MyInter myInter;

        [Tooltip("long")] public long myLong;
        [Tooltip("ulong")] public ulong myULong;
        [Tooltip("int")] public int myInt;
        [Tooltip("uint")] public int myUint;
        [Tooltip("sbyte")] public sbyte mySbyte;
        [Tooltip("byte")] public sbyte myByte;
        [Tooltip("short")] public short shortTest;
        [Tooltip("ushort")] public ushort ushortTest;
        [Tooltip("Boolean")] public bool booleanTest;
        [Tooltip("double")] public double doubleTest;
        [Tooltip("float")] public double floatTest;
        [Tooltip("String")] public string stringTest;
        [Tooltip("Color")] public Color myColor;
        [Tooltip("ObjectReference")] public UnityEngine.Object myObjectReference;
        [Tooltip("LayerMask")] public LayerMask myLayerMask;

        [Serializable]
        public enum Mode
        {
            LocalPivotPlusOffset, TwoLocalPivots,
            WorldPivotPlusOffset, TwoWorldPivots
        }
        [Tooltip("enum")] public Mode myEnum;

        [Serializable, Flags]
        public enum ModeFlags
        {
            LocalPivotPlusOffset, TwoLocalPivots,
            WorldPivotPlusOffset, TwoWorldPivots
        }
        [Tooltip("flags")] public ModeFlags myFlags;

        [Tooltip("Vector2")] public Vector2 myVector2;
        [Tooltip("Vector2Int")] public Vector2Int myVector2Int;
        [Tooltip("Vector3")] public Vector3 myVector3;
        [Tooltip("Vector3Int")] public Vector3Int myVector3Int;
        [Tooltip("Vector4")] public Vector4 myVector4;
        [Tooltip("Rect")] public Rect myRect;
        [Tooltip("RectInt")] public RectInt myRectInt;
        [Tooltip("Character")] public char myCharacter;
        [Tooltip("AnimationCurve")] public AnimationCurve myAnimationCurve;
        [Tooltip("Bounds")] public Bounds myBounds;
        [Tooltip("BoundsInt")] public BoundsInt myBoundsInt;
        [Tooltip("Gradient")] public Gradient myGradient;
        [Tooltip("Quaternion")] public Quaternion myQuaternion;
        [Tooltip("Hash128")] public Hash128 myHash128;
        [Tooltip("ExposedReference")] public ExposedReference<Material> exposedRef;

    }
}

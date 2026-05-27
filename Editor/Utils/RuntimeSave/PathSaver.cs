using System;
using UnityEngine;

namespace SaintsField.Editor.Utils.RuntimeSave
{
    [Serializable]
    public struct PathSaver
    {
        public
#if UNITY_6000_4_OR_NEWER
            EntityId
#else
            int
#endif
            targetInstanceId;

        // public UnityEngine.Object propertyObject;
        public string globalObjectIdString;
        public string scenePath;

        public string propertyPath;
        public SaverPropertyType propertyType;

        public int intValue;
        public uint uintValue;
        public long longValue;
        public ulong ulongValue;
        public bool boolValue;
        public float floatValue;
        public double doubleValue;
        public string stringValue;
        // public object boxedValue;
        public Color colorValue;
        public UnityEngine.Object objectReferenceValue;
        public bool objectReferenceValueIsNull;
        public int objectReferenceInstanceIDValue;
        public int enumValueIndex;
        public int enumValueFlag;
        public Vector2 vector2Value;
        public Vector3 vector3Value;
        public Vector4 vector4Value;
        public Rect rectValue;
        // public int arraySize;
        public AnimationCurve animationCurveValue;
        public Bounds boundsValue;
        public Gradient gradientValue;
        public Quaternion quaternionValue;
        public UnityEngine.Object exposedReferenceValue;
        public Vector2Int vector2IntValue;
        public Vector3Int vector3IntValue;
        public RectInt rectIntValue;
        public BoundsInt boundsIntValue;
        // public long managedReferenceId;
        // public string managedReferenceFullTypename;
        // public string managedReferenceFieldTypename;
        // public object managedReferenceValue;
        public Hash128 hash128Value;
    }
}

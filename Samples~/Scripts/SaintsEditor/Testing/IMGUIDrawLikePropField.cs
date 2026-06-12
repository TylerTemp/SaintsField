using System;
using System.Collections.Generic;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class IMGUIDrawLikePropField :
        SaintsMonoBehaviour
        // MonoBehaviour
    {
        public Vector2 v2;

        public Gradient gradient = new Gradient();
        public AnimationCurve curve = new AnimationCurve();

        public Hash128 hash128Value = new Hash128(1, 2, 3, 4);

        public readonly List<int> ReadonlyList = new List<int>();

        [Serializable]
         public enum MyEnum
         {
             None,
             One,
             Two,
         }

        [Serializable]
        public class MyClass
        {
            public string myString;
            public GameObject myObj;
            public MyEnum myEnum;
        }

        public bool boolV;
        public byte byteV;
        public sbyte sbyteV;
#pragma warning disable CS0414 // Field is assigned but its value is never used
        public short shortV = -5;
        public ushort ushortV = 0;
        public long longV = long.MaxValue;
        public ulong uLongV = ulong.MaxValue;
        public float floatV = 0.1234f;
        public double doubleV = 0.1234d;
        public string stringV = "Hi";
        public char charV = 'c';
#pragma warning restore CS0414 // Field is assigned but its value is never used
        public Vector2 vector2V = Vector2.one;
        public Vector3 vector3V = Vector3.one;
        public Vector4 vector4V = Vector4.one;
        public Vector2Int vector2VInt = Vector2Int.one;
        public Vector3Int vector3VInt = Vector3Int.one;
        public Color colorV = Color.red;
        public Bounds boundsV = new Bounds(Vector3.one, Vector3.up);
        public BoundsInt boundsIntV = new BoundsInt(Vector3Int.one, Vector3Int.up);
        public Rect rectV = new Rect(0, 0, 1, 1);
        public RectInt rectIntV = new RectInt(0, 0, 1, 1);
        public MyEnum myEnum;
        public GameObject go;
        public Transform trans;
        public Scriptable so;
        // public void SetDummy() => _dummy = _so;

        public MyClass myClass;

        [Serializable]
        public struct MyStruct
        {
            public string myString;
        }

        public MyStruct myStruct;

        public Color[] colors = {Color.red, Color.green, Color.blue};

    }
}

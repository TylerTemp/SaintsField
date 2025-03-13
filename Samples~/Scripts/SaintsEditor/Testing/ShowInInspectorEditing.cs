using System.Collections.Generic;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class ShowInInspectorEditing : SaintsMonoBehaviour
    {
        private enum MyEnum
        {
            None,
            One,
            Two,
        }

        [ShowInInspector] private bool _boolV;
        [ShowInInspector] private byte _byteV;
        [ShowInInspector] private sbyte _sbyteV;
        [ShowInInspector] private short _shortV = -5;
        [ShowInInspector] private ushort _ushortV = 0;
        [ShowInInspector] private long _longV = long.MaxValue;
        [ShowInInspector] private ulong _uLongV = ulong.MaxValue;
        [ShowInInspector] private float _floatV = 0.1234f;
        [ShowInInspector] private double _doubleV = 0.1234d;
        [ShowInInspector] private string _stringV = "Hi";
        [ShowInInspector] private Vector2 _vector2V = Vector2.one;
        [ShowInInspector] private Vector3 _vector3V = Vector3.one;
        [ShowInInspector] private Vector4 _vector4V = Vector4.one;
        [ShowInInspector] private Vector2Int _vector2VInt = Vector2Int.one;
        [ShowInInspector] private Vector3Int _vector3VInt = Vector3Int.one;
        [ShowInInspector] private Color _colorV = Color.red;
        [ShowInInspector] private Bounds _boundsV = new Bounds(Vector3.one, Vector3.up);
        [ShowInInspector] private Rect _rectV = new Rect(0, 0, 1, 1);
        [ShowInInspector] private RectInt _rectIntV = new RectInt(0, 0, 1, 1);
        [ShowInInspector] private MyEnum _myEnum;
        [ShowInInspector] private GameObject _go;
        [ShowInInspector] private Transform _trans;
        [ShowInInspector] private Scriptable _so;

        private class MyClass
        {
            public string MyString;
        }

        [ShowInInspector] private MyClass _myClass;
        [ShowInInspector] private MyClass _myClassD = new MyClass
        {
            MyString = "Hi",
        };

        private struct MyStruct
        {
            public string MyString;
        }

        [ShowInInspector] private MyStruct _myStruct;

        [ShowInInspector] private Color[] _colors = {Color.red, Color.green, Color.blue};

        [ShowInInspector] private Dictionary<string, Color> _dictColors = new Dictionary<string, Color>
        {
            { "Red", Color.red },
            { "Green", Color.green },
            { "Blue", Color.blue },
        };
    }
}

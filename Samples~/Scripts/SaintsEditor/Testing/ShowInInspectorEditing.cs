using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Playa;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

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

        private class MyClass
        {
            public string MyString;
            public GameObject MyObj;
            private MyEnum _myEnum;
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
        // private void SetDummy() => _dummy = _so;

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
        [ShowInInspector, Ordered] private Color[] _colorEmptyArray;

        [Button, Ordered]
        private void ArrayToNull()
        {
            _colorEmptyArray = null;
        }

        [Button, Ordered]
        private void ArrayChange0ToRed()
        {
            _colorEmptyArray[0] = Color.red;
        }

        [Button, Ordered]
        private void ArraySwap()
        {
            (_colorEmptyArray[0], _colorEmptyArray[1]) = (_colorEmptyArray[1], _colorEmptyArray[0]);
        }

        [ShowInInspector, Ordered] private List<Color> _colorEmptyList;
        [Button, Ordered]
        private void ListToNull() => _colorEmptyList = null;
        [Button, Ordered]
        private void ListChangeColor()
        {
            _colorEmptyList[0] = Color.red;
        }
        [Button, Ordered]
        private void ListChangeSize()
        {
            _colorEmptyList.Add(Color.blue);
        }

        [ShowInInspector, Ordered] private MyClass[] _myClasses;

        [ShowInInspector] private Dictionary<string, Color> _dictColors = new Dictionary<string, Color>
        {
            { "Red", Color.red },
            { "Green", Color.green },
            { "Blue", Color.blue },
        };

        private interface Inter
        {
            int MyInt { get; set; }
        }

        public class InterClass : Inter
        {
            public int MyInt { get; set; }
            public string InterClassField;
        }

        public struct InterStruct : Inter
        {
            public string InterStructField;
            public int MyInt { get; set; }
        }

        [ShowInInspector] private static Inter _inter;

        public class GeneralDummyClass: IDummy
        {
            public string GetComment()
            {
                return "DummyClass";
            }

            public int MyInt { get; set; }
            public int GenDumInt;
            public string GenDumString;
        }

        [ShowInInspector] private static IDummy _dummy;

        [Button]
        private void DebugDummy() => Debug.Log(_dummy);

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            string g = AssetDatabase.FindAssets("t:Scriptable").FirstOrDefault();
            if (!string.IsNullOrEmpty(g))
            {
                _dummy = AssetDatabase.LoadAssetAtPath<Scriptable>(AssetDatabase.GUIDToAssetPath(g));
            }
        }
#endif

        public struct NullSwap
        {
            public int Number;
        }

        [NonSerialized, ShowInInspector] public NullSwap _nullSwap;

        [ShowInInspector]
        private IReadOnlyDictionary<int, string> _readOnlyDict =
            new Dictionary<int, string>();

        [ShowInInspector]
        private IEnumerable<int> _ie = Enumerable.Range(0, 3);

        private class MyList : IReadOnlyList<int>
        {
            public int[] Lis;

            public IEnumerator<int> GetEnumerator()
            {
                if (Lis == null)
                {
                    yield break;
                }

                foreach (int i in Lis)
                {
                    yield return i;
                }
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public int Count => Lis?.Length ?? 0;

            public int this[int index] => Lis[index];
        }

        [ShowInInspector] private MyList _myListNull;
        [ShowInInspector] private MyList _myListSome = new MyList { Lis = new[] { 1, 2 } };
    }
}

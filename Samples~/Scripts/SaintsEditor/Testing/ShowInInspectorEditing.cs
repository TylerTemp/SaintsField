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
        private class TestPrivateFieldDummy
            : IDummy
        {
            private string _comment;

            public string GetComment() => _comment;

            public int MyInt { get; private set; }
        }

        [ShowInInspector] private TestPrivateFieldDummy _testPrivateFieldDummy;

        [ShowInInspector]
        private readonly List<int> _readonlyList = new List<int>();

         public enum MyEnum
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

        [ShowInInspector, Ordered] private bool _boolV;
        [ShowInInspector, Ordered] private byte _byteV;
        [ShowInInspector, Ordered] private sbyte _sbyteV;
        [ShowInInspector, Ordered] private short _shortV = -5;
        [ShowInInspector, Ordered] private ushort _ushortV = 0;
        [ShowInInspector, Ordered] private long _longV = long.MaxValue;
        [ShowInInspector, Ordered] private ulong _uLongV = ulong.MaxValue;
        [ShowInInspector, Ordered] private float _floatV = 0.1234f;
        [ShowInInspector, Ordered] private double _doubleV = 0.1234d;
        [ShowInInspector, Ordered] private string _stringV = "Hi";
        [ShowInInspector, Ordered] private char _charV = 'c';
        [ShowInInspector, Ordered] private Vector2 _vector2V = Vector2.one;
        [ShowInInspector, Ordered] private Vector3 _vector3V = Vector3.one;
        [ShowInInspector, Ordered] private Vector4 _vector4V = Vector4.one;
        [ShowInInspector, Ordered] private Vector2Int _vector2VInt = Vector2Int.one;
        [ShowInInspector, Ordered] private Vector3Int _vector3VInt = Vector3Int.one;
        [ShowInInspector, Ordered] private Color _colorV = Color.red;
        [ShowInInspector, Ordered] private Bounds _boundsV = new Bounds(Vector3.one, Vector3.up);
        [ShowInInspector, Ordered] private Rect _rectV = new Rect(0, 0, 1, 1);
        [ShowInInspector, Ordered] private RectInt _rectIntV = new RectInt(0, 0, 1, 1);
        [ShowInInspector, Ordered] private MyEnum _myEnum;
        [ShowInInspector, Ordered] private GameObject _go;
        [ShowInInspector, Ordered] private Transform _trans;
        [ShowInInspector, Ordered] private Scriptable _so;
        // private void SetDummy() => _dummy = _so;

        [ShowInInspector, Ordered] private MyClass _myClass;
        [ShowInInspector, Ordered] private MyClass _myClassD = new MyClass
        {
            MyString = "Hi",
        };

        private struct MyStruct
        {
            public string MyString;
        }

        [ShowInInspector, Ordered] private MyStruct _myStruct;

        [ShowInInspector, Ordered] private Color[] _colors = {Color.red, Color.green, Color.blue};
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

        [ShowInInspector, Ordered] private Dictionary<string, Color> _dictColors = new Dictionary<string, Color>
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

        [ShowInInspector, Ordered] private static Inter _inter;

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

        [ShowInInspector, Ordered] private static IDummy _dummy;

        [Button, Ordered]
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

        [ShowInInspector, Ordered]
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

        [ShowInInspector, Ordered] private MyList _myListNull;
        [ShowInInspector, Ordered] private MyList _myListSome = new MyList { Lis = new[] { 1, 2 } };

        [Button, Ordered]
        private void DictExternalAdd()
        {
            _myDictionaryNull["External"] = 1;
        }
        [ShowInInspector, Ordered] private Dictionary<string, int> _myDictionaryNull;


        [ShowInInspector, Ordered] public Dictionary<MyEnum, int> MyDictionary = new Dictionary<MyEnum, int>
        {
            {MyEnum.One, 1},
        };

        [ShowInInspector, Ordered]
        private IReadOnlyDictionary<int, string> _readOnlyDict =
            new Dictionary<int, string>
            {
                {1, "One"},
                {2, "Two"},
            };

        [ShowInInspector, Ordered] public Dictionary<int, int[]> _intToInts = new Dictionary<int, int[]>();

        public struct KeyStruct: IEquatable<KeyStruct>
        {
            public int Key;

            public bool Equals(KeyStruct other)
            {
                return Key == other.Key;
            }

            public override bool Equals(object obj)
            {
                return obj is KeyStruct other && Equals(other);
            }

            public override int GetHashCode()
            {
                return Key;
            }

            public override string ToString()
            {
                return $"<KeyStruct Key={Key}/>";
            }
        }

        [ShowInInspector] private Dictionary<KeyStruct, int> _keyStructDict = new Dictionary<KeyStruct, int>();
    }
}

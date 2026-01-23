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
        [ShowInInspector] private Gradient _gradient = new Gradient();
        [ShowInInspector] private AnimationCurve _curve = new AnimationCurve();

        [ShowInInspector] private Hash128 _hash128Value = new Hash128(1, 2, 3, 4);

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
        [ShowInInspector] private char _charV = 'c';
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
        [ShowInInspector] private Color[] _colorEmptyArray;

        [Button]
        private void ArrayToNull()
        {
            _colorEmptyArray = null;
        }

        [Button]
        private void ArrayChange0ToRed()
        {
            _colorEmptyArray[0] = Color.red;
        }

        [Button]
        private void ArraySwap()
        {
            (_colorEmptyArray[0], _colorEmptyArray[1]) = (_colorEmptyArray[1], _colorEmptyArray[0]);
        }

        [ShowInInspector] private List<Color> _colorEmptyList;
        [Button]
        private void ListToNull() => _colorEmptyList = null;
        [Button]
        private void ListChangeColor()
        {
            _colorEmptyList[0] = Color.red;
        }
        [Button]
        private void ListChangeSize()
        {
            _colorEmptyList.Add(Color.blue);
        }

        [ShowInInspector] private MyClass[] _myClasses;

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

        [Button]
        private void DictExternalAdd()
        {
            _myDictionaryNull["External"] = 1;
        }
        [ShowInInspector] private Dictionary<string, int> _myDictionaryNull;


        [ShowInInspector] public Dictionary<MyEnum, int> MyDictionary = new Dictionary<MyEnum, int>
        {
            {MyEnum.One, 1},
        };

        [ShowInInspector]
        private IReadOnlyDictionary<int, string> _readOnlyDict =
            new Dictionary<int, string>
            {
                {1, "One"},
                {2, "Two"},
            };

        [ShowInInspector] public Dictionary<int, int[]> _intToInts = new Dictionary<int, int[]>();

        public struct KeyStruct: IEquatable<KeyStruct>
        {
            public int Key;

            public bool Equals(KeyStruct other)
            {
                return Key == other.Key;
            }

            public override bool Equals(object obj)
            {
                // ReSharper disable once Unity.BurstLoadingManagedType
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

        [ShowInInspector, InfoBox("If getter gives error, we display an error box")]
        private int WrongGetter => throw new NotSupportedException("Expected Exception");

        [ShowInInspector, InfoBox("We don't handle if setter gives error")]
        private int WrongSetter  // this will just give errors to console, we won't handle it.
        {
            get => 20;
            set => throw new NotSupportedException("Expected Exception");
        }

        [ShowInInspector, InfoBox("nested field can be error handled too")]
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private IEnumerator _ienumerator = new []{1, 2, 3}.GetEnumerator();

        [LayoutStart("IEnumerator", ELayout.Horizontal)]
        [Button] private void MoveIt() => _ienumerator.MoveNext();
        [LayoutTerminateHere]
        [Button] private void ReCreateIt() => _ienumerator = new []{1, 2, 3}.GetEnumerator();

        private class NestChange
        {
            public int NestedValue;
        }

        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        [ShowInInspector] private NestChange _nestChange = new NestChange();

        [Button]
        private void ChangeNestedValue()
        {
            _nestChange.NestedValue = (_nestChange.NestedValue + 1) % 3;
        }
    }
}

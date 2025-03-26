using System;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class DrawerFallbackFlow : SaintsMonoBehaviour
    {
        [AdvancedDropdown(nameof(LanguageNames))]
        public string curLang;
        private static readonly string[] LanguageNames =
        {
            "Chinese",
            "English",
            "Japanese",
        };

        [Serializable]
        public enum MyEnum
        {
            None,
            [RichLabel("Number1")]
            One,
            Two,
        }
        public bool _boolV;
        public bool[] _boolVs;
        public byte _byteV;
        public sbyte _sbyteV;
        public short _shortV = -5;
        public ushort _ushortV = 0;
        public long _longV = long.MaxValue;
        public ulong _uLongV = ulong.MaxValue;
        public float _floatV = 0.1234f;
        public double _doubleV = 0.1234d;
        public string _stringV = "Hi";
        public Vector2 _vector2V = Vector2.one;
        public Vector3 _vector3V = Vector3.one;
        public Vector4 _vector4V = Vector4.one;
        public Vector2Int _vector2VInt = Vector2Int.one;
        public Vector3Int _vector3VInt = Vector3Int.one;
        public Color _colorV = Color.red;
        public Bounds _boundsV = new Bounds(Vector3.one, Vector3.up);
        public Rect _rectV = new Rect(0, 0, 1, 1);
        public RectInt _rectIntV = new RectInt(0, 0, 1, 1);
        public MyEnum _myEnum;
        public GameObject _go;
        public Transform _trans;
        public Scriptable _so;

        public string f1;

        [InfoBox("Test Info")]
        public string f2;

        [PropRange(0, 10)] public int pRange;

        [EnumToggleButtons] public MyEnum myEnum;

        [Serializable]
        public struct MyStruct
        {
            public string myString;
        }

        public MyStruct myStruct;

        [Serializable]
        public struct SaintsStruct
        {
            [ShowInInspector, Ordered] public string MyString => "My saintsRow!";

            [LayoutStart("Group", ELayout.TitleBox)]
            [Ordered] public string s1;
            [Ordered] public int i1;
            [Ordered] public GameObject go;
        }

        public SaintsStruct structWithoutRaw;
        [InfoBox("Prop without SaintsRow")]
        public SaintsStruct structWithOtherProp;

        [Range(0, 1), InfoBox("Order no longer matters with SaintsEditor")]
        public float f3;
    }
}

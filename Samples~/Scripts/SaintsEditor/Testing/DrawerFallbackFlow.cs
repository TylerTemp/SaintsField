using System;
using SaintsField.Playa;
using UnityEngine;
using UnityEngine.Serialization;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class DrawerFallbackFlow : SaintsMonoBehaviour
    {
        public char character;

        [Serializable]
        public enum EnumFromSome
        {
            First = 1,
            Second = 2,
            Third = 3,
        }

        public EnumFromSome enumFromSome;

        [Serializable, Flags]
        public enum EnumFromSomeFlags
        {
            First = 1,
            Second = 1 << 1,
            Third = 1 << 2,
        }

        public EnumFromSomeFlags enumFromSomeFlags;

        [BelowButton(nameof(Reset))]
        public string s;

        private void Reset()
        {
            enumFromSome = 0;
            enumFromSomeFlags = 0;
        }

        [SepTitle(EColor.Aqua)]
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

        [Space]
        public bool boolV;
        public bool[] boolVs;
        public byte byteV;
        public sbyte sbyteV;
        public short shortV = -5;
        public ushort ushortV = 0;
        public long longV = long.MaxValue;
        public ulong uLongV = ulong.MaxValue;
        public float floatV = 0.1234f;
        public double doubleV = 0.1234d;
        public string stringV = "Hi";
        public Vector2 vector2V = Vector2.one;
        public Vector3 vector3V = Vector3.one;
        public Vector4 vector4V = Vector4.one;
        public Vector2Int vector2VInt = Vector2Int.one;
        public Vector3Int vector3VInt = Vector3Int.one;
        public Color colorV = Color.red;
        public Bounds boundsV = new Bounds(Vector3.one, Vector3.up);
        public Rect rectV = new Rect(0, 0, 1, 1);
        public RectInt rectIntV = new RectInt(0, 0, 1, 1);
        public MyEnum myEnumV;
        public GameObject go;
        public Transform trans;
        public Scriptable so;

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
        public MyStruct[] myStructs;

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
        // this won't work ATM for SaintsRow
        public SaintsStruct[] structWithoutRaws;
        [InfoBox("Prop without SaintsRow")]
        public SaintsStruct structWithOtherProp;

        [Range(0, 1), InfoBox("Order no longer matters with SaintsEditor")]
        public float f3;

        public int[] intArr;
    }
}

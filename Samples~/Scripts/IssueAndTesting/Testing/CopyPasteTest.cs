using System;
using SaintsField.Playa;
using SaintsField.Samples.Scripts.SaintsEditor;
using UnityEngine;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SaintsField.Samples.Scripts.IssueAndTesting.Testing
{
    public class CopyPasteTest : SaintsMonoBehaviour
    {
        // public bool boolV;
        // [LeftToggle] public bool leftToggle;
        //
        // public sbyte sByteV;
        // public byte byteV;
        //
        // public string s;

        [ResizableTextArea]
        public string res;

        [PropRange(0, 10)] public float propR;

        [Layer, PostFieldButton(nameof(SetToUI), "U"), BelowRichLabel("<field/>")] public string layerString;
        private void SetToUI() => layerString = "UIX";
        [Layer, PostFieldButton(nameof(SetToNumber), "U"), BelowRichLabel("<field/>")] public int layerInt;
        private void SetToNumber() => layerInt = -1;

        [Serializable]
        public class MyClass
        {
            public string myString;
        }

        [PostFieldButton("R")]
        [SaintsRow] public MyClass myClass1;
        [SaintsRow] public MyClass myClass2;

        private void R()
        {
            myClass1.myString = $"{Random.Range(0, 99999)}";
        }

        [AdvancedDropdown]
        public Vector2Int v2I;
        [AdvancedDropdown]
        public Vector2 v2;

        [Button]
        private void Paste()
        {
#if UNITY_EDITOR
            // Debug.Log(EditorGUIUtility.systemCopyBuffer);
            res = EditorGUIUtility.systemCopyBuffer;
#endif
        }
    }
}

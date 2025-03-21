using SaintsField.Playa;
using SaintsField.Samples.Scripts.SaintsEditor;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SaintsField.Samples.Scripts.IssueAndTesting.Testing
{
    public class CopyPasteTest : SaintsMonoBehaviour
    {
        public bool boolV;
        [LeftToggle] public bool leftToggle;

        public sbyte sByteV;
        public byte byteV;

        public string s;

        [ResizableTextArea]
        public string res;

        [PropRange(0, 10)] public float propR;

        [Layer, PostFieldButton(nameof(SetToUI), "U"), BelowRichLabel("<field/>")] public string layerString;
        private void SetToUI() => layerString = "UIX";
        [Layer, PostFieldButton(nameof(SetToNumber), "U"), BelowRichLabel("<field/>")] public int layerInt;
        private void SetToNumber() => layerInt = -1;

        [Button]
        private void Paste()
        {
#if UNITY_EDITOR
            Debug.Log(EditorGUIUtility.systemCopyBuffer);
            res = EditorGUIUtility.systemCopyBuffer;
#endif
        }
    }
}

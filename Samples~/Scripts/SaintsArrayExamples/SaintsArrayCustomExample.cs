using System;
using System.Collections.Generic;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsArrayExamples
{
    public class SaintsArrayCustomExample : MonoBehaviour
    {
        // example: using ISaintsArray so you don't need to specify the type name everytime
        [Serializable]
        public class MyList : IWrapProp
        {
            [SerializeField] public List<string> myStrings;

#if UNITY_EDITOR
            public static readonly string EditorPropertyName = nameof(myStrings);
#endif
        }

        [SaintsArray]
        public MyList[] myLis;

        // example: any Serializable which hold a serialized array/list is fine
        [Serializable]
        public struct MyArr
        {
            [FieldLabelText(nameof(MyInnerRichLabel), true)]
            public int[] myArray;

            private string MyInnerRichLabel(object _, int index) => $"<color=pink> Inner [{(char)('A' + index)}]";
        }

        [FieldLabelText(nameof(MyOuterLabel), true), SaintsArray("myArray")]
        public MyArr[] myArr;

        private string MyOuterLabel(object _, int index) => $"<color=Lime> Outer {index}";
    }
}

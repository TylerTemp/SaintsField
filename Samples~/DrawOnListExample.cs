using System.Collections.Generic;
using UnityEngine;

namespace SaintsField.Samples
{
    public class DrawOnListExample : MonoBehaviour
    {
        [System.Serializable]
        public class SimpleObject
        {
            [RichLabel("Normal")] public string myNormalString;
            [field: SerializeField, RichLabel("Sub!")]
            public List<string> mySubStrings { get; private set; }

            public List<string> myNoDecStrings;
        }

        [field: SerializeField, RichLabel("HI"), AboveRichLabel("Above!")]
        public List<SimpleObject> myArrField { get; private set; }

        // [System.Serializable]
        // public class SimpleObject
        // {
        //     public string stringProperty;
        //     public float floatProperty;
        //     public int intProperty;
        // }
        //
        // [EditorGUITable.Table]
        // public List<SimpleObject> simpleObjects;
    }
}

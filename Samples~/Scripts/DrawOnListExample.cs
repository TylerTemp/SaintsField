using System.Collections.Generic;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class DrawOnListExample : MonoBehaviour
    {
        [System.Serializable]
        public class SimpleObject
        {
            [FieldLabelText("Dec")] public string myDecString;
            public string myNormalString;
            [field: SerializeField, FieldLabelText(null), Scene]
            public List<string> mySubStrings { get; private set; }

            public List<string> myNoDecStrings;
        }

        [field: SerializeField, FieldAboveText("This will go on every element :(")]
        public List<SimpleObject> myArrField2 { get; private set; }

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

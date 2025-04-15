using System;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class ListFallFlow : SaintsMonoBehaviour
    {
        [Serializable]
        public struct MyStruct
        {
            [LayoutStart("Main", ELayout.TitleBox)]
            public string s;
            public int i;

            [LayoutEnd]
            [Button]
            public void CallFunc() => Debug.Log($"{s}/{i}");
        }

        // [SaintsRow]
        public MyStruct[] structArr;

        [PropRange(0, 1)]
        public float[] floatArr;
    }
}

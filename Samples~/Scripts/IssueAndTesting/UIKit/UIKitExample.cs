using System;
using UnityEngine;
// using NaughtyAttributes;

namespace SaintsField.Samples.Scripts.UIKit
{
    public class UIKitExample : MonoBehaviour
    {
        // [ShowNonSerializedField] public static readonly Color red = Color.red;
        // [ShowNativeProperty] public Color blue => Color.blue;

        public int normal;
        [UIKitPropDec] public string prop;
        [UIKitPropFix] public string propFix;
        [UiKitTextDec] public string text;

        [Serializable]
        public struct MyStruct
        {
            public string normal;
            [UIKitPropDec] public string prop;
            [UIKitPropFix] public string propFix;
            [UiKitTextDec] public string text;
            [MockKit] public string mockKit;
            [UiKitTextDec] public string mockKitMockKitMockKitMockKitMockKitMockKitMockKitMockKitMockKitMockKitMockKti;
            [MockKit] public string mockKitMockKitMockKitMockKitMockKitMockKitMockKitMockKitMockKitMockKitMockKit;
        }

        [Serializable]
        public struct Nest1
        {
            public string normal;
            [UIKitPropDec] public string prop;
            [UiKitTextDec] public string text;

            public MyStruct myStruct;
            public MyStruct[] myStructList;
        }

        [Serializable]
        public struct Nest2
        {
            public string normal;
            [UIKitPropDec] public string prop;
            [UiKitTextDec] public string text;

            public Nest1 nest1;
            public Nest1[] nest1List;
        }

        public Nest2 testStruct;
        public Nest2[] testStructList;

        // [Button]
        // private void EditorButton()
        // {
        //     Debug.Log("EditorButton");
        // }
    }
}

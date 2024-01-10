using System;
using UnityEngine;
// using NaughtyAttributes;

namespace SaintsField.Samples.Scripts.UIKit
{
    public class UIKitExample2 : MonoBehaviour
    {
        // [ShowNonSerializedField] public static readonly Color red = Color.red;
        // [ShowNativeProperty] public Color blue => Color.blue;

        // public int normal;
        [UIKitPropDec] public string content;

        // [Serializable]
        // public struct MyStruct
        // {
        //     public string normal;
        //
        //     // [AboveRichLabel("<color=green>SaintsField")]
        //     [UIKitPropDec] public string content;
        // }
        //
        // public MyStruct testStruct;

        // [Button]
        // private void EditorButton()
        // {
        //     Debug.Log("EditorButton");
        // }
    }
}

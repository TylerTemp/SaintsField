using System;
using SaintsField.Playa;
using UnityEngine;
// using NaughtyAttributes;

namespace SaintsField.Samples.Scripts.UIKit
{
    public class UIKitExample2 : MonoBehaviour
    {
        // [ShowNonSerializedField] public static readonly Color red = Color.red;
        // [ShowNativeProperty] public Color blue => Color.blue;

        // public int normal;
        public string content;
#if UNITY_2022_2_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
        [UIToolkit]
#endif
        public string contentFixed;

        [field: SerializeField] public string AutoContent { get; private set; }
        [field: SerializeField
#if UNITY_2022_2_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
                , UIToolkit
#endif
        ] public string AutoContentFixed { get; private set; }

        [ShowInInspector, Ordered] public const float MyConstFloat = 3.14f;
        [ShowInInspector, Ordered] public static readonly Color MyColor = Color.green;

        [ShowInInspector, Ordered]
        public Color AutoColor
        {
            get => Color.green;
            // ReSharper disable once ValueParameterNotUsed
            set
            {
                // nothing
            }
        }

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

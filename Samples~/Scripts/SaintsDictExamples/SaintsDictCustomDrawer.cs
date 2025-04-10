using System;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsDictExamples
{
    public class SaintsDictCustomDrawer : MonoBehaviour
    {
        // [Serializable]
        // public enum MyEnum
        // {
        //     One,
        //     Two,
        //     Three,
        //     Four,
        // }
        //
        // public SaintsDictionary<MyEnum, AnimatorState> eToState;
        // public SaintsDictionary<MyEnum, Vector3> eToV3;
        //
        // // for old unity
        // [Serializable]
        // public class EnumToAnimatorState: SaintsDictionary<MyEnum, AnimatorState> {}
        // public EnumToAnimatorState eToStateOld;
        //
        // [Serializable]
        // public class EnumToVector3: SaintsDictionary<MyEnum, Vector3> {}
        // public EnumToVector3 eToV3Old;

        [Serializable]
        public struct MyStruct
        {
            [LayoutStart("Main", ELayout.TitleBox)]
            public string ss;
            public int si;

            [Button("Click")]
            public void BtnFunc() {}
        }

        public SaintsDictionary<string, MyStruct> myStructToGameObject;
    }
}

using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsDictExamples
{
    public class SaintsDictCustomDrawer : MonoBehaviour
    {
        [Serializable]
        public enum MyEnum
        {
            One,
            Two,
            Three,
            Four,
        }

        public SaintsDictionary<MyEnum, AnimatorState> eToState;
        public SaintsDictionary<MyEnum, Vector3> eToV3;

        // for old unity
        [Serializable]
        public class EnumToAnimatorState: SaintsDictionary<MyEnum, AnimatorState> {}
        public EnumToAnimatorState eToStateOld;

        [Serializable]
        public class EnumToVector3: SaintsDictionary<MyEnum, Vector3> {}
        public EnumToVector3 eToV3Old;
    }
}

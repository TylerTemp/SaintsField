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
    }
}

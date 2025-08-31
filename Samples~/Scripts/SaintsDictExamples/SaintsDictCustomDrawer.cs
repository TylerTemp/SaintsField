using System;
using System.Collections.Generic;
using SaintsField.Playa;
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

        [Serializable]
        public struct MyKeyStruct: IEquatable<MyKeyStruct>
        {
            public string key;

            [PlayaSeparator("Key List")]
            public string[] ks;

            public bool Equals(MyKeyStruct other)
            {
                return key == other.key && KsEqual(ks, other.ks);
            }

            public override bool Equals(object obj)
            {
                return obj is MyKeyStruct other && Equals(other);
            }

            public override int GetHashCode()
            {
                // the fuck...
                int hashCode = 17;
                hashCode *= 31 + key?.GetHashCode() ?? 0;
                hashCode *= 31 + ks?.GetHashCode() ?? 0;
                return hashCode;
            }

            private static bool KsEqual(string[] ks1, string[] ks2)
            {
                if (ks1 == null)
                {
                    return ks2 == null;
                }

                if (ks2 == null)
                {
                    return false;
                }

                if (ks1.Length != ks2.Length)
                {
                    return false;
                }

                // ReSharper disable once LoopCanBeConvertedToQuery
                for (int i = 0; i < ks1.Length; i++)
                {
                    if (ks1[i] != ks2[i])
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        [Serializable]
        public struct MyValueStruct
        {
            [LayoutStart("Data", ELayout.TitleBox)]
            public string ss;

            public int si;


            [LayoutEnd]

            [LayoutStart("Buttons", ELayout.Horizontal)]
            [Button("Ok")]
            public void BtnOk() {}
            [Button("Cancel")]
            public void BtnCancel() {}
        }

        public SaintsDictionary<MyKeyStruct, MyValueStruct> myStructToGameObject;
    }
}

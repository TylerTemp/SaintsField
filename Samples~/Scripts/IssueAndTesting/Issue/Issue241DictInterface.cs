using System;
using System.Collections.Generic;
using SaintsField.Utils;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue241DictInterface : MonoBehaviour
    {
#if UNITY_2021_3_OR_NEWER
        public interface IMyKey: IEquatable<IMyKey>
        {
        }

        [Serializable]
        public class Key1 : IMyKey
        {
            public string k1;

            public bool Equals(IMyKey other)
            {
                return other is Key1 other1 && k1 == other1.k1;
            }

            public override bool Equals(object obj)
            {
                if (obj is null) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != GetType()) return false;
                return Equals((Key1)obj);
            }

            public override int GetHashCode()
            {
                return (k1 != null ? k1.GetHashCode() : 0);
            }
        }

        [Serializable]
        public struct Key2 : IMyKey
        {
            public string k2;

            public bool Equals(IMyKey other)
            {
                return other is Key2 other2 && k2 == other2.k2;
            }

            public override bool Equals(object obj)
            {
                return obj is Key2 other && Equals(other);
            }

            public override int GetHashCode()
            {
                return k2 != null ? k2.GetHashCode() : 0;
            }
        }

        public interface IMyValue
        {
        }

        [Serializable]
        public class Value1 : IMyValue
        {
            public string v1;
        }

        [Serializable]
        public class Value2 : IMyValue
        {
            public string v2;
        }

        public SaintsDictionary<IMyKey, IMyValue> iDict;
#endif
    }
}

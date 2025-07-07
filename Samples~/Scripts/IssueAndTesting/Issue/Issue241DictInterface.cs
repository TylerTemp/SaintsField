using System;
using System.Collections.Generic;
using SaintsField.Utils;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue241DictInterface : MonoBehaviour
    {
#if UNITY_2021_3_OR_NEWER
        [Serializable]
        public class InterfaceDictionary<TKey, TValue> : SaintsDictionaryBase<TKey, TValue>
        {
            [Serializable]
            public class SaintsWrap<T> : BaseWrap<T>
            {
                [SerializeReference, SaintsRow(inline: true), ReferencePicker(hideLabel: true)] public T value;

                public override T Value
                {
                    get => value;
                    set => this.value = value;
                }

#if UNITY_EDITOR
                // ReSharper disable once StaticMemberInGenericType
                public static readonly string EditorPropertyName = nameof(value);
#endif

                public SaintsWrap(T v)
                {
                    value = v;
                }
            }

            [SerializeField]
            private List<SaintsWrap<TKey>> _saintsKeys = new List<SaintsWrap<TKey>>();

            [SerializeField]
            private List<SaintsWrap<TValue>> _saintsValues = new List<SaintsWrap<TValue>>();
#if UNITY_EDITOR
            // ReSharper disable once UnusedMember.Local
            private static string EditorPropKeys => nameof(_saintsKeys);
            // ReSharper disable once UnusedMember.Local
            private static string EditorPropValues => nameof(_saintsValues);
#endif

            protected override int SerializedKeysCount()
            {
                return _saintsKeys.Count;
            }

            protected override void SerializedKeyAdd(TKey key)
            {
                _saintsKeys.Add(new SaintsWrap<TKey>(key));
            }

            protected override TKey SerializedKeyGetAt(int index)
            {
                return _saintsKeys[index].value;
            }

            protected override void SerializedKeysClear()
            {
                _saintsKeys.Clear();
            }

            protected override int SerializedValuesCount()
            {
                return _saintsValues.Count;
            }

            protected override void SerializedValueAdd(TValue value)
            {
                _saintsValues.Add(new SaintsWrap<TValue>(value));
            }

            protected override TValue SerializedValueGetAt(int index)
            {
                return _saintsValues[index].value;
            }

            protected override void SerializedValuesClear()
            {
                _saintsValues.Clear();
            }

            protected override void SerializedSetKeyValue(TKey tKey, TValue tValue)
            {
                int index = _saintsKeys.FindIndex(wrap => wrap.value.Equals(tKey));
                if (index >= 0)
                {
                    _saintsValues[index].value = tValue;
                }
                else
                {
                    _saintsKeys.Add(new SaintsWrap<TKey>(tKey));
                    _saintsValues.Add(new SaintsWrap<TValue>(tValue));
                }
            }

            protected override void SerializedRemoveKeyValue(TKey key)
            {
                int index = _saintsKeys.FindIndex(wrap => wrap.value.Equals(key));
                if (index >= 0)
                {
                    _saintsKeys.RemoveAt(index);
                    _saintsValues.RemoveAt(index);
                }
            }
        }

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

        public InterfaceDictionary<IMyKey, IMyValue> iDict;
#endif
    }
}

using System;
using System.Collections.Generic;
using SaintsField.Utils;
using UnityEngine;

namespace SaintsField
{
    [Serializable]
    public class SaintsHashSet<T>: SaintsHashSetBase<T>
    {
        [Serializable]
        public class SaintsWrap : BaseWrap<T>
        {
            [SerializeField] public T value;
            public override T Value { get => value; set => this.value = value; }

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
        private List<SaintsWrap> _saintsList = new List<SaintsWrap>();

#if UNITY_EDITOR
        // ReSharper disable once UnusedMember.Local
        private static string EditorPropertyName => nameof(_saintsList);
#endif

        protected override int SerializedCount() => _saintsList.Count;

        protected override void SerializedAdd(T key)
        {
            _saintsList.Add(new SaintsWrap(key));
        }

        protected override T SerializedGetAt(int index) => _saintsList[index].value;

        protected override void SerializedClear() => _saintsList.Clear();
    }
}

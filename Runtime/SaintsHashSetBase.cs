using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace SaintsField
{
    public abstract class SaintsHashSetBase<T>:
        ISerializationCallbackReceiver
    {
        protected abstract int SerializedCount();
        protected abstract void SerializedAdd(T key);
        protected abstract T SerializedGetAt(int index);
        protected abstract void SerializedClear();

        protected HashSet<T> HashSet = new HashSet<T>();

        public void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            // do nothing
#else
            SerializedClear();
            foreach (T value in HashSet)
            {
                SerializedAdd(value);
            }
#endif
        }

        public void OnAfterDeserialize()
        {
            HashSet.Clear();

            int serCount = SerializedCount();
            for (int index = 0; index < serCount; index++)
            {
                T value = SerializedGetAt(index);
                HashSet.Add(value);
            }

#if UNITY_EDITOR
            // do nothing
#else
            SerializedClear();
#endif
        }
    }
}

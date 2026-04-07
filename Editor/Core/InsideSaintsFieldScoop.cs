using System;
using System.Collections.Generic;
using SaintsField.Editor.Utils;
using UnityEditor;

namespace SaintsField.Editor.Core
{
    public readonly struct InsideSaintsFieldScoop: IDisposable
    {
        public readonly struct PropertyKey : IEquatable<PropertyKey>
        {
            private readonly
#if UNITY_6000_4_OR_NEWER
                ulong
#else
                int
#endif
                _objectHash;

            private readonly string _propertyPath;


            public PropertyKey(
#if UNITY_6000_4_OR_NEWER
                UnityEngine.EntityId entityId,
#else
                int objectHash,
#endif
                string propertyPath)
            {
                _objectHash =
#if UNITY_6000_4_OR_NEWER
                    UnityEngine.EntityId.ToULong(entityId)
#else
                    objectHash
#endif
                ;

                _propertyPath = propertyPath;
            }

            public override string ToString()
            {
                return $"{_objectHash}.{_propertyPath}";
            }

            public bool Equals(PropertyKey other)
            {
                return _objectHash == other._objectHash && _propertyPath == other._propertyPath;
            }

            public override bool Equals(object obj)
            {
                return obj is PropertyKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                return Util.CombineHashCode(_objectHash, _propertyPath);
            }
        }

        private readonly PropertyKey _property;

        public static PropertyKey MakeKey(SerializedProperty property) => new PropertyKey(
            property.serializedObject.targetObject.
#if UNITY_6000_4_OR_NEWER
                GetEntityId
#else

                GetInstanceID
#endif
            (),

            property.propertyPath
        );

        private readonly Dictionary<PropertyKey, int> _counter;

        public InsideSaintsFieldScoop(Dictionary<PropertyKey, int> counter, PropertyKey key)
        {
            _counter = counter;
            _property = key;

            // ReSharper disable once CanSimplifyDictionaryTryGetValueWithGetValueOrDefault
            if (!_counter.TryGetValue(key, out int count))
            {
                count = 0;
            }

            // Debug.Log($"subCount {key} {count}+1");
            _counter[key] = count + 1;
        }

        public void Dispose()
        {
            // SaintsPropertyDrawer.IsSubDrawer = false;
            if (_counter.TryGetValue(_property, out int count))
            {
                // Debug.Log($"subCount {_property} {count}-1");
                _counter[_property] = count - 1;
            }
        }
    }
}

using System;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace SaintsField.Events
{
    [Serializable]
    public class PersistentArgument: ISerializationCallbackReceiver
    {
        public string name;

        [Serializable]
        public enum ValueType
        {
            Dynamic,
            Serialized,
            OptionalDefault,
        }

        public ValueType valueType;

        public bool isOptional;
        public int invokedParameterIndex = -1;  // -1=serialized; otherwise use dynamic invoked parameter index
        public bool isUnityObject;

        public TypeReference typeReference = new TypeReference();

        public UnityEngine.Object unityObject;
        public byte[] serializeBinaryData = Array.Empty<byte>();
        public object SerializeObject;

        public void OnBeforeSerialize()
        {
            serializeBinaryData = SerializeObject == null
                ? Array.Empty<byte>()
                : SerializationUtil.ToBinaryType(SerializeObject);
        }

        public void OnAfterDeserialize()
        {
            if (serializeBinaryData.Length > 0)
            {
                try
                {
                    SerializeObject = SerializationUtil.FromBinaryType(typeReference.Type, serializeBinaryData);
                }
                catch (ArgumentException e)
                {
#if SAINTSFIELD_DEBUG
                    Debug.LogWarning(e);
#endif
                }
            }
        }

        public Type GetArgumentType() => isUnityObject ? unityObject?.GetType() : typeReference.Type;
        public object GetArgumentValue() => isUnityObject ? unityObject : SerializeObject;
    }
}

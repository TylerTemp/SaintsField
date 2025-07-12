using System;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace SaintsField.Events
{
    [Serializable]
    public class PersistentArgument: ISerializationCallbackReceiver
    {
        public bool isUnityObject;  // true=unityObject; false=serializeBinaryData(SerializeObject)

        public TypeReference typeReference;

        public UnityEngine.Object unityObject;
        public byte[] serializeBinaryData = Array.Empty<byte>();
        public object SerializeObject;

        public void OnBeforeSerialize()
        {
            if (!isUnityObject)
            {
                serializeBinaryData = SerializationUtil.ToBinaryType(SerializeObject);
            }
        }

        public void OnAfterDeserialize()
        {
            if(!isUnityObject)
            {
                SerializeObject = SerializationUtil.FromBinaryType(typeReference.Type, serializeBinaryData);
            }
        }

        public Type GetArgumentType() => isUnityObject ? unityObject?.GetType() : typeReference.Type;
    }
}

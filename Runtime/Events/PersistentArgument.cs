using System;
// using SaintsField.Playa;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace SaintsField.Events
{
    [Serializable]
    public class PersistentArgument: ISerializationCallbackReceiver
    {
        public int invokedParameterIndex = -1;  // -1=serialized; otherwise use dynamic invoked parameter index

        public bool isUnityObject;  // true=unityObject; false=serializeBinaryData(SerializeObject)

        public TypeReference typeReference;

        public UnityEngine.Object unityObject;
        public byte[] serializeBinaryData = Array.Empty<byte>();
        public object SerializeObject;

        // [Button]
        // private void Ser43()
        // {
        //     SerializeObject = 43;
        //     serializeBinaryData = SerializationUtil.ToBinaryType(SerializeObject);
        //     typeReference = new TypeReference(typeof(Int32));
        // }

        public void OnBeforeSerialize()
        {
            if (invokedParameterIndex != -1)
            {
                return;
            }

            if (!isUnityObject)
            {
                serializeBinaryData = SerializationUtil.ToBinaryType(SerializeObject);
            }
        }

        public void OnAfterDeserialize()
        {
            if (invokedParameterIndex != -1)
            {
                return;
            }

            if(!isUnityObject && typeReference.Type != null)
            {
                SerializeObject = SerializationUtil.FromBinaryType(typeReference.Type, serializeBinaryData);
            }
        }

        public Type GetArgumentType() => isUnityObject ? unityObject?.GetType() : typeReference.Type;
        public object GetArgumentValue() => isUnityObject ? unityObject : SerializeObject;
    }
}

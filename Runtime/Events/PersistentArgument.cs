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
        public enum CallType
        {
            Dynamic,
            Serialized,
            OptionalDefault,
        }

        public CallType callType;

        public bool isOptional;
        public int invokedParameterIndex = -1;  // -1=serialized; otherwise use dynamic invoked parameter index
        public bool isUnityObject;

        public TypeReference typeReference = new TypeReference();

        public UnityEngine.Object unityObject;
        // public bool serializedAsJson;
        // public byte[] serializeBinaryData = Array.Empty<byte>();
        public string serializeJsonData = "";
        public object SerializeObject;

        public void OnBeforeSerialize()
        {
            // serializeBinaryData = SerializeObject == null
            //     ? Array.Empty<byte>()
            //     : SerializationUtil.ToBinaryType(SerializeObject);
        }

        public void OnAfterDeserialize()
        {
            // if (serializedAsJson)
            // {
                if (!string.IsNullOrEmpty(serializeJsonData))
                {
                    try
                    {
                        SerializeObject = SerializationUtil.FromJsonType(typeReference.Type, serializeJsonData);
                    }
                    catch (Exception e)
                    {
#if SAINTSFIELD_DEBUG
                        Debug.LogWarning(e);
#endif
                    }
                }
//             }
//             else
//             {
//                 if (serializeBinaryData.Length > 0)
//                 {
//                     try
//                     {
//                         SerializeObject = SerializationUtil.FromBinaryType(typeReference.Type, serializeBinaryData);
//                     }
//                     catch (Exception e)
//                     {
// #if SAINTSFIELD_DEBUG
//                         Debug.LogWarning(e);
// #endif
//                     }
//                 }
//             }
        }

        public Type GetArgumentType() => isUnityObject ? unityObject?.GetType() : typeReference.Type;
        public object GetArgumentValue() => isUnityObject ? unityObject : SerializeObject;
    }
}

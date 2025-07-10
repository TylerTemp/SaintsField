using System;

namespace SaintsField.Runtime.Events
{
    [Serializable]
    public class PersistentArgument
    {
        public bool isUnityObject;
        public UnityEngine.Object unityObject;

        public byte[] serializeBinaryData;
        public object SerializeObject;
    }
}

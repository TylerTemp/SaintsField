using System;
using UnityEditor;

namespace SaintsField.Editor.Utils
{
    public static class EMessageTypeExtensions
    {
        public static MessageType GetMessageType(this EMessageType messageType)
        {
            switch (messageType)
            {
                case EMessageType.None:
                    return MessageType.None;
                case EMessageType.Info:
                    return MessageType.Info;
                case EMessageType.Warning:
                    return MessageType.Warning;
                case EMessageType.Error:
                    return MessageType.Error;
                default:
                    throw new ArgumentOutOfRangeException(nameof(messageType), messageType, null);
            }
        }
    }
}

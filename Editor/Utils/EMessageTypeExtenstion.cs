using System;
using UnityEditor;

namespace SaintsField.Editor.Utils
{
    public static class EMessageTypeExtensions
    {
        public static MessageType GetMessageType(this EMessageType messageType) => messageType switch
        {
            EMessageType.None => MessageType.None,
            EMessageType.Info => MessageType.Info,
            EMessageType.Warning => MessageType.Warning,
            EMessageType.Error => MessageType.Error,
            _ => throw new ArgumentOutOfRangeException(nameof(messageType), messageType, null)
        };
    }
}

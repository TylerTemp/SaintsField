using System;
using UnityEditor;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Utils
{
    public static class EMessageTypeExtensions
    {
        public static MessageType GetMessageType(this EMessageType messageType)
        {
            // ReSharper disable once ConvertSwitchStatementToSwitchExpression
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

        public static HelpBoxMessageType GetUIToolkitMessageType(this EMessageType messageType)
        {
            // ReSharper disable once ConvertSwitchStatementToSwitchExpression
            switch (messageType)
            {
                case EMessageType.None:
                    return HelpBoxMessageType.None;
                case EMessageType.Info:
                    return HelpBoxMessageType.Info;
                case EMessageType.Warning:
                    return HelpBoxMessageType.Warning;
                case EMessageType.Error:
                    return HelpBoxMessageType.Error;
                default:
                    throw new ArgumentOutOfRangeException(nameof(messageType), messageType, null);
            }
        }
    }
}

using UnityEditor;

namespace SaintsField
{
    public enum EMessageType
    {
        /// <summary>
        ///   <para>Neutral message.</para>
        /// </summary>
        None,
        /// <summary>
        ///   <para>Info message.</para>
        /// </summary>
        Info,
        /// <summary>
        ///   <para>Warning message.</para>
        /// </summary>
        Warning,
        /// <summary>
        ///   <para>Error message.</para>
        /// </summary>
        Error,
    }

    public static class EMessageTypeExtensions
    {
        public static MessageType GetMessageType(this EMessageType messageType) => messageType switch
        {
            EMessageType.None => MessageType.None,
            EMessageType.Info => MessageType.Info,
            EMessageType.Warning => MessageType.Warning,
            EMessageType.Error => MessageType.Error,
        };
    }
}

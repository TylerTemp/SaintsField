using UnityEngine;

namespace SaintsField
{
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = true)]
    public class InfoBoxAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy { get; }

        public readonly bool Above;
        public readonly string Content;
        public readonly EMessageType MessageType;
        public readonly bool isCallback;
        public readonly string ShowCallback;

        public InfoBoxAttribute(string content, EMessageType messageType=EMessageType.Info, string show=null, bool isCallback=false, bool above=false, string groupBy="")
        {
            GroupBy = groupBy;

            Above = above;
            Content = content;
            MessageType = messageType;
            this.isCallback = isCallback;
            ShowCallback = show;
        }
    }
}

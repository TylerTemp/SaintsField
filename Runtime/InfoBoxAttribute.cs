﻿using UnityEngine;

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
        public readonly bool ContentIsCallback;
        public readonly string ShowCallback;

        public InfoBoxAttribute(string content, EMessageType messageType, string show=null, bool contentIsCallback=false, bool above=false, string groupBy="")
        {
            GroupBy = groupBy;

            Above = above;
            Content = content;
            MessageType = messageType;
            ContentIsCallback = contentIsCallback;
            ShowCallback = show;
        }
    }
}

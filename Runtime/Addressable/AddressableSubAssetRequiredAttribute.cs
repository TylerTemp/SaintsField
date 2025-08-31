using System.Diagnostics;
using SaintsField.Interfaces;
using UnityEngine;

namespace SaintsField.Addressable
{
    [Conditional("UNITY_EDITOR")]
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = true)]
    public class AddressableSubAssetRequiredAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy => "";

        public readonly string ErrorMessage;
        public readonly EMessageType MessageType;

        public AddressableSubAssetRequiredAttribute(string errorMessage = null, EMessageType messageType = EMessageType.Error)
        {
            ErrorMessage = errorMessage;
            MessageType = messageType;
        }
    }
}

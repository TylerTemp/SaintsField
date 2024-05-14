using System;
using UnityEngine;
using UnityEngine.Events;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue27 : MonoBehaviour
    {
        [Serializable]
        public enum RCType
        {
            Int,
            Float,
            Bool,
            String,
            Long,
            Json
        }
        [Serializable]
        public class ValueDetail<T>
        {
            public T Value;
            public T defaultValue;
        }

        [Serializable]
        public class RCValue
        {
            public string key;
            public RCType valueType;
            [ShowIf(nameof(valueType), RCType.Int)] public ValueDetail<int> intValue; //this is not working
            [ShowIf(nameof(valueType), RCType.Int)] public UnityEvent<int> onIntValueFetched; //this is not working
            [ShowIf(nameof(valueType), RCType.Float)] public ValueDetail<float> floatValue; //this is not working
            [ShowIf(nameof(valueType), RCType.Float)] public UnityEvent<float> onFloatValueFetched; //this is not working
            [ShowIf(nameof(valueType), RCType.Bool)] public ValueDetail<bool> boolValue; //this is not working
            [ShowIf(nameof(valueType), RCType.Bool)] public UnityEvent<bool> onBoolValueFetched; //this is not working
            [ShowIf(nameof(valueType), RCType.String)] public ValueDetail<string> stringValue; //this is not working
            [ShowIf(nameof(valueType), RCType.String)] public UnityEvent<string> onStringValueFetched; //this is not working
            [ShowIf(nameof(valueType), RCType.Long)] public ValueDetail<long> longValue; //this is not working
            [ShowIf(nameof(valueType), RCType.Long)] public UnityEvent<long> onLongValueFetched; //this is not working
            [ShowIf(nameof(valueType), RCType.Json)] public ValueDetail<string> jsonValue; //this is not working
            [ShowIf(nameof(valueType), RCType.Json)] public UnityEvent<string> onJsonValueFetched; //this is not working
        }

        [SerializeField] RCValue[] rCValues;

    }
}

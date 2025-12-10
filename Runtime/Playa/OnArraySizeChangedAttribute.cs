using System;
using System.Diagnostics;
using SaintsField.Utils;
using UnityEngine;

namespace SaintsField.Playa
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field)]
    [Obsolete("Use OnValueChanged instead")]
    public class OnArraySizeChangedAttribute: PropertyAttribute, IPlayaAttribute
    {
        public readonly string Callback;

        public OnArraySizeChangedAttribute(string callback)
        {
            Callback = RuntimeUtil.ParseCallback(callback).content;
        }
    }
}

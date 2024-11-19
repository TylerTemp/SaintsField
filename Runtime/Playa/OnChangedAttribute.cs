using System;
using System.Diagnostics;
using SaintsField.Utils;
using UnityEngine;

namespace SaintsField.Playa
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field)]
    public class OnChangedAttribute: PropertyAttribute, IPlayaAttribute
    {
        public readonly string Callback;

        public OnChangedAttribute(string callback)
        {
            Callback = RuntimeUtil.ParseCallback(callback).content;
        }
    }
}

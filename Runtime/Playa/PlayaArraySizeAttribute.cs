using System;
using System.Diagnostics;
using UnityEngine;

namespace SaintsField.Playa
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property)]
    public class PlayaArraySizeAttribute: PropertyAttribute, IPlayaAttribute
    {
        public readonly int Size;

        public PlayaArraySizeAttribute(int size)
        {
            Size = size;
        }
    }
}

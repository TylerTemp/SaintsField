using System;
using System.Diagnostics;
using UnityEngine;

namespace SaintsField.Playa
{
    [Conditional("UNITY_EDITOR")]
    [Obsolete("Use `ArraySize` instead")]
    [AttributeUsage(AttributeTargets.Field)]
    public class PlayaArraySizeAttribute: PropertyAttribute, IPlayaAttribute, IPlayaArraySizeAttribute
    {
        public readonly int Size;

        public PlayaArraySizeAttribute(int size)
        {
            Size = size;
        }
    }
}

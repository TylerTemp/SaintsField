using System;
using System.Diagnostics;
using SaintsField.Playa;

// ReSharper disable once CheckNamespace
namespace SaintsField.Utils
{
    [Conditional("UNITY_EDITOR")]
    public class SaintsSerializedActualAttribute: Attribute, IPlayaAttribute
    {
        public readonly string Path;
        public readonly Type PathType;

        public SaintsSerializedActualAttribute(string path, Type pathType)
        {
            Path = path;
            PathType = pathType;
        }
    }
}

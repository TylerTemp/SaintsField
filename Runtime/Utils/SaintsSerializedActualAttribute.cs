using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using SaintsField.Playa;
using SaintsField.SaintsSerialization;

// ReSharper disable once CheckNamespace
namespace SaintsField.Utils
{
    [Conditional("UNITY_EDITOR")]
    public class SaintsSerializedActualAttribute: Attribute, IPlayaAttribute
    {
        // public readonly string Path;
        // public readonly Type PathType;

        public readonly IReadOnlyList<SaintsSerializedPath> Paths;

        public SaintsSerializedActualAttribute(params string[] objPaths)
        {
            Paths = objPaths.Select(ConvertSaintsSerializedPath).ToArray();
        }

        private static SaintsSerializedPath ConvertSaintsSerializedPath(string arg)
        {
            string[] commaSplit = arg.Split(',');
            string name = commaSplit[0];
            bool isProperty = int.Parse(commaSplit[1]) != 0;
            SaintsTargetCollection targetCollection = (SaintsTargetCollection)int.Parse(commaSplit[2]);
            SaintsPropertyType saintsPropertyType = (SaintsPropertyType)int.Parse(commaSplit[3]);
            return new SaintsSerializedPath(name, isProperty, targetCollection, saintsPropertyType);
        }
    }
}

using System.Diagnostics;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    public class GetScriptableObjectAttribute: PropertyAttribute, ISaintsAttribute, IPlayaAttribute, IPlayaArraySizeAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;

        public string GroupBy => "";

        public readonly string PathSuffix;

        public GetScriptableObjectAttribute(string pathSuffix=null)
        {
            PathSuffix = string.IsNullOrEmpty(pathSuffix)? null: pathSuffix + ".asset";
        }
    }
}

using UnityEngine;

namespace SaintsField
{
    public class GetScriptableObjectAttribute: PropertyAttribute, ISaintsAttribute
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

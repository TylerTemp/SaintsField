using System;
using System.Diagnostics;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class GetMainCameraAttribute: GetByXPathAttribute
    {
        private const string XPath = "scene:://@{GetComponent(Camera)}[@{tag} = 'MainCamera']";
        public GetMainCameraAttribute(): base(XPath)
        {
        }

        public GetMainCameraAttribute(EXP config) : base(config, XPath)
        {
        }
    }
}

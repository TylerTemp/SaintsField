using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SaintsField.Interfaces;
using SaintsField.Playa;
using SaintsField.SaintsXPathParser.Optimization;
using SaintsField.Utils;
using UnityEngine;

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

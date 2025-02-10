using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.SaintsXPathParser.Optimization;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.XPathDrawers.GetByXPathDrawer
{
    public partial class GetByXPathAttributeDrawer
    {
        private static (string error, bool hasElement, IEnumerable<object> results) GetXPathByOptimized(OptimizationPayload optimizationPayload, SerializedProperty property, MemberInfo info)
        {
            switch (optimizationPayload)
            {
                case GetComponentPayload getComponentPayload:
                    return GetComponentOptimized(getComponentPayload.CompType, property, info);
                default:
                    throw new ArgumentOutOfRangeException(nameof(optimizationPayload), optimizationPayload, null);
            }
        }

        private static (string error, bool hasElement, IEnumerable<object>  results) GetComponentOptimized(Type compType, SerializedProperty property, MemberInfo info)
        {
            (string error, Type fieldType, Type interfaceType) = GetExpectedTypeOfProp(property, info);
            if (error != "")
            {
                return (error, false, null);
            }

            Type type = compType ?? fieldType;

            IReadOnlyList<Object> r = Util.GetTargetsTypeFromObj(property.serializedObject.targetObject, type);
            List<object> validValues = new List<object>();
            foreach (Object rawResult in r)
            {
                (bool valid, object value) = ValidateXPathResult(rawResult, type, interfaceType);
                if (valid)
                {
                    validValues.Add(value);
                }
            }

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_GET_BY_XPATH
            Debug.Log($"#GetByXPath# GetComponentOptimized: {validValues.Count} valid values found");
#endif

            return ("", validValues.Count > 0, validValues);
        }
    }
}

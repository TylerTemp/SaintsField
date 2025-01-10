using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.AutoRunner
{
    public interface IAutoRunnerFixDrawer
    {
        AutoRunnerFixerResult AutoRunFix(PropertyAttribute propertyAttribute, IReadOnlyList<PropertyAttribute> allAttributes,
            SerializedProperty property,
            MemberInfo memberInfo, object parent);
    }
}

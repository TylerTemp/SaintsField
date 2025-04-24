using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.SaintsDictionary.DictionaryWrap
{
    public partial class SaintsDictionaryWrapDrawer
    {
        protected override bool UseCreateFieldUIToolKit => true;

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, FieldInfo info, object parent)
        {
            // string label = GetPreferredLabel(property);
            //
            SerializedProperty realProp = property.FindPropertyRelative("value") ?? SerializedUtils.FindPropertyByAutoPropertyName(property, "value");
            Debug.Assert(realProp != null, property.propertyPath);
            return PropertyFieldFallbackUIToolkit(realProp);
        }
    }
}

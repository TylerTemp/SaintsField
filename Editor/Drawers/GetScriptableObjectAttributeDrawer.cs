using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(GetScriptableObjectAttribute))]
    public class GetScriptableObjectAttributeDrawer: SaintsPropertyDrawer
    {
        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => 0;

        protected override bool DrawPostField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute,
            bool valueChanged)
        {
            if (property.objectReferenceValue != null)
            {
                return false;
            }

            GetScriptableObjectAttribute getScriptableObjectAttribute = (GetScriptableObjectAttribute) saintsAttribute;

            Type fieldType = SerializedUtils.GetType(property);

            IEnumerable<string> paths = AssetDatabase.FindAssets($"t:{fieldType.Name}")
                .Select(AssetDatabase.GUIDToAssetPath);

            if (getScriptableObjectAttribute.PathSuffix != null)
            {
                paths = paths.Where(each => each.EndsWith(getScriptableObjectAttribute.PathSuffix));
            }
            Object result = paths
                .Select(each => AssetDatabase.LoadAssetAtPath(each, fieldType))
                .FirstOrDefault(each => each != null);

            // ReSharper disable once InvertIf
            if (result != null)
            {
                property.objectReferenceValue = result;
                SetValueChanged(property);
            }

            return true;
        }
    }
}

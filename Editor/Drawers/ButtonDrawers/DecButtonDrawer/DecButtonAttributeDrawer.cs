using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.ButtonDrawers.DecButtonDrawer
{
    public abstract partial class DecButtonAttributeDrawer: SaintsPropertyDrawer
    {
        private static IEnumerable<(string error, object result)> CallButtonFunc(SerializedProperty property, DecButtonAttribute decButtonAttribute, FieldInfo fieldInfo, object target)
        {
            SaintsContext.SerializedProperty = property;

            if (property.serializedObject.targetObjects.Length < 2)
            {
                yield return Util.GetMethodOf<object>(decButtonAttribute.FuncName, null, property, fieldInfo, target);
                yield break;
            }

            string propPath = property.propertyPath;
            foreach (Object t in property.serializedObject.targetObjects)
            {
                // Debug.Log($"{t.GetType().Name}:{t}");
                // ReSharper disable once ConvertToUsingDeclaration
                using (SerializedObject so = new SerializedObject(t))
                {
                    SerializedProperty prop = so.FindProperty(propPath);

                    // Debug.Log($"Found property {prop.name} in {t.GetType().Name}");
                    (PropertyAttribute[] _, object parent) = SerializedUtils.GetAttributesAndDirectParent<PropertyAttribute>(prop);
                    yield return Util.GetMethodOf<object>(decButtonAttribute.FuncName, null, prop, fieldInfo, parent);
                    // property = prop;
                }
                // (PropertyAttribute[] allAttributesRaw, object parent) = SerializedUtils.GetAttributesAndDirectParent<PropertyAttribute>(property);

            }
            // return Util.GetMethodOf<object>(decButtonAttribute.FuncName, null, property, fieldInfo, target);
        }


    }
}

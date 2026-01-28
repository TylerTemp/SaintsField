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
        public static IEnumerable<(string error, object result)> CallButtonFunc(SerializedProperty property, string callback, FieldInfo fieldInfo, object target)
        {
            SaintsContext.SerializedProperty = property;

            if (property.serializedObject.targetObjects.Length <= 1)
            {
                object useParent = target;
                if(target != null && ReflectUtils.TypeIsStruct(target.GetType()))
                {
                    (SerializedUtils.FieldOrProp _, object refreshedParent) =
                        SerializedUtils.GetFieldInfoAndDirectParent(property);
                    if (refreshedParent != null)
                    {
                        // Debug.Log($"rewrite parent {refreshedParent}");
                        useParent = refreshedParent;
                    }
                }

                yield return Util.GetOf<object>(callback, null, property, fieldInfo, useParent, null);
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
                    yield return Util.GetOf<object>(callback, null, prop, fieldInfo, parent, null);
                    // property = prop;
                }
                // (PropertyAttribute[] allAttributesRaw, object parent) = SerializedUtils.GetAttributesAndDirectParent<PropertyAttribute>(property);

            }
            // return Util.GetMethodOf<object>(decButtonAttribute.FuncName, null, property, fieldInfo, target);
        }


    }
}

using System.Collections.Generic;
using System;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

#if SAINTSFIELD_UNITASK && !SAINTSFIELD_UNITASK_DISABLE
using Cysharp.Threading.Tasks;
#endif

namespace SaintsField.Editor.Drawers.ButtonDrawers.DecButtonDrawer
{
    public abstract partial class DecButtonAttributeDrawer: SaintsPropertyDrawer
    {
#if SAINTSFIELD_UNITASK && !SAINTSFIELD_UNITASK_DISABLE
        protected static (bool returnIsUniTask, Type returnUniTaskValueType) GetUniTaskReturnInfo(Type returnType)
        {
            bool returnIsUniTask = false;
            Type returnUniTaskValueType = null;

            if (typeof(UniTask).IsAssignableFrom(returnType))
            {
                returnIsUniTask = true;
            }

            foreach (Type genBaseType in ReflectUtils.GetGenBaseTypes(returnType))
            {
                if (genBaseType.GetGenericTypeDefinition() == typeof(UniTask<>))
                {
                    returnIsUniTask = true;
                    returnUniTaskValueType = genBaseType.GetGenericArguments()[0];
                    break;
                }
            }

            return (returnIsUniTask, returnUniTaskValueType);
        }
#endif

        public static IEnumerable<(string error, MemberInfo memberInfo, object result)> CallButtonFunc(SerializedProperty property, string callback, FieldInfo fieldInfo, object target)
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
            foreach (UnityEngine.Object t in property.serializedObject.targetObjects)
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

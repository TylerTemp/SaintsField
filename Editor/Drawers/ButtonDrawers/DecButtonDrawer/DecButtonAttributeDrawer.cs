using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;
using SaintsField.Condition;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

#if SAINTSFIELD_UNITASK && !SAINTSFIELD_UNITASK_DISABLE
using Cysharp.Threading.Tasks;
#endif

namespace SaintsField.Editor.Drawers.ButtonDrawers.DecButtonDrawer
{
    public abstract partial class DecButtonAttributeDrawer: SaintsPropertyDrawer
    {
        private enum AsyncReturnType
        {
            None,
            Awaitable,
            UniTask,
        }

        private static (AsyncReturnType returnAsync, Type returnAsyncValueType) GetAsyncReturnInfo(Type returnType)
        {
#if UNITY_6000_0_OR_NEWER
            if (typeof(Awaitable).IsAssignableFrom(returnType))
            {
                return (AsyncReturnType.Awaitable, null);
            }

            foreach (Type genBaseType in ReflectUtils.GetGenBaseTypes(returnType))
            {
                if (genBaseType.GetGenericTypeDefinition() == typeof(Awaitable<>))
                {
                    return (AsyncReturnType.Awaitable, genBaseType.GetGenericArguments()[0]);
                }
            }
#endif

#if SAINTSFIELD_UNITASK && !SAINTSFIELD_UNITASK_DISABLE
            if (typeof(UniTask).IsAssignableFrom(returnType))
            {
                return (AsyncReturnType.UniTask, null);
            }

            foreach (Type genBaseType in ReflectUtils.GetGenBaseTypes(returnType))
            {
                if (genBaseType.GetGenericTypeDefinition() == typeof(UniTask<>))
                {
                    return (AsyncReturnType.UniTask, genBaseType.GetGenericArguments()[0]);
                }
            }
#endif

            return (AsyncReturnType.None, null);
        }

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



        protected abstract IReadOnlyList<DecButtonShowIfAttribute> GetCurrentShowHide(
            IReadOnlyList<PropertyAttribute> attributes, ISaintsAttribute currentAttribute);

        protected abstract IReadOnlyList<DecButtonDisableIfAttribute> GetCurrentDisableEnable(
            IReadOnlyList<PropertyAttribute> attributes, ISaintsAttribute currentAttribute);

        private (string error, bool show, object reParent) GetShowResult(SerializedProperty property,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes, FieldInfo info)
        {
            IReadOnlyList<DecButtonShowIfAttribute> showIf = GetCurrentShowHide(
                allAttributes,
                saintsAttribute
            );
            if (showIf.Count == 0)
            {
                return ("", true, null);
            }

            object reParent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
            (string error, bool show) result = GetShow(property, showIf, info, reParent);
            return (result.error, result.show, reParent);
        }

        private (string error, bool disable, object reParent) GetDisableResult(SerializedProperty property,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes, FieldInfo info,
            object parent)
        {
            IReadOnlyList<DecButtonDisableIfAttribute> disableIf = GetCurrentDisableEnable(
                allAttributes,
                saintsAttribute
            );
            if (disableIf.Count == 0)
            {
                return ("", false, null);
            }

            object reParent = parent ?? SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
            (string error, bool disable) result = GetDisable(property, disableIf, info, reParent);
            return (result.error, result.disable, reParent);
        }

        private static (string error, bool show) GetShow(SerializedProperty property,
            IReadOnlyList<DecButtonShowIfAttribute> conditionAttributes,
            FieldInfo info, object parent)
        {

            List<bool> showOrResults = new List<bool>();

            foreach (DecButtonShowIfAttribute decButtonShowIfAttribute in conditionAttributes)
            {
                IReadOnlyList<ConditionInfo> conditionInfos = decButtonShowIfAttribute.ConditionInfos;
                bool isShow = decButtonShowIfAttribute.IsShow;

                (IReadOnlyList<string> errors, IReadOnlyList<bool> boolResults) = Util.ConditionChecker(conditionInfos, property, info, parent);
                // Debug.Log($"isShow={isShow}/result={string.Join(", ", boolResults)}/error={string.Join(", ", errors)}");
                if (errors.Count > 0)
                {
                    return (string.Join("\n", errors), true);
                }

                bool thisShow;
                if (isShow)
                {
                    thisShow = boolResults.All(each => each);
                }
                else
                {
                    bool isHidden = boolResults.Any(each => each);
                    thisShow = !isHidden;
                }
                showOrResults.Add(thisShow);
            }

            bool showFinal = true;
            if (showOrResults.Count > 0)
            {
                showFinal = showOrResults.Any(each => each);
            }

            return ("", showFinal);
        }

        private static (string error, bool disable) GetDisable(SerializedProperty property,
            IReadOnlyList<DecButtonDisableIfAttribute> conditionAttributes,
            FieldInfo info, object parent)
        {
            List<bool> orResults = new List<bool>();

            foreach (DecButtonDisableIfAttribute decButtonShowIfAttribute in conditionAttributes)
            {
                IReadOnlyList<ConditionInfo> conditionInfos = decButtonShowIfAttribute.ConditionInfos;
                bool isDisable = decButtonShowIfAttribute.IsDisable;

                (IReadOnlyList<string> errors, IReadOnlyList<bool> boolResults) = Util.ConditionChecker(conditionInfos, property, info, parent);
                // Debug.Log($"isDisable={isDisable}/result={string.Join(", ", boolResults)}/error={string.Join(", ", errors)}");
                if (errors.Count > 0)
                {
                    return (string.Join("\n", errors), true);
                }

                bool thisDisable;
                if (isDisable)
                {
                    thisDisable = boolResults.All(each => each);
                }
                else
                {
                    bool isHidden = boolResults.Any(each => each);
                    thisDisable = !isHidden;
                }
                orResults.Add(thisDisable);
            }

            bool resultFinal = false;
            if (orResults.Count > 0)
            {
                resultFinal = orResults.Any(each => each);
            }

            // Debug.Log($"resultFinal={resultFinal}");

            return ("", resultFinal);
        }
    }
}

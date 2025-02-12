using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Playa.Renderer.BaseRenderer
{
    public abstract partial class SerializedFieldBaseRenderer: AbsRenderer
    {
        protected SerializedFieldBaseRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo) : base(serializedObject, fieldWithInfo)
        {
        }

        protected static IEnumerable<UnityEngine.Object> CanDrop(IEnumerable<UnityEngine.Object> targets, Type elementType) => targets.Where(each => Util.GetTypeFromObj(each, elementType));

        private static void InvokeCallback(string callback, object newValue, object parent)
        {
            List<Type> types = ReflectUtils.GetSelfAndBaseTypes(parent);
            types.Reverse();

            const BindingFlags bindAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
                                          BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.FlattenHierarchy;

            foreach (Type type in types)
            {
                MethodInfo methodInfo = type.GetMethod(callback, bindAttr);
                if (methodInfo == null)
                {
                    continue;
                }

                object[] passParams = ReflectUtils.MethodParamsFill(methodInfo.GetParameters(), new[]
                {
                    newValue,
                });

                try
                {
                    methodInfo.Invoke(parent, passParams);
                }
                catch (TargetInvocationException e)
                {
                    Debug.LogException(e);
                    // Debug.Assert(e.InnerException != null);
                    // return e.InnerException?.Message ?? e.Message;
                    return;
                }
                catch (InvalidCastException e)
                {
                    Debug.LogException(e);
                    // return e.Message;
                    return;
                }
                catch (Exception e)
                {
                    // _error = e.Message;
                    Debug.LogException(e);
                    // return e.Message;
                    return;
                }

                // return "";
                return;
            }

            string error = $"No field or method named `{callback}` found on `{parent}`";
            Debug.LogError(error);
        }

        protected static void InvokeArraySizeCallback(string callback, SerializedProperty property, MemberInfo memberInfo)
        {
            object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
            if (parent == null)
            {
                Debug.LogWarning("Property disposed unexpectedly");
                return;
            }

            (string error, int _, object newValue) = Util.GetValue(property, memberInfo, parent);
            if (error != "")
            {
                Debug.LogError(error);
                return;
            }

            InvokeCallback(callback, newValue, parent);
        }

        public override string ToString() => $"Ser<{FieldWithInfo.FieldInfo?.Name ?? FieldWithInfo.SerializedProperty.displayName}>";
    }
}

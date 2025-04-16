using System;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.OnValueChangedDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.SuperPriority)]
#endif
    [CustomPropertyDrawer(typeof(OnValueChangedAttribute), true)]
    public partial class OnValueChangedAttributeDrawer : SaintsPropertyDrawer
    {
        // protected override bool DrawPostFieldImGui(Rect position, SerializedProperty property, GUIContent label,
        //     ISaintsAttribute saintsAttribute, int index, OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        // {
        //     // Debug.Log($"OnValueChangedAttributeDrawer={valueChanged}");
        //     if (!onGUIPayload)
        //     {
        //         return true;
        //     }
        //
        //     _error = InvokeCallback(saintsAttribute, parent);
        //
        //     return true;
        // }

        private static string InvokeCallback(string callback, object newValue, int index, object parent)
        {
            // no, don't use this. We already have the value
            // (string error, object _) = Util.GetMethodOf<object>(callback, null, property, info, target);
            // return error != "" ? error : "";

            // object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;

            const BindingFlags bindAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
                                          BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.FlattenHierarchy;

            foreach (Type type in ReflectUtils.GetSelfAndBaseTypes(parent))
            {
                MethodInfo methodInfo = type.GetMethod(callback, bindAttr);
                if (methodInfo == null)
                {
                    continue;
                }

                object[] passParams = ReflectUtils.MethodParamsFill(methodInfo.GetParameters(), index == -1
                    ? new[]
                    {
                        newValue,
                    }
                    : new []
                    {
                        newValue,
                        index,
                    });

                try
                {
                    methodInfo.Invoke(parent, passParams);
                }
                catch (TargetInvocationException e)
                {
                    Debug.LogException(e);
                    Debug.Assert(e.InnerException != null);
                    return e.InnerException.Message;
                }
                catch (InvalidCastException e)
                {
                    Debug.LogException(e);
                    return e.Message;
                }
                catch (Exception e)
                {
                    // _error = e.Message;
                    Debug.LogException(e);
                    return e.Message;
                }

                return "";
            }

            return $"No field or method named `{callback}` found on `{parent}`";
        }

    }
}

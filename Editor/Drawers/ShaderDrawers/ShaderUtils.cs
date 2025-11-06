#if UNITY_2021_2_OR_NEWER
using System;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.ShaderDrawers
{
    public static class ShaderUtils
    {
        public static (string error, Shader shader) GetShader(string callback, int index, SerializedProperty property, MemberInfo info, object parent)
        {
            if (string.IsNullOrEmpty(callback))  // find on target
            {
                Renderer directRenderer;
                Object obj = property.serializedObject.targetObject;
                switch (obj)
                {
                    case Component comp:
                        directRenderer = comp.GetComponent<Renderer>();
                        break;
                    case GameObject go:
                        directRenderer = go.GetComponent<Renderer>();
                        break;
                    default:
                        return ($"{obj} is not a valid target", null);
                }
                return GetShaderFromRenderer(directRenderer, index);
            }

            (string error, Object uObj) = Util.GetOf<Object>(callback, null, property, info, parent);
            if (error != "")
            {
#if SAINTSFIELD_DEBUG
                Debug.LogError(error);
#endif
                return (error, null);
            }

            return GetShaderFromObject(uObj, callback, index);
        }

        public static (string error, Shader shader) GetShaderFromObject(object uObj, string callback, int index)
        {
            if (RuntimeUtil.IsNull(uObj))
            {
                return ($"Target {callback} is null", null);
            }

            // ReSharper disable once ConvertSwitchStatementToSwitchExpression
            switch (uObj)
            {
                case Material material:
                    return ("", material.shader);
                case Shader shader:
                    return ("", shader);
                case Renderer renderer:
                    return GetShaderFromRenderer(renderer, index);
                case Component comp:
                    return GetShaderFromRenderer(comp.GetComponent<Renderer>(), index);
                case GameObject go:
                    return GetShaderFromRenderer(go.GetComponent<Renderer>(), index);
                default:
                    return ($"Target `{callback}` is not a valid target: {uObj} ({uObj.GetType()})", null);
            }
        }

        public static (string error, Shader shader) GetShaderFromRenderer(Renderer renderer, int index)
        {
            if (renderer == null)
            {
                return ($"No renderer found on target", null);
            }

            Material[] targetMaterials = renderer.sharedMaterials;
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (index >= targetMaterials.Length)
            {
                return ($"Index {index} out of range ({targetMaterials.Length})", null);
            }

            Material result = targetMaterials[index];
            if (RuntimeUtil.IsNull(result))
            {
                return ("Material is null", null);
            }
            return ("", result.shader);
        }

        public static void UpdateHelpBox(HelpBox helpBox, string error)
        {
            if (helpBox.text == error)
            {
                return;
            }

            if (string.IsNullOrEmpty(error))
            {
                helpBox.style.display = DisplayStyle.None;
                helpBox.text = "";
            }
            else
            {
                helpBox.text = error;
                helpBox.style.display = DisplayStyle.Flex;
            }
        }

        public static (string error, Shader shader) GetShaderForShowInInspector(object curValue, string callback, int index, object target)
        {
            if (string.IsNullOrEmpty(callback))  // find on target
            {
                Renderer directRenderer;
                switch (target)
                {
                    case Component comp:
                        directRenderer = comp.GetComponent<Renderer>();
                        break;
                    case GameObject go:
                        directRenderer = go.GetComponent<Renderer>();
                        break;
                    default:
                        return ($"{target} is not a valid target", null);
                }
                return ShaderUtils.GetShaderFromRenderer(directRenderer, index);
            }

            foreach (Type type in ReflectUtils.GetSelfAndBaseTypesFromInstance(target))
            {
                (ReflectUtils.GetPropType getPropType, object fieldOrMethodInfo) = ReflectUtils.GetProp(type, callback);

                switch (getPropType)
                {
                    case ReflectUtils.GetPropType.NotFound:
                        continue;

                    case ReflectUtils.GetPropType.Property:
                    {
                        object genResult = ((PropertyInfo)fieldOrMethodInfo).GetValue(target);
                        if(genResult != null)
                        {
                            return ShaderUtils.GetShaderFromObject(genResult, callback, index);
                        }
                    }
                        break;
                    case ReflectUtils.GetPropType.Field:
                    {
                        FieldInfo fInfo = (FieldInfo)fieldOrMethodInfo;
                        object genResult = fInfo.GetValue(target);
                        if(genResult != null)
                        {
                            return ShaderUtils.GetShaderFromObject(genResult, callback, index);
                        }
                        // Debug.Log($"{fInfo}/{fInfo.Name}, target={target} genResult={genResult}");
                    }
                        break;
                    case ReflectUtils.GetPropType.Method:
                    {
                        MethodInfo methodInfo = (MethodInfo)fieldOrMethodInfo;

                        object[] passParams = ReflectUtils.MethodParamsFill(methodInfo.GetParameters(), new[]
                        {
                            curValue,
                        });


                        object genResult;
                        try
                        {
                            genResult = methodInfo.Invoke(target, passParams);
                        }
                        catch (TargetInvocationException e)
                        {
                            return (e.InnerException?.Message ?? e.Message, null);
                        }
                        catch (Exception e)
                        {
                            return (e.Message, null);
                        }

                        if (genResult != null)
                        {
                            return ShaderUtils.GetShaderFromObject(genResult, callback, index);
                        }

                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException(nameof(getPropType), getPropType, null);
                }
            }

            return ($"Target `{callback}` not found", null);
        }
    }
}
#endif

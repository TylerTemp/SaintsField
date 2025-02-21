#if UNITY_2021_2_OR_NEWER
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;

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

            if (RuntimeUtil.IsNull(uObj))
            {
                return ($"Target `{callback}` is null", null);
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

        private static (string error, Shader shader) GetShaderFromRenderer(Renderer renderer, int index)
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
    }
}
#endif

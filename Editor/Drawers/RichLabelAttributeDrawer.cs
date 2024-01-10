using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(RichLabelAttribute))]
    public class RichLabelAttributeDrawer: SaintsPropertyDrawer
    {
        private readonly RichTextDrawer _richTextDrawer = new RichTextDrawer();

        // private readonly Color _backgroundColor;
        //
        // public RichLabelAttributeDrawer()
        // {
        //     _backgroundColor = EditorGUIUtility.isProSkin
        //         ? new Color32(56, 56, 56, 255)
        //         : new Color32(194, 194, 194, 255);
        // }

        #region IMGUI

        private string _error = "";

        ~RichLabelAttributeDrawer()
        {
            _richTextDrawer.Dispose();
        }

        // protected override float GetLabelHeight(SerializedProperty property, GUIContent label,
        //     ISaintsAttribute saintsAttribute) =>
        //     EditorGUIUtility.singleLineHeight;

        protected override bool WillDrawLabel(SerializedProperty property, ISaintsAttribute saintsAttribute)
        {
            RichLabelAttribute targetAttribute = (RichLabelAttribute)saintsAttribute;
            bool result = GetLabelXml(property, targetAttribute) != null;
            // Debug.Log($"richLabel willDraw={result}");
            return result;
        }

        protected override void DrawLabel(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute)
        {
            RichLabelAttribute targetAttribute = (RichLabelAttribute)saintsAttribute;

            string labelXml = GetLabelXml(property, targetAttribute);

            if (labelXml is null)
            {
                return;
            }

            // EditorGUI.DrawRect(position, _backgroundColor);
            _richTextDrawer.DrawChunks(position, label, RichTextDrawer.ParseRichXml(labelXml, label.text));
            // LabelMouseProcess(position, property);
        }

        // protected override IEnumerable<VisualElement> DrawLabelChunkUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute)
        // {
        //     RichLabelAttribute targetAttribute = (RichLabelAttribute)saintsAttribute;
        //
        //     string labelXml = GetLabelXml(property, targetAttribute);
        //
        //     return labelXml is null
        //         ? Array.Empty<VisualElement>()
        //         : _richTextDrawer.DrawChunksUIToolKit(property.displayName, RichTextDrawer.ParseRichXml(labelXml, property.displayName));
        // }

        private string GetLabelXml(SerializedProperty property, RichLabelAttribute targetAttribute)
        {
            if (!targetAttribute.IsCallback)
            {
                return targetAttribute.RichTextXml;
            }

            _error = "";
            object target = GetParentTarget(property);
            (ReflectUtils.GetPropType getPropType, object fieldOrMethodInfo) =
                ReflectUtils.GetProp(target.GetType(), targetAttribute.RichTextXml);
            switch (getPropType)
            {
                case ReflectUtils.GetPropType.Field:
                {
                    object result = ((FieldInfo)fieldOrMethodInfo).GetValue(target);
                    return result == null ? string.Empty : result.ToString();
                }

                case ReflectUtils.GetPropType.Property:
                {
                    object result = ((PropertyInfo)fieldOrMethodInfo).GetValue(target);
                    return result == null ? string.Empty : result.ToString();
                }
                case ReflectUtils.GetPropType.Method:
                {
                    MethodInfo methodInfo = (MethodInfo)fieldOrMethodInfo;
                    ParameterInfo[] methodParams = methodInfo.GetParameters();
                    ParameterInfo[] requiredParams = methodParams.Where(p => !p.IsOptional).ToArray();
                    // Debug.Assert(methodParams.All(p => p.IsOptional));
                    Debug.Assert(requiredParams.Length <= 1);
                    if (methodInfo.ReturnType != typeof(string))
                    {
                        _error =
                            $"Expect returning string from `{targetAttribute.RichTextXml}`, get {methodInfo.ReturnType}";
                    }
                    else
                    {
                        int arrayIndex = 0;
                        bool dataCallback = false;
                        if (requiredParams.Length == 1)
                        {
                            Debug.Assert(requiredParams[0].ParameterType == typeof(int));
                            string[] propPaths = property.propertyPath.Split('.');
                            string lastPropPath = propPaths[propPaths.Length - 1];
                            if(lastPropPath.StartsWith("data[") && lastPropPath.EndsWith("]"))
                            {
                                dataCallback = true;
                                arrayIndex = int.Parse(lastPropPath.Substring(5, lastPropPath.Length - 6));
                            }
                        }

                        object[] passParams;
                        if(dataCallback)
                        {
                            List<object> injectedParams = new List<object>();
                            bool injected = false;
                            foreach (ParameterInfo methodParam in methodParams)
                            {
                                if (!injected && methodParam.ParameterType == typeof(int))
                                {
                                    injectedParams.Add(arrayIndex);
                                    injected = true;
                                }
                                else
                                {
                                    injectedParams.Add(methodParam.DefaultValue);
                                }
                            }
                            passParams = injectedParams.ToArray();
                        }
                        else
                        {
                            passParams = methodParams
                                .Select(p => p.DefaultValue)
                                .ToArray();
                        }

                        try
                        {
                            return (string)methodInfo.Invoke(
                                target,
                                passParams
                            );
                        }
                        catch (TargetInvocationException e)
                        {
                            Debug.Assert(e.InnerException != null);
                            _error = e.InnerException.Message;
                            Debug.LogException(e);
                        }
                        catch (Exception e)
                        {
                            _error = e.Message;
                            Debug.LogException(e);
                        }
                    }
                    return null;
                }
                case ReflectUtils.GetPropType.NotFound:
                {
                    _error =
                        $"not found `{targetAttribute.RichTextXml}` on `{target}`";
                    return null;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(getPropType), getPropType, null);
            }
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute)
        {
            return _error != "";
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute)
        {
            return _error == "" ? 0 : ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            return ImGuiHelpBox.Draw(position, _error, MessageType.Error);
        }
        #endregion

        #region UIToolkit

        protected override VisualElement CreateOverlayUIKit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container, object parent)
        {
            return new Button(() =>
            {
                OnLabelStateChangedUIToolkit(property, container, "");
            })
            {
                text = "Event it!"
            };
        }

        #endregion
    }
}

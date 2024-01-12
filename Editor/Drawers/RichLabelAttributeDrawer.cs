using System;
using System.Collections.Generic;
using System.IO;
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
            (string error, string _) = GetLabelXml(property, targetAttribute, GetParentTarget(property));
            // bool result = GetLabelXml(property, targetAttribute) != null;
            // Debug.Log($"richLabel willDraw={result}");
            // return result;
            _error = error;
            return error != null;
        }

        protected override void DrawLabel(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute)
        {
            RichLabelAttribute targetAttribute = (RichLabelAttribute)saintsAttribute;

            (string error, string labelXml) = GetLabelXml(property, targetAttribute, GetParentTarget(property));
            _error = error;

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

        private static (string error, string xml) GetLabelXml(SerializedProperty property, RichLabelAttribute targetAttribute, object target)
        {
            if (!targetAttribute.IsCallback)
            {
                return ("", targetAttribute.RichTextXml);
            }
            //
            // _error = "";
            // object target = GetParentTarget(property);
            (ReflectUtils.GetPropType getPropType, object fieldOrMethodInfo) =
                ReflectUtils.GetProp(target.GetType(), targetAttribute.RichTextXml);
            switch (getPropType)
            {
                case ReflectUtils.GetPropType.Field:
                {
                    object result = ((FieldInfo)fieldOrMethodInfo).GetValue(target);
                    return ("", result == null ? string.Empty : result.ToString());
                }

                case ReflectUtils.GetPropType.Property:
                {
                    object result = ((PropertyInfo)fieldOrMethodInfo).GetValue(target);
                    return ("", result == null ? string.Empty : result.ToString());
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
                        return (
                            $"Expect returning string from `{targetAttribute.RichTextXml}`, get {methodInfo.ReturnType}", property.displayName);
                    }

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
                        return ("", (string)methodInfo.Invoke(
                            target,
                            passParams
                        ));
                    }
                    catch (TargetInvocationException e)
                    {
                        Debug.LogException(e);
                        Debug.Assert(e.InnerException != null);
                        return (e.InnerException.Message, property.displayName);
                    }
                    catch (Exception e)
                    {
                        // _error = e.Message;
                        Debug.LogException(e);
                        return (e.Message, property.displayName);
                    }
                }
                case ReflectUtils.GetPropType.NotFound:
                {
                    return ($"not found `{targetAttribute.RichTextXml}` on `{target}`", property.displayName);
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

        private static string NameRichLabelContainer(SerializedProperty property) => $"{property.propertyPath}__RichLabelContainer";
        private static string NameRichLabelHelpBox(SerializedProperty property) => $"{property.propertyPath}__RichLabelHelpBox";

        protected override VisualElement CreateOverlayUIKit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container, object parent)
        {
            return new VisualElement
            {
                style =
                {
                    position = Position.Absolute,
                    flexDirection = FlexDirection.Row,
                    height = EditorGUIUtility.singleLineHeight,
                    marginLeft = LabelLeftSpace,
                    width = LabelBaseWidth,
                    textOverflow = TextOverflow.Clip,
                    overflow = Overflow.Hidden,
                    unityTextAlign = TextAnchor.MiddleLeft,

                    flexShrink = 0,
                    flexGrow = 0,
                },
                name = NameRichLabelContainer(property),
                userData = new string(' ', property.displayName.Length),
                pickingMode = PickingMode.Ignore,
            };
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container, object parent)
        {
            return new HelpBox("", HelpBoxMessageType.Error)
            {
                name = NameRichLabelHelpBox(property),
                userData = "",
                style =
                {
                    display = DisplayStyle.None,
                },
            };
        }

        protected override void OnUpdateUiToolKit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container, object parent)
        {
            VisualElement labelContainer = container.Q<VisualElement>(NameRichLabelContainer(property));
            string curXml = (string)labelContainer.userData;
            (string error, string nowXml) = GetLabelXml(property, (RichLabelAttribute)saintsAttribute, parent);
            if (curXml != nowXml)
            {
                labelContainer.userData = nowXml;
                labelContainer.Clear();
                if (nowXml != null)
                {
                    foreach (VisualElement richChunk in _richTextDrawer.DrawChunksUIToolKit(property.displayName, RichTextDrawer.ParseRichXml(nowXml, property.displayName)))
                    {
                        labelContainer.Add(richChunk);
                    }
                }

                OnLabelStateChangedUIToolkit(property, container, nowXml);
            }

            HelpBox helpBox = container.Q<HelpBox>(NameRichLabelHelpBox(property));
            string curError = (string)helpBox.userData;
            if (curError != error)
            {
                helpBox.userData = error;
                helpBox.style.display = string.IsNullOrEmpty(error) ? DisplayStyle.None : DisplayStyle.Flex;
                helpBox.text = error;
            }

        }

        #endregion
    }
}

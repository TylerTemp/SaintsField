using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(OverlayRichLabelAttribute))]
    public class OverlayRichLabelAttributeDrawer: SaintsPropertyDrawer
    {
        private readonly RichTextDrawer _richTextDrawer = new RichTextDrawer();
        private string _error = "";

        ~OverlayRichLabelAttributeDrawer()
        {
            _richTextDrawer.Dispose();
        }

        protected override (bool willDraw, Rect drawPosition) DrawOverlay(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, bool hasLabel, IReadOnlyCollection<Rect> takenPositions)
        {
            string inputContent = GetContent(property);
            // Debug.Log(inputContent);
            OverlayRichLabelAttribute targetAttribute = (OverlayRichLabelAttribute)saintsAttribute;

            float contentWidth = GetPlainTextWidth(inputContent) + targetAttribute.Padding;
            string labelXml = GetLabelXml(property, targetAttribute);

            if (labelXml is null)
            {
                return (false, default);
            }

            float labelWidth = hasLabel? EditorGUIUtility.labelWidth : 0;

            RichTextDrawer.RichTextChunk[] payloads = RichTextDrawer.ParseRichXml(labelXml, label.text).ToArray();
            float overlayWidth = _richTextDrawer.GetWidth(label, position.height, payloads);

            float leftWidth = position.width - labelWidth - contentWidth;

            bool hasEnoughSpace = !targetAttribute.End && leftWidth > overlayWidth;

            float useWidth = hasEnoughSpace? overlayWidth : leftWidth;
            float useOffset = hasEnoughSpace? labelWidth + contentWidth : position.width - overlayWidth;

            Rect overlayRect = new Rect(position)
            {
                x = position.x + useOffset,
                width = useWidth,
            };

            _richTextDrawer.DrawChunks(overlayRect, label, payloads);

            return (true, overlayRect);
        }

        private static string GetContent(SerializedProperty property)
        {
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    return property.longValue.ToString();
                case SerializedPropertyType.Float:
                    return $"{property.doubleValue}";
                case SerializedPropertyType.String:
                    return property.stringValue;
                default:
                    throw new ArgumentOutOfRangeException(nameof(property.propertyType), property.propertyType, null);
            }
        }

        private static float GetPlainTextWidth(string plainContent)
        {
            return EditorStyles.label.CalcSize(new GUIContent(plainContent)).x;
        }

        private string GetLabelXml(SerializedProperty property, OverlayRichLabelAttribute targetAttribute)
        {
            if (!targetAttribute.IsCallback)
            {
                return targetAttribute.RichTextXml;
            }

            _error = "";
            object target = property.serializedObject.targetObject;
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
                    Debug.Assert(methodParams.All(p => p.IsOptional));
                    if (methodInfo.ReturnType != typeof(string))
                    {
                        _error =
                            $"Expect returning string from `{targetAttribute.RichTextXml}`, get {methodInfo.ReturnType}";
                    }
                    else
                    {
                        try
                        {
                            return (string)methodInfo.Invoke(target,
                                methodParams.Select(p => p.DefaultValue).ToArray());
                        }
                        catch (TargetInvocationException e)
                        {
                            _error = e.InnerException!.Message;
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
    }
}

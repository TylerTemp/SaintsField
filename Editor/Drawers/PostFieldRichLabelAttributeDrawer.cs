using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(PostFieldRichLabelAttribute))]
    public class PostFieldRichLabelAttributeDrawer: SaintsPropertyDrawer
    {
        private readonly RichTextDrawer _richTextDrawer = new RichTextDrawer();
        private string _error = "";

        ~PostFieldRichLabelAttributeDrawer()
        {
            _richTextDrawer.Dispose();
        }

        private IReadOnlyList<RichTextDrawer.RichTextChunk> _payloads;

        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            PostFieldRichLabelAttribute targetAttribute = (PostFieldRichLabelAttribute)saintsAttribute;
            string labelXml = GetLabelXml(property, targetAttribute);
            if (string.IsNullOrEmpty(labelXml))
            {
                _payloads = null;
                return 0;
            }

            _payloads = RichTextDrawer.ParseRichXml(labelXml, label.text).ToArray();
            return _richTextDrawer.GetWidth(label, position.height, _payloads) + targetAttribute.Padding;
        }

        protected override bool DrawPostField(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute, bool valueChanged)
        {
            if (_error != "")
            {
                return false;
            }

            if(_payloads == null || _payloads.Count == 0)
            {
                return false;
            }

            PostFieldRichLabelAttribute targetAttribute = (PostFieldRichLabelAttribute)saintsAttribute;

            Rect drawRect = new Rect(position)
            {
                x = position.x + targetAttribute.Padding,
                width = position.width - targetAttribute.Padding,
            };

            _richTextDrawer.DrawChunks(drawRect, label, _payloads);

            return true;
        }

        private string GetLabelXml(SerializedProperty property, PostFieldRichLabelAttribute targetAttribute)
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

        protected override bool WillDrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            return _error != "";
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute)
        {
            return _error == "" ? 0 : HelpBox.GetHeight(_error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) =>
            _error == ""
                ? position
                : HelpBox.Draw(position, _error, MessageType.Error);
    }
}

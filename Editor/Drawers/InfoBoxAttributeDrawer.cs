using System;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(InfoBoxAttribute))]
    public class InfoBoxAttributeDrawer: SaintsPropertyDrawer
    {
        private string _error = "";

        private bool _overrideMessageType;
        private EMessageType _messageType;

        protected override bool WillDrawAbove(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            InfoBoxAttribute infoboxAttribute = (InfoBoxAttribute)saintsAttribute;

            // ReSharper disable once ConvertIfStatementToReturnStatement
            if(!infoboxAttribute.Above)
            {
                return false;
            }

            return WillDraw(property, infoboxAttribute);
        }

        protected override float GetAboveExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute)
        {
            InfoBoxAttribute infoboxAttribute = (InfoBoxAttribute)saintsAttribute;
            return ((InfoBoxAttribute)saintsAttribute).Above
                ? HelpBox.GetHeight(GetContent(property, (InfoBoxAttribute) saintsAttribute), width, _overrideMessageType? _messageType: infoboxAttribute.MessageType)
                : 0;
        }

        protected override Rect DrawAbove(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            InfoBoxAttribute infoboxAttribute = (InfoBoxAttribute)saintsAttribute;
            return HelpBox.Draw(position, GetContent(property, infoboxAttribute), _overrideMessageType? _messageType: infoboxAttribute.MessageType);
        }

        protected override bool WillDrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            if (_error != "")
            {
                return true;
            }

            InfoBoxAttribute infoboxAttribute = (InfoBoxAttribute)saintsAttribute;

            // ReSharper disable once ConvertIfStatementToReturnStatement
            if(infoboxAttribute.Above)
            {
                return false;
            }

            return WillDraw(property, infoboxAttribute);
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute)
        {
            InfoBoxAttribute infoboxAttribute = (InfoBoxAttribute)saintsAttribute;

            float boxHeight = !infoboxAttribute.Above && WillDraw(property, (InfoBoxAttribute)saintsAttribute)
                ? HelpBox.GetHeight(GetContent(property, (InfoBoxAttribute)saintsAttribute), width, _overrideMessageType ? _messageType : infoboxAttribute.MessageType)
                : 0f;

            float errorHeight = _error != "" ? HelpBox.GetHeight(_error, width, EMessageType.Error) : 0f;
            return boxHeight + errorHeight;
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            InfoBoxAttribute infoboxAttribute = (InfoBoxAttribute)saintsAttribute;
            Rect leftRect = HelpBox.Draw(position, GetContent(property, infoboxAttribute), _overrideMessageType? _messageType: infoboxAttribute.MessageType);

            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (_error == "")
            {
                return leftRect;
            }

            return HelpBox.Draw(leftRect, _error, EMessageType.Error);
        }

        private bool WillDraw(SerializedProperty property, InfoBoxAttribute infoboxAttribute)
        {
            if (infoboxAttribute.ShowCallback == null)
            {
                return true;
            }

            object target = GetParentTarget(property);

            (ReflectUtils.GetPropType getPropType, object fieldOrMethodInfo) found = ReflectUtils.GetProp(target.GetType(), infoboxAttribute.ShowCallback);
            switch (found)
            {
                case (ReflectUtils.GetPropType.NotFound, _):
                {
                    _error = $"No field or method named `{infoboxAttribute.ShowCallback}` found on `{target}`";
                    Debug.LogError(_error);
                    return false;
                }
                case (ReflectUtils.GetPropType.Property, PropertyInfo propertyInfo):
                {
                    if (!ReflectUtils.Truly(propertyInfo.GetValue(target)))
                    {
                        return false;
                    }
                }
                    break;
                case (ReflectUtils.GetPropType.Field, FieldInfo foundFieldInfo):
                {
                    if (!ReflectUtils.Truly(foundFieldInfo.GetValue(target)))
                    {
                        return false;
                    }
                }
                    break;
                case (ReflectUtils.GetPropType.Method, MethodInfo methodInfo):
                {
                    ParameterInfo[] methodParams = methodInfo.GetParameters();
                    Debug.Assert(methodParams.All(p => p.IsOptional));
                    Debug.Assert(methodInfo.ReturnType == typeof(bool));
                    if (!(bool)methodInfo.Invoke(target, methodParams.Select(p => p.DefaultValue).ToArray()))
                    {
                        return false;
                    }
                }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(found), found, null);
            }

            return true;
        }

        private string GetContent(SerializedProperty property, InfoBoxAttribute infoboxAttribute)
        {
            _overrideMessageType = false;
            if (!infoboxAttribute.ContentIsCallback)
            {
                return infoboxAttribute.Content;
            }

            object target = GetParentTarget(property);

            (ReflectUtils.GetPropType getPropType, object fieldOrMethodInfo) found = ReflectUtils.GetProp(target.GetType(), infoboxAttribute.Content);
            switch (found)
            {
                case (ReflectUtils.GetPropType.NotFound, _):
                {
                    _error = $"No field or method named `{infoboxAttribute.ShowCallback}` found on `{target}`";
                    Debug.LogError(_error);
                    return string.Empty;
                }
                case (ReflectUtils.GetPropType.Property, PropertyInfo propertyInfo):
                {
                    // Debug.Log(propertyInfo.GetValue(target).GetType());
                    object result = propertyInfo.GetValue(target);
                    // ReSharper disable once InvertIf
                    if(result is ValueTuple<EMessageType, string> resultTuple)
                    {
                        _overrideMessageType = true;
                        _messageType = resultTuple.Item1;
                        return resultTuple.Item2;
                    }

                    return result.ToString();
                }
                case (ReflectUtils.GetPropType.Field, FieldInfo foundFieldInfo):
                {
                    object result = foundFieldInfo.GetValue(target);
                    // ReSharper disable once InvertIf
                    if(result is ValueTuple<EMessageType, string> resultTuple)
                    {
                        _overrideMessageType = true;
                        _messageType = resultTuple.Item1;
                        return resultTuple.Item2;
                    }

                    return result.ToString();
                }
                case (ReflectUtils.GetPropType.Method, MethodInfo methodInfo):
                {
                    ParameterInfo[] methodParams = methodInfo.GetParameters();
                    Debug.Assert(methodParams.All(p => p.IsOptional));
                    // Debug.Assert(methodInfo.ReturnType == typeof(string));
                    // Debug.Log(methodInfo.ReturnType);
                    // ReSharper disable once InvertIf
                    if (methodInfo.ReturnType == typeof(ValueTuple<EMessageType, string>))
                    {
                        (EMessageType messageType, string content) = ((EMessageType, string))methodInfo.Invoke(target, methodParams.Select(p => p.DefaultValue).ToArray());
                        _overrideMessageType = true;
                        _messageType = messageType;
                        return content;
                    }

                    return methodInfo.Invoke(target, methodParams.Select(p => p.DefaultValue).ToArray()).ToString();
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(found), found, null);
            }
        }
    }
}

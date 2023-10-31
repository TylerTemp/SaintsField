using System;
using System.Linq;
using System.Reflection;
using ExtInspector.Editor.Standalone;
using ExtInspector.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace ExtInspector.Editor
{
    [CustomPropertyDrawer(typeof(RichLabelAttribute))]
    public class RichLabelAttributeDrawer: SaintsPropertyDrawer
    {
        private readonly RichTextDrawer _richTextDrawer = new RichTextDrawer();
        private string _error = "";

        ~RichLabelAttributeDrawer()
        {
            _richTextDrawer.Dispose();
        }

        protected override float GetLabelHeight(SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute)
        {
            RichLabelAttribute targetAttribute = (RichLabelAttribute)saintsAttribute;
            return targetAttribute.RichTextXml is null
                ? 0
                : base.GetPropertyHeight(property, label);
        }

        protected override bool WillDrawLabel(SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            RichLabelAttribute targetAttribute = (RichLabelAttribute)saintsAttribute;
            return GetLabelXml(property, targetAttribute) == null;
        }

        protected override void DrawLabel(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute)
        {
            // label = EditorGUI.BeginProperty(position, label, property);

            RichLabelAttribute targetAttribute = (RichLabelAttribute)saintsAttribute;

            // (Rect labelRect, Rect propertyRect) =
            //     RectUtils.SplitWidthRect(EditorGUI.IndentedRect(position), EditorGUIUtility.labelWidth);

            // string labelXml = targetAttribute.RichTextXml;
            // if (targetAttribute.IsCallback)
            // {
            //     _error = "";
            //     object target = property.serializedObject.targetObject;
            //     (ReflectUil.GetPropType getPropType, object fieldOrMethodInfo) = ReflectUil.GetProp(target.GetType(), targetAttribute.RichTextXml);
            //     switch (getPropType)
            //     {
            //         case ReflectUil.GetPropType.Field:
            //         {
            //             object result = ((FieldInfo)fieldOrMethodInfo).GetValue(target);
            //             labelXml = result == null ? string.Empty : result.ToString();
            //         }
            //             break;
            //         case ReflectUil.GetPropType.Method:
            //         {
            //             MethodInfo methodInfo = (MethodInfo) fieldOrMethodInfo;
            //             ParameterInfo[] methodParams = methodInfo.GetParameters();
            //             Debug.Assert(methodParams.All(p => p.IsOptional));
            //             if (methodInfo.ReturnType != typeof(string))
            //             {
            //                 _error =
            //                     $"Expect returning string from `{targetAttribute.RichTextXml}`, get {methodInfo.ReturnType}";
            //             }
            //             else
            //             {
            //                 try
            //                 {
            //                     labelXml = (string) methodInfo.Invoke(target, methodParams.Select(p => p.DefaultValue).ToArray());
            //                 }
            //                 catch (TargetInvocationException e)
            //                 {
            //                     _error = e.InnerException!.Message;
            //                     Debug.LogException(e);
            //                 }
            //                 catch (Exception e)
            //                 {
            //                     _error = e.Message;
            //                     Debug.LogException(e);
            //                 }
            //             }
            //         }
            //             break;
            //         case ReflectUil.GetPropType.NotFound:
            //         {
            //             _error =
            //                 $"not found `{targetAttribute.RichTextXml}` on `{target}`";
            //         }
            //             break;
            //         default:
            //             throw new ArgumentOutOfRangeException(nameof(getPropType), getPropType, null);
            //     }
            // }

            string labelXml = GetLabelXml(property, targetAttribute);

            if (labelXml is null)
            {
                return;
            }

// #if EXT_INSPECTOR_LOG
//             Debug.Log($"RichLabelAttributeDrawer: {labelXml}");
// #endif

            _richTextDrawer.DrawChunks(position, label, RichTextDrawer.ParseRichXml(labelXml, label.text));

            // EditorGUI.PropertyField(propertyRect, property, GUIContent.none);

            // EditorGUI.EndProperty();
            // return;
        }

        private string GetLabelXml(SerializedProperty property, RichLabelAttribute targetAttribute)
        {
            if (!targetAttribute.IsCallback)
            {
                return targetAttribute.RichTextXml;
            }

            _error = "";
            object target = property.serializedObject.targetObject;
            (ReflectUil.GetPropType getPropType, object fieldOrMethodInfo) =
                ReflectUil.GetProp(target.GetType(), targetAttribute.RichTextXml);
            switch (getPropType)
            {
                case ReflectUil.GetPropType.Field:
                {
                    object result = ((FieldInfo)fieldOrMethodInfo).GetValue(target);
                    return result == null ? string.Empty : result.ToString();
                }

                case ReflectUil.GetPropType.Property:
                {
                    object result = ((PropertyInfo)fieldOrMethodInfo).GetValue(target);
                    return result == null ? string.Empty : result.ToString();
                }
                case ReflectUil.GetPropType.Method:
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
                case ReflectUil.GetPropType.NotFound:
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
            return _error == "" ? 0 : HelpBox.GetHeight(_error, width);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            return HelpBox.Draw(position, _error, MessageType.Error);
        }
    }
}

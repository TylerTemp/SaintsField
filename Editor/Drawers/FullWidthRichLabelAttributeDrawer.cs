using System;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using HelpBox = SaintsField.Editor.Utils.HelpBox;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(AboveRichLabelAttribute))]
    [CustomPropertyDrawer(typeof(BelowRichLabelAttribute))]
    [CustomPropertyDrawer(typeof(FullWidthRichLabelAttribute))]
    public class FullWidthRichLabelAttributeDrawer: SaintsPropertyDrawer
    {
        private readonly RichTextDrawer _richTextDrawer = new RichTextDrawer();
        private string _error = "";

        ~FullWidthRichLabelAttributeDrawer()
        {
            _richTextDrawer.Dispose();
        }

        protected override bool WillDrawAbove(SerializedProperty property, ISaintsAttribute saintsAttribute)
        {
            // Debug.Log("ABOVE!");
            FullWidthRichLabelAttribute fullWidthRichLabelAttribute = (FullWidthRichLabelAttribute)saintsAttribute;
            return fullWidthRichLabelAttribute.Above;
        }

        protected override float GetAboveExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute)
        {
            FullWidthRichLabelAttribute fullWidthRichLabelAttribute = (FullWidthRichLabelAttribute)saintsAttribute;
            return fullWidthRichLabelAttribute.Above? EditorGUIUtility.singleLineHeight: 0;
        }

        protected override Rect DrawAboveImGui(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            return DrawImGui(position, property, label, saintsAttribute);
        }

        protected override VisualElement CreateAboveUIToolKit(SerializedProperty property, ISaintsAttribute saintsAttribute)
        {
            return DrawUIToolKit(property, saintsAttribute);
        }

        private Rect DrawImGui(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            FullWidthRichLabelAttribute fullWidthRichLabelAttribute = (FullWidthRichLabelAttribute)saintsAttribute;

            string labelXml = GetLabelXml(property, fullWidthRichLabelAttribute);

            if (labelXml is null)
            {
                return position;
            }

            (Rect curRect, Rect leftRect) = RectUtils.SplitHeightRect(position, EditorGUIUtility.singleLineHeight);

            _richTextDrawer.DrawChunks(curRect, label, RichTextDrawer.ParseRichXml(labelXml, label.text));
            return leftRect;
        }

        private VisualElement DrawUIToolKit(SerializedProperty property, ISaintsAttribute saintsAttribute)
        {
            FullWidthRichLabelAttribute fullWidthRichLabelAttribute = (FullWidthRichLabelAttribute)saintsAttribute;

            string labelXml = GetLabelXml(property, fullWidthRichLabelAttribute);

            if (labelXml is null)
            {
                return new VisualElement();
            }

            VisualElement container = new VisualElement
            {
                style =
                {
                    // height = EditorGUIUtility.singleLineHeight,
                    flexDirection = FlexDirection.Row,
                    flexWrap = Wrap.Wrap,
                    // alignItems = Align.Center, // vertical
                    // overflow = Overflow.Hidden,
                },
                pickingMode = PickingMode.Ignore,
            };
            foreach (VisualElement variable in _richTextDrawer.DrawChunksUIToolKit(property.displayName, RichTextDrawer.ParseRichXml(labelXml, property.displayName)))
            {
                container.Add(variable);
            }

            return container;
        }

        private string GetLabelXml(SerializedProperty property, FullWidthRichLabelAttribute targetAttribute)
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
            FullWidthRichLabelAttribute fullWidthRichLabelAttribute = (FullWidthRichLabelAttribute)saintsAttribute;
            return !fullWidthRichLabelAttribute.Above || _error != "";
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute)
        {
            FullWidthRichLabelAttribute fullWidthRichLabelAttribute = (FullWidthRichLabelAttribute)saintsAttribute;
            float errorHeight = _error == "" ? 0 : HelpBox.GetHeight(_error, width, MessageType.Error);
            return fullWidthRichLabelAttribute.Above
                ? errorHeight
                : errorHeight + EditorGUIUtility.singleLineHeight;
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            Rect useRect = position;
            FullWidthRichLabelAttribute fullWidthRichLabelAttribute = (FullWidthRichLabelAttribute)saintsAttribute;
            if (!fullWidthRichLabelAttribute.Above)
            {
                useRect = DrawImGui(position, property, label, fullWidthRichLabelAttribute);
            }
            return _error == ""
                ? useRect
                : HelpBox.Draw(useRect, _error, MessageType.Error);
        }

        protected override VisualElement DrawBelowUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute)
        {
            VisualElement container = new VisualElement();

            FullWidthRichLabelAttribute fullWidthRichLabelAttribute = (FullWidthRichLabelAttribute) saintsAttribute;
            if (!fullWidthRichLabelAttribute.Above)
            {
                // useRect = DrawImGui(position, property, label, fullWidthRichLabelAttribute);
                container.Add(DrawUIToolKit(property, fullWidthRichLabelAttribute));
            }

            if (_error != "")
            {
                container.Add(new UnityEngine.UIElements.HelpBox(_error, HelpBoxMessageType.Error));
            }

            return container;
        }
    }
}

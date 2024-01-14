using System;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(InfoBoxAttribute))]
    public class InfoBoxAttributeDrawer: SaintsPropertyDrawer
    {
        #region IMGUI
        private string _error = "";

        // private bool _overrideMessageType;
        // private EMessageType _messageType;

        protected override bool WillDrawAbove(SerializedProperty property, ISaintsAttribute saintsAttribute)
        {
            InfoBoxAttribute infoboxAttribute = (InfoBoxAttribute)saintsAttribute;

            // ReSharper disable once ConvertIfStatementToReturnStatement
            if(!infoboxAttribute.Above)
            {
                return false;
            }

            (string error, bool willDraw) = WillDraw(infoboxAttribute, GetParentTarget(property));
            _error = error;
            return willDraw;
        }

        protected override float GetAboveExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute)
        {
            InfoBoxAttribute infoboxAttribute = (InfoBoxAttribute)saintsAttribute;
            if (!infoboxAttribute.Above)
            {
                return 0;
            }

            MetaInfo metaInfo = GetMetaInfo(infoboxAttribute, GetParentTarget(property));
            if (metaInfo.Error != "")
            {
                return 0;
            }

            return ImGuiHelpBox.GetHeight(metaInfo.Content, width, metaInfo.MessageType);
        }

        protected override Rect DrawAboveImGui(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            InfoBoxAttribute infoboxAttribute = (InfoBoxAttribute)saintsAttribute;
            MetaInfo metaInfo = GetMetaInfo(infoboxAttribute, GetParentTarget(property));
            return ImGuiHelpBox.Draw(position, metaInfo.Content, metaInfo.MessageType);
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute)
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

            (string error, bool willDraw) = WillDraw(infoboxAttribute, GetParentTarget(property));
            _error = error;
            return willDraw;
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute)
        {
            InfoBoxAttribute infoboxAttribute = (InfoBoxAttribute)saintsAttribute;

            float boxHeight = 0f;
            if (!infoboxAttribute.Above)
            {
                object parent = GetParentTarget(property);
                MetaInfo metaInfo = GetMetaInfo(infoboxAttribute, parent);
                _error = metaInfo.Error;
                if (metaInfo.WillDraw)
                {
                    boxHeight = ImGuiHelpBox.GetHeight(metaInfo.Content, width, metaInfo.MessageType);
                }
            }

            float errorHeight = _error != "" ? ImGuiHelpBox.GetHeight(_error, width, EMessageType.Error) : 0f;
            return boxHeight + errorHeight;
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            InfoBoxAttribute infoboxAttribute = (InfoBoxAttribute)saintsAttribute;
            MetaInfo metaInfo = GetMetaInfo(infoboxAttribute, GetParentTarget(property));
            Rect leftRect = ImGuiHelpBox.Draw(position, metaInfo.Content, metaInfo.MessageType);
            _error = metaInfo.Error;

            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (_error == "")
            {
                return leftRect;
            }

            return ImGuiHelpBox.Draw(leftRect, _error, EMessageType.Error);
        }

        #endregion

        private struct MetaInfo
        {
            public string Error;
            public EMessageType MessageType;
            public string Content;
            public bool WillDraw;
        }

        private static MetaInfo GetMetaInfo(InfoBoxAttribute infoboxAttribute, object target)
        {
            (string drawError, bool willDraw) = WillDraw(infoboxAttribute, target);
            if (drawError != "")
            {
                return new MetaInfo
                {
                    Error = drawError,
                };
            }

            if (!infoboxAttribute.ContentIsCallback)
            {
                return new MetaInfo
                {
                    Content = infoboxAttribute.Content,
                    MessageType = infoboxAttribute.MessageType,
                    WillDraw = willDraw,
                    Error = "",
                };
            }

            (ReflectUtils.GetPropType getPropType, object fieldOrMethodInfo) found = ReflectUtils.GetProp(target.GetType(), infoboxAttribute.Content);

            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (found.getPropType == ReflectUtils.GetPropType.NotFound)
            {
                return new MetaInfo
                {
                    Error = $"No field or method named `{infoboxAttribute.ShowCallback}` found on `{target}`",
                };
            }

            if (found.getPropType == ReflectUtils.GetPropType.Property && found.fieldOrMethodInfo is PropertyInfo propertyInfo)
            {
                object propertyValue = propertyInfo.GetValue(target);

                if (propertyValue is ValueTuple<EMessageType, string> resultTuple)
                {
                    return new MetaInfo
                    {
                        Error = "",
                        WillDraw = willDraw,
                        MessageType = resultTuple.Item1,
                        Content = resultTuple.Item2,
                    };
                }
                return new MetaInfo
                {
                    Error = "",
                    WillDraw = willDraw,
                    MessageType = infoboxAttribute.MessageType,
                    Content = propertyValue.ToString(),
                };
            }

            if (found.getPropType == ReflectUtils.GetPropType.Field && found.fieldOrMethodInfo is FieldInfo foundFieldInfo)
            {
                object fieldValue = foundFieldInfo.GetValue(target);

                if (fieldValue is ValueTuple<EMessageType, string> resultTuple)
                {
                    return new MetaInfo
                    {
                        Error = "",
                        WillDraw = willDraw,
                        MessageType = resultTuple.Item1,
                        Content = resultTuple.Item2,
                    };
                }

                return new MetaInfo
                {
                    Error = "",
                    WillDraw = willDraw,
                    MessageType = infoboxAttribute.MessageType,
                    Content = fieldValue.ToString(),
                };
            }
            // ReSharper disable once InvertIf
            if (found.getPropType == ReflectUtils.GetPropType.Method && found.fieldOrMethodInfo is MethodInfo methodInfo)
            {
                ParameterInfo[] methodParams = methodInfo.GetParameters();
                Debug.Assert(methodParams.All(p => p.IsOptional));

                if (methodInfo.ReturnType != typeof(ValueTuple<EMessageType, string>))
                    return new MetaInfo
                    {
                        Error = "",
                        WillDraw = willDraw,
                        MessageType = infoboxAttribute.MessageType,
                        Content = methodInfo.Invoke(target, methodParams.Select(p => p.DefaultValue).ToArray())
                            .ToString(),
                    };
                (EMessageType messageType, string content) = ((EMessageType, string))methodInfo.Invoke(target, methodParams.Select(p => p.DefaultValue).ToArray());
                return new MetaInfo
                {
                    Error = "",
                    WillDraw = willDraw,
                    MessageType = messageType,
                    Content = content,
                };
            }
            throw new ArgumentOutOfRangeException(nameof(found.getPropType), found.getPropType, null);
        }

        private static (string error, bool willDraw) WillDraw(InfoBoxAttribute infoboxAttribute, object target)
        {
            if (infoboxAttribute.ShowCallback == null)
            {
                return ("", true);
            }

            (ReflectUtils.GetPropType getPropType, object fieldOrMethodInfo) found = ReflectUtils.GetProp(target.GetType(), infoboxAttribute.ShowCallback);

            if (found.getPropType == ReflectUtils.GetPropType.NotFound)
            {
                return ($"No field or method named `{infoboxAttribute.ShowCallback}` found on `{target}`", false);
            }

            if (found.getPropType == ReflectUtils.GetPropType.Property && found.fieldOrMethodInfo is PropertyInfo propertyInfo)
            {
                if (!ReflectUtils.Truly(propertyInfo.GetValue(target)))
                {
                    return ("", false);
                }
            }
            else if (found.getPropType == ReflectUtils.GetPropType.Field && found.fieldOrMethodInfo is FieldInfo foundFieldInfo)
            {
                if (!ReflectUtils.Truly(foundFieldInfo.GetValue(target)))
                {
                    return ("", false);
                }
            }
            else if (found.getPropType == ReflectUtils.GetPropType.Method && found.fieldOrMethodInfo is MethodInfo methodInfo)
            {
                ParameterInfo[] methodParams = methodInfo.GetParameters();
                Debug.Assert(methodParams.All(p => p.IsOptional));
                Debug.Assert(methodInfo.ReturnType == typeof(bool));
                if (!(bool)methodInfo.Invoke(target, methodParams.Select(p => p.DefaultValue).ToArray()))
                {
                    return ("", false);
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(found), found, null);
            }

            return ("", true);

        }

        #region UIToolkit

        private static string NameInfoBox(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__InfoBox";
        private static string NameHelpBox(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__InfoBoxHelpBox";

        protected override VisualElement CreateAboveUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, object parent)
        {
            InfoBoxAttribute infoboxAttribute = (InfoBoxAttribute)saintsAttribute;
            if (!infoboxAttribute.Above)
            {
                return null;
            }

            return new HelpBox
            {
                name = NameInfoBox(property, index),
                style =
                {
                    display = DisplayStyle.None,
                },
                userData = new MetaInfo
                {
                    Content = "",
                    Error = "",
                    MessageType = EMessageType.None,
                },
            };
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
                VisualElement container,
                object parent)
        {
            HelpBox errorBox = new HelpBox
            {
                name = NameHelpBox(property, index),
                style =
                {
                    display = DisplayStyle.None,
                },
            };

            InfoBoxAttribute infoboxAttribute = (InfoBoxAttribute)saintsAttribute;
            if(infoboxAttribute.Above)
            {
                return errorBox;
            }

            VisualElement root = new VisualElement();

            root.Add(new HelpBox
            {
                name = NameInfoBox(property, index),
                style =
                {
                    display = DisplayStyle.None,
                },
                userData = new MetaInfo
                {
                    Content = "",
                    Error = "",
                    MessageType = EMessageType.None,
                },
            });
            root.Add(errorBox);

            // OnUpdateUiToolKit(property, saintsAttribute, index, root, parent);
            return root;
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChangedCallback, object parent)
        {
            MetaInfo metaInfo = GetMetaInfo((InfoBoxAttribute)saintsAttribute, parent);
            HelpBox infoBox = container.Q<HelpBox>(NameInfoBox(property, index));
            MetaInfo oriMetaInfo = (MetaInfo)infoBox.userData;

            bool changed = false;
            if (oriMetaInfo.Error != metaInfo.Error)
            {
                changed = true;
                HelpBox errorBox = container.Q<HelpBox>(NameHelpBox(property, index));
                errorBox.style.display = metaInfo.Error != "" ? DisplayStyle.Flex : DisplayStyle.None;
                errorBox.text = metaInfo.Error;
            }

            if(oriMetaInfo.MessageType != metaInfo.MessageType || oriMetaInfo.Content != metaInfo.Content || oriMetaInfo.WillDraw != metaInfo.WillDraw)
            {

                Debug.Log($"change box {NameInfoBox(property, index)}: {metaInfo.MessageType} {metaInfo.Content} {metaInfo.WillDraw}");
                changed = true;
                infoBox.style.display = metaInfo.WillDraw ? DisplayStyle.Flex : DisplayStyle.None;
                infoBox.text = metaInfo.Content;
                infoBox.messageType = metaInfo.MessageType.GetUIToolkitMessageType();
            }

            if (changed)
            {
                infoBox.userData = metaInfo;
            }
        }

        #endregion
    }
}

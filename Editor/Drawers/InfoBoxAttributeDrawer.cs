using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
#if UNITY_2021_3_OR_NEWER
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(InfoBoxAttribute))]
    public class InfoBoxAttributeDrawer: SaintsPropertyDrawer
    {
        #region IMGUI
        private string _error = "";

        // private bool _overrideMessageType;
        // private EMessageType _messageType;

        protected override bool WillDrawAbove(SerializedProperty property, ISaintsAttribute saintsAttribute,
            FieldInfo info,
            object parent)
        {
            InfoBoxAttribute infoboxAttribute = (InfoBoxAttribute)saintsAttribute;

            // ReSharper disable once ConvertIfStatementToReturnStatement
            if(!infoboxAttribute.Above)
            {
                return false;
            }

            (string error, bool willDraw) = WillDraw(infoboxAttribute, parent);
            _error = error;
            return willDraw;
        }

        protected override float GetAboveExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            InfoBoxAttribute infoboxAttribute = (InfoBoxAttribute)saintsAttribute;
            if (!infoboxAttribute.Above)
            {
                return 0;
            }

            MetaInfo metaInfo = GetMetaInfo(property, infoboxAttribute, info, parent);
            if (metaInfo.Error != "")
            {
                return 0;
            }

            return ImGuiHelpBox.GetHeight(metaInfo.Content, width, metaInfo.MessageType);
        }

        protected override Rect DrawAboveImGui(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            InfoBoxAttribute infoboxAttribute = (InfoBoxAttribute)saintsAttribute;
            MetaInfo metaInfo = GetMetaInfo(property, infoboxAttribute, info, parent);
            return ImGuiHelpBox.Draw(position, metaInfo.Content, metaInfo.MessageType);
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            FieldInfo info,
            object parent)
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

            (string error, bool willDraw) = WillDraw(infoboxAttribute, parent);
            _error = error;
            return willDraw;
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            InfoBoxAttribute infoboxAttribute = (InfoBoxAttribute)saintsAttribute;

            float boxHeight = 0f;
            if (!infoboxAttribute.Above)
            {
                MetaInfo metaInfo = GetMetaInfo(property, infoboxAttribute, info, parent);
                _error = metaInfo.Error;
                if (metaInfo.WillDrawBox)
                {
                    boxHeight = ImGuiHelpBox.GetHeight(metaInfo.Content, width, metaInfo.MessageType);
                }
            }

            float errorHeight = _error != "" ? ImGuiHelpBox.GetHeight(_error, width, EMessageType.Error) : 0f;
            return boxHeight + errorHeight;
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            InfoBoxAttribute infoboxAttribute = (InfoBoxAttribute)saintsAttribute;
            MetaInfo metaInfo = GetMetaInfo(property, infoboxAttribute, info, parent);
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
            // ReSharper disable InconsistentNaming
            public string Error;
            public EMessageType MessageType;
            public string Content;
            public bool WillDrawBox;
            // ReSharper enable InconsistentNaming
        }

        private static MetaInfo GetMetaInfo(SerializedProperty property, InfoBoxAttribute infoboxAttribute,
            FieldInfo fieldInfo, object target)
        {
            (string drawError, bool willDraw) = WillDraw(infoboxAttribute, target);
            if (drawError != "")
            {
                return new MetaInfo
                {
                    Error = drawError,
                };
            }

            if (!infoboxAttribute.isCallback)
            {
                return new MetaInfo
                {
                    Content = infoboxAttribute.Content,
                    MessageType = infoboxAttribute.MessageType,
                    WillDrawBox = infoboxAttribute.Content != null && willDraw,
                    Error = "",
                };
            }

            List<Type> types = ReflectUtils.GetSelfAndBaseTypes(target);
            types.Reverse();
            foreach (Type eachType in types)
            {
                (ReflectUtils.GetPropType getPropType, object fieldOrMethodInfo) =
                    ReflectUtils.GetProp(eachType, infoboxAttribute.Content);
                switch (getPropType)
                {
                    case ReflectUtils.GetPropType.Field:
                    {
                        object fieldValue = ((FieldInfo)fieldOrMethodInfo).GetValue(target);

                        if (fieldValue is ValueTuple<EMessageType, string> resultTuple)
                        {
                            return new MetaInfo
                            {
                                Error = "",
                                WillDrawBox = resultTuple.Item2 != null && willDraw,
                                MessageType = resultTuple.Item1,
                                Content = resultTuple.Item2,
                            };
                        }

                        return new MetaInfo
                        {
                            Error = "",
                            WillDrawBox = fieldValue != null && willDraw,
                            MessageType = infoboxAttribute.MessageType,
                            Content = fieldValue == null ? "" : fieldValue.ToString(),
                        };
                    }
                    case ReflectUtils.GetPropType.Property:
                    {
                        object propertyValue = ((PropertyInfo)fieldOrMethodInfo).GetValue(target);

                        if (propertyValue is ValueTuple<EMessageType, string> resultTuple)
                        {
                            return new MetaInfo
                            {
                                Error = "",
                                WillDrawBox = resultTuple.Item2 != null && willDraw,
                                MessageType = resultTuple.Item1,
                                Content = resultTuple.Item2,
                            };
                        }

                        return new MetaInfo
                        {
                            Error = "",
                            WillDrawBox = propertyValue != null && willDraw,
                            MessageType = infoboxAttribute.MessageType,
                            Content = propertyValue?.ToString(),
                        };
                    }
                    case ReflectUtils.GetPropType.Method:
                    {
                        MethodInfo methodInfo = (MethodInfo)fieldOrMethodInfo;
                        ParameterInfo[] methodParams = methodInfo.GetParameters();
                        Debug.Assert(methodParams.All(p => p.IsOptional));

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_INFO_BOX
                        Debug.Log(methodInfo.ReturnType);
                        Debug.Log(methodInfo.ReturnType == typeof(ValueTuple<EMessageType, string>));
#endif
                        int arrayIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);
                        object rawValue = fieldInfo.GetValue(target);
                        object curValue = arrayIndex == -1
                            ? rawValue
                            : SerializedUtils.GetValueAtIndex(rawValue, arrayIndex);

                        object[] passParams = ReflectUtils.MethodParamsFill(methodInfo.GetParameters(), arrayIndex == -1
                            ? new[]
                            {
                                curValue,
                            }
                            : new[]
                            {
                                curValue,
                                arrayIndex,
                            });


                        if (methodInfo.ReturnType != typeof(ValueTuple<EMessageType, string>))
                        {
                            object methodDirectResult = methodInfo.Invoke(target, passParams);
                            return new MetaInfo
                            {
                                Error = "",
                                WillDrawBox = willDraw && methodDirectResult != null,
                                MessageType = infoboxAttribute.MessageType,
                                Content = methodDirectResult?.ToString(),
                            };
                        }

                        (EMessageType messageType, string content) =
                            ((EMessageType, string))methodInfo.Invoke(target,
                                methodParams.Select(p => p.DefaultValue).ToArray());
                        return new MetaInfo
                        {
                            Error = "",
                            WillDrawBox = willDraw && content != null,
                            MessageType = messageType,
                            Content = content,
                        };
                    }
                    case ReflectUtils.GetPropType.NotFound:
                        continue;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(getPropType), getPropType, null);
                }
            }

            return new MetaInfo
            {
                Error = $"No field or method named `{infoboxAttribute.ShowCallback}` found on `{target}`",
            };
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

#if UNITY_2021_3_OR_NEWER

        #region UIToolkit

        private static string NameInfoBox(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__InfoBox";
        private static string NameHelpBox(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__InfoBoxHelpBox";

        protected override VisualElement CreateAboveUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
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

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            VisualElement container,
            FieldInfo info,
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
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            MetaInfo metaInfo = GetMetaInfo(property, (InfoBoxAttribute)saintsAttribute, info, parent);
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

            if(oriMetaInfo.MessageType != metaInfo.MessageType || oriMetaInfo.Content != metaInfo.Content || oriMetaInfo.WillDrawBox != metaInfo.WillDrawBox)
            {

                // Debug.Log($"change box {NameInfoBox(property, index)}: {metaInfo.MessageType} {metaInfo.Content} {metaInfo.WillDraw}");
                changed = true;
                infoBox.style.display = metaInfo.WillDrawBox ? DisplayStyle.Flex : DisplayStyle.None;
                infoBox.text = metaInfo.Content;
                infoBox.messageType = metaInfo.MessageType.GetUIToolkitMessageType();
            }

            if (changed)
            {
                infoBox.userData = metaInfo;
            }
        }

        #endregion

#endif
    }
}

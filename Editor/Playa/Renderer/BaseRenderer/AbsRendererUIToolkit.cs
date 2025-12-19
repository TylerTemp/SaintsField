#if UNITY_2021_3_OR_NEWER //&& !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.AnimatorParamDrawer;
using SaintsField.Editor.Drawers.AnimatorStateDrawer;
using SaintsField.Editor.Drawers.CurveRangeDrawer;
using SaintsField.Editor.Drawers.DateTimeDrawer;
using SaintsField.Editor.Drawers.EnumFlagsDrawers.EnumToggleButtonsDrawer;
using SaintsField.Editor.Drawers.GuidDrawer;
using SaintsField.Editor.Drawers.InputAxisDrawer;
using SaintsField.Editor.Drawers.LayerDrawer;
using SaintsField.Editor.Drawers.MinMaxSliderDrawer;
using SaintsField.Editor.Drawers.ProgressBarDrawer;
using SaintsField.Editor.Drawers.PropRangeDrawer;
using SaintsField.Editor.Drawers.RateDrawer;
using SaintsField.Editor.Drawers.ReferencePicker;
using SaintsField.Editor.Drawers.ResizableTextAreaDrawer;
using SaintsField.Editor.Drawers.SaintsDictionary;
using SaintsField.Editor.Drawers.SceneDrawer;
#if UNITY_2021_2_OR_NEWER
using SaintsField.Editor.Drawers.ShaderDrawers.ShaderKeywordDrawer;
using SaintsField.Editor.Drawers.ShaderDrawers.ShaderParamDrawer;
#endif
using SaintsField.Editor.Drawers.SortingLayerDrawer;
using SaintsField.Editor.Drawers.TagDrawer;
using SaintsField.Editor.Drawers.TimeSpanDrawer;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using SaintsField.Editor.Drawers.ValueButtonsDrawer;
using SaintsField.Editor.Playa.Renderer.ListDrawerSettings;
using Saintsfield.Editor.Playa.Renderer.ShowInInspectorFieldFakeRenderer;
using SaintsField.Editor.Utils;
using SaintsField.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Playa.Renderer.BaseRenderer
{
    public abstract partial class AbsRenderer: IRichTextTagProvider
    {
        private const string ClassSaintsFieldPlaya = "saintsfield-playa";
        public const string ClassSaintsFieldEditingDisabled = "saintsfield-editing-disabled";
        public const string ClassSaintsFieldPlayaContainer = ClassSaintsFieldPlaya + "-container";

        private VisualElement _rootElement;

        public virtual VisualElement CreateVisualElement()
        {
            int flexGrow;
            if (InDirectHorizontalLayout)
            {
                flexGrow = 1;
            }
            else
            {
                flexGrow = InAnyHorizontalLayout ? 0 : 1;
            }

            VisualElement root = new VisualElement
            {
                style =
                {
                    // flexGrow = 1,
                    // flexGrow = InAnyHorizontalLayout? 0: 1,
                    flexGrow = flexGrow,
                    width = new StyleLength(Length.Percent(100)),
                },
                name = ToString(),
            };
            root.AddToClassList(ClassSaintsFieldPlaya);
            bool hasAnyChildren = false;

            (VisualElement target, bool targetNeedUpdate) = CreateTargetUIToolkit(root);
            if (target != null)
            {
                VisualElement targetContainer = new VisualElement
                {
                    style =
                    {
                        flexGrow = 1,
                        flexShrink = 0,

                        width = new StyleLength(Length.Percent(100)),
                    },
                };
                targetContainer.AddToClassList(ClassSaintsFieldPlayaContainer);
                targetContainer.Add(target);
                root.Add(targetContainer);
                hasAnyChildren = true;
            }

            if (!targetNeedUpdate)
            {
                GUIColorAttribute guiColor = FieldWithInfo.PlayaAttributes?.OfType<GUIColorAttribute>().FirstOrDefault();
                if (guiColor != null)
                {
                    if (guiColor.IsCallback)
                    {
                        targetNeedUpdate = true;
                    }
                    else  // we need to update at least once to apply the color
                    {
                        UIToolkitUtils.OnAttachToPanelOnce(root, _ =>
                        {
                            root.schedule.Execute(() => OnUpdateUIToolKit(_rootElement));
                        });
                    }
                }
            }

            if (targetNeedUpdate)
            {
                UIToolkitUtils.OnAttachToPanelOnce(root, _ =>
                {
                    root.schedule.Execute(() => OnUpdateUIToolKit(_rootElement));
                    root.schedule.Execute(() => OnUpdateUIToolKit(_rootElement)).Every(100);
                });
            }
            if(targetNeedUpdate || hasAnyChildren)
            {
                return _rootElement = root;
            }

            return null;
        }

        protected abstract (VisualElement target, bool needUpdate) CreateTargetUIToolkit(VisualElement container);

        private static void MergeIntoGroup(Dictionary<string, VisualElement> groupElements, string groupBy, VisualElement root, VisualElement child)
        {
            if (string.IsNullOrEmpty(groupBy))
            {
                root.Add(child);
                return;
            }

            bool exists = groupElements.TryGetValue(groupBy, out VisualElement groupElement);
            if (!exists)
            {
                groupElement = new VisualElement
                {
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                    }
                };
                groupElement.AddToClassList($"{ClassSaintsFieldPlaya}-group-{groupBy}");
                groupElements.Add(groupBy, groupElement);
                root.Add(groupElement);
            }

            groupElement.Add(child);
        }

        private class InfoBoxUserData
        {
            public string XmlContent;
            public EMessageType MessageType;

            public InfoBoxAttribute InfoBoxAttribute;
            public SaintsFieldWithInfo FieldWithInfo;
            public RichTextDrawer RichTextDrawer;
        }


        protected virtual PreCheckResult OnUpdateUIToolKit(VisualElement root)
        {
            return UpdatePreCheckUIToolkitInternal(FieldWithInfo, _rootElement);
        }

        // protected PreCheckResult HelperOnUpdateUIToolKitRawBase()
        // {
        //     return UpdatePreCheckUIToolkit();
        // }

        // protected PreCheckResult UpdatePreCheckUIToolkit()
        // {
        //
        // }

        private Color _preColor;

        private PreCheckResult UpdatePreCheckUIToolkitInternal(SaintsFieldWithInfo fieldWithInfo, VisualElement result)
        {
            PreCheckResult preCheckResult = GetPreCheckResult(fieldWithInfo, false);
            // Debug.Log($"{preCheckResult.HasGuiColor}/{preCheckResult.GuiColor}");
            if(result.enabledSelf != !preCheckResult.IsDisabled)
            {
                result.SetEnabled(!preCheckResult.IsDisabled);
            }

            bool isShown = result.style.display != DisplayStyle.None;

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_PLAYA_IS_SHOWN
            Debug.Log($"{fieldWithInfo} {result.name} isShown={isShown}, preCheckIsShown={preCheckResult.IsShown}");
#endif

            if(isShown != preCheckResult.IsShown)
            {
                result.style.display = preCheckResult.IsShown ? DisplayStyle.Flex : DisplayStyle.None;
            }

            ApplyGuiColor(result);

            return preCheckResult;
        }

        public static readonly Color ReColor = EColor.EditorSeparator.GetColor();

        // before set: useful for struct editing that C# will mess-up and change the value of the reference you have

        private static readonly Type[] SkipTypes = { typeof(IntPtr), typeof(UIntPtr), typeof(void) };

        public static bool SkipTypeDrawing(Type checkType)
        {
            foreach (Type disallowType in SkipTypes)
            {
                if (disallowType.IsAssignableFrom(checkType))
                {
                    return true;
                }
            }

            return false;
        }

        public static string GetDropdownTypeLabel(Type type)
        {
            return type == null
                ? "null"
                : $"{type.Name}: <color=#{ColorUtility.ToHtmlStringRGB(EColor.Gray.GetColor())}>{type.Namespace}</color>";
        }

        public string GetLabel()
        {
            switch (FieldWithInfo.RenderType)
            {
                case SaintsRenderType.SerializedField:
                case SaintsRenderType.InjectedSerializedField:
                {
                    // ReSharper disable once ConvertIfStatementToReturnStatement
                    if (SerializedUtils.IsOk(FieldWithInfo.SerializedProperty))
                    {
                        return FieldWithInfo.SerializedProperty.displayName;
                    }

                    return "";
                }
                case SaintsRenderType.NonSerializedField:
                {
                    if (FieldWithInfo.FieldInfo != null)
                    {
                        return ObjectNames.NicifyVariableName(FieldWithInfo.FieldInfo.Name);
                    }

                    return "";
                }
                case SaintsRenderType.Method:
                    return ObjectNames.NicifyVariableName(FieldWithInfo.MethodInfo.Name);
                case SaintsRenderType.NativeProperty:
                    return ObjectNames.NicifyVariableName(FieldWithInfo.PropertyInfo.Name);
                case SaintsRenderType.ClassStruct:
                    return ObjectNames.NicifyVariableName(FieldWithInfo.ClassStructType.Name);
                case SaintsRenderType.Other:
                    return "";
                default:
                    throw new ArgumentOutOfRangeException(nameof(FieldWithInfo.RenderType), FieldWithInfo.RenderType, null);
            }
        }

        public string GetContainerType()
        {
            return GetTargetType().Name;
        }

        private Type GetTargetType()
        {
            return  FieldWithInfo.ClassStructType ?? FieldWithInfo.Targets[0].GetType();
        }

        public string GetContainerTypeBaseType()
        {
            return GetTargetType().BaseType?.Name ?? "";
        }

        public string GetIndex(string formatter)
        {
            switch (FieldWithInfo.RenderType)
            {
                case SaintsRenderType.SerializedField:
                case SaintsRenderType.InjectedSerializedField:
                {
                    // ReSharper disable once ConvertIfStatementToReturnStatement
                    if (!SerializedUtils.IsOk(FieldWithInfo.SerializedProperty))
                    {
                        return "";
                    }

                    int propPath = SerializedUtils.PropertyPathIndex(FieldWithInfo.SerializedProperty.propertyPath);
                    return propPath < 0 ? "" : propPath.ToString();
                }
                case SaintsRenderType.NonSerializedField:
                case SaintsRenderType.Method:
                case SaintsRenderType.NativeProperty:
                case SaintsRenderType.ClassStruct:
                case SaintsRenderType.Other:
                    return "";
                default:
                    throw new ArgumentOutOfRangeException(nameof(FieldWithInfo.RenderType), FieldWithInfo.RenderType, null);
            }
        }

        public string GetField(string rawContent, string tagName, string tagValue)
        {
            switch (FieldWithInfo.RenderType)
            {
                case SaintsRenderType.SerializedField:
                case SaintsRenderType.InjectedSerializedField:
                {
                    if (!SerializedUtils.IsOk(FieldWithInfo.SerializedProperty))
                    {
                        return "";
                    }

                    bool hasError = false;

                    (string error, int index, object value) result = Util.GetValue(FieldWithInfo.SerializedProperty, FieldWithInfo.FieldInfo, FieldWithInfo.Targets[0]);
                    (string error, int index, object value) accResult = result;
                    if (tagName == "field")
                    {
                        if (result.error != "")
                        {
                            hasError = true;
                        }
                    }
                    else
                    {
                        string revName = tagName["field.".Length..];

                        // string[] subFields = revName.Split(SerializedUtils.DotSplitSeparator);
                        // object accParent = FieldWithInfo.Targets[0];

                        (string error, object result) getOfValue = Util.GetOf<object>(revName, null, FieldWithInfo.SerializedProperty,
                            FieldWithInfo.FieldInfo, FieldWithInfo.Targets[0], null);

                        hasError = getOfValue.error != "";
                        accResult = (getOfValue.error, accResult.index, getOfValue.result);
                    }

                    // ReSharper disable once ConvertIfStatementToReturnStatement
                    if (hasError)
                    {
                        return rawContent;
                    }

                    return RichTextDrawer.TagStringFormatter(accResult.value, tagValue);
                }
                case SaintsRenderType.NonSerializedField:
                case SaintsRenderType.Method:
                case SaintsRenderType.NativeProperty:
                case SaintsRenderType.ClassStruct:
                case SaintsRenderType.Other:
                    return "";
                default:
                    throw new ArgumentOutOfRangeException(nameof(FieldWithInfo.RenderType), FieldWithInfo.RenderType, null);
            }
        }
    }
}
#endif

#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.ProgressBarDrawer
{
    public partial class ProgressBarAttributeDrawer
    {
        private class UIToolkitPayload
        {
            // ReSharper disable once InconsistentNaming
            public readonly VisualElement Background;

            // ReSharper disable once InconsistentNaming
            public readonly VisualElement Progress;
            public MetaInfo MetaInfo;

            public UIToolkitPayload(VisualElement background, VisualElement progress, MetaInfo metaInfo)
            {
                Background = background;
                Progress = progress;
                MetaInfo = metaInfo;
            }
        }

        public class ProgressBarField : BaseField<float>
        {
            public ProgressBarField(string label, VisualElement visualInput) : base(label, visualInput)
            {
            }
        }

        private static string NameProgressBarField(SerializedProperty property) =>
            $"{property.propertyPath}__ProgressBarField";
        private static string NameProgressBar(SerializedProperty property) => $"{property.propertyPath}__ProgressBar";

        private static string NameHelpBox(SerializedProperty property) =>
            $"{property.propertyPath}__ProgressBar_HelpBox";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            ProgressBarAttribute progressBarAttribute = (ProgressBarAttribute)saintsAttribute;

            MetaInfo metaInfo = GetMetaInfo(property, saintsAttribute, info, parent);

            #region ProgrssBar

            float propValue = property.propertyType == SerializedPropertyType.Integer
                ? property.intValue
                : property.floatValue;

            ProgressBar progressBar = new ProgressBar
            {
                name = NameProgressBar(property),

                title = GetTitle(property, progressBarAttribute.TitleCallback, progressBarAttribute.Step, propValue,
                    metaInfo.Min, metaInfo.Max, parent).title,
                lowValue = 0,
                highValue = metaInfo.Max - metaInfo.Min,
                value = propValue - metaInfo.Min,

                style =
                {
                    flexGrow = 1,
                },
            };

            Type type = typeof(AbstractProgressBar);
            FieldInfo backgroundFieldInfo =
                type.GetField("m_Background", BindingFlags.NonPublic | BindingFlags.Instance);

            VisualElement background = null;
            if (backgroundFieldInfo != null)
            {
                background = (VisualElement)backgroundFieldInfo.GetValue(progressBar);
                background.style.backgroundColor = metaInfo.BackgroundColor;
            }

            FieldInfo progressFieldInfo = type.GetField("m_Progress", BindingFlags.NonPublic | BindingFlags.Instance);
            VisualElement progress = null;
            if (progressFieldInfo != null)
            {
                progress = (VisualElement)progressFieldInfo.GetValue(progressBar);
                progress.style.backgroundColor = metaInfo.Color;
            }

            progressBar.userData = new UIToolkitPayload(background, progress, metaInfo);

            #endregion

            ProgressBarField progressBarField = new ProgressBarField(GetPreferredLabel(property), progressBar)
            {
                name = NameProgressBarField(property),
            };
            progressBarField.AddToClassList(ProgressBarField.alignedFieldUssClassName);

            progressBarField.AddToClassList(ClassAllowDisable);
            progressBarField.BindProperty(property);

            return progressBarField;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            HelpBox helpBox = new HelpBox("", HelpBoxMessageType.Error)
            {
                name = NameHelpBox(property),
                style =
                {
                    display = DisplayStyle.None,
                },
            };
            helpBox.AddToClassList(ClassAllowDisable);
            return helpBox;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            ProgressBarField progressBarField = container.Q<ProgressBarField>(name: NameProgressBarField(property));
            UIToolkitUtils.AddContextualMenuManipulator(progressBarField.labelElement, property, () => Util.PropertyChangedCallback(property, info, onValueChangedCallback));

            ProgressBar progressBar = container.Q<ProgressBar>(NameProgressBar(property));

            progressBar.RegisterCallback<PointerDownEvent>(evt =>
            {
                progressBar.CapturePointer(0);
                OnProgressBarInteract(property, (ProgressBarAttribute)saintsAttribute, container, progressBar,
                    evt.localPosition, onValueChangedCallback, info, parent);
            });
            progressBar.RegisterCallback<PointerUpEvent>(_ => { progressBar.ReleasePointer(0); });
            progressBar.RegisterCallback<PointerMoveEvent>(evt =>
            {
                if (progressBar.HasPointerCapture(0))
                {
                    OnProgressBarInteract(property, (ProgressBarAttribute)saintsAttribute, container, progressBar,
                        evt.localPosition, onValueChangedCallback, info, parent);
                }
            });
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, Action<object> onValueChanged, FieldInfo info)
        {
            object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
            if (parent == null)
            {
                Debug.LogWarning($"{property.propertyPath} parent disposed unexpectedly.");
                return;
            }

            MetaInfo metaInfo = GetMetaInfo(property, saintsAttribute, info, parent);

            ProgressBar progressBar = container.Q<ProgressBar>(NameProgressBar(property));
            UIToolkitPayload uiToolkitPayload = (UIToolkitPayload)progressBar.userData;
            MetaInfo oldMetaInfo = uiToolkitPayload.MetaInfo;

            bool changed = false;
            string error = metaInfo.Error;
            float propValue = metaInfo.Value;
            float progressValue = progressBar.value - metaInfo.Min;

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (metaInfo.Min != oldMetaInfo.Min
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                || metaInfo.Max != oldMetaInfo.Max
                || !Mathf.Approximately(progressValue, propValue))
            {
                changed = true;
                progressBar.highValue = metaInfo.Max - metaInfo.Min;

                ProgressBarAttribute progressBarAttribute = (ProgressBarAttribute)saintsAttribute;

                if (propValue < metaInfo.Min || propValue > metaInfo.Max)
                {
                    // Debug.Log($"update change: {metaInfo.Min} <= {propValue} <= {metaInfo.Max}");
                    propValue = ChangeValue(property, progressBarAttribute, container, progressBar,
                        Mathf.Clamp(propValue, metaInfo.Min, metaInfo.Max), metaInfo.Min, metaInfo.Max,
                        onValueChanged, info, parent);
                    // Debug.Log($"now prop = {propValue}");
                }

                progressBar.value = propValue - metaInfo.Min;

                (string titleError, string title) = GetTitle(property, progressBarAttribute.TitleCallback,
                    progressBarAttribute.Step, propValue, metaInfo.Min, metaInfo.Max, parent);
                // Debug.Log($"update change title {title}");
                progressBar.title = title;
                if (titleError != "")
                {
                    error = titleError;
                }
            }

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_PROGRESS_BAR
            Debug.Log($"oldColor={oldMetaInfo.Color}/newColor={metaInfo.Color}, progress={uiToolkitPayload.Progress}");
#endif
            if (metaInfo.Color != oldMetaInfo.Color && uiToolkitPayload.Progress != null)
            {
                changed = true;
                uiToolkitPayload.Progress.style.backgroundColor = metaInfo.Color;
            }

            if (metaInfo.BackgroundColor != oldMetaInfo.BackgroundColor && uiToolkitPayload.Background != null)
            {
                changed = true;
                uiToolkitPayload.Background.style.backgroundColor = metaInfo.BackgroundColor;
            }

            if (changed)
            {
                // progressBar.userData = metaInfo;
                uiToolkitPayload.MetaInfo = metaInfo;
            }

            UpdateHelpBox(property, container, error);
        }

        private static void OnProgressBarInteract(SerializedProperty property,
            ProgressBarAttribute progressBarAttribute, VisualElement container, ProgressBar progressBar,
            Vector3 mousePosition, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            float curWidth = progressBar.resolvedStyle.width;
            if (float.IsNaN(curWidth))
            {
                return;
            }

            UIToolkitPayload uiToolkitPayload = (UIToolkitPayload)progressBar.userData;

            float curValue = Mathf.Lerp(uiToolkitPayload.MetaInfo.Min, uiToolkitPayload.MetaInfo.Max,
                mousePosition.x / curWidth);
            // Debug.Log(curValue);
            ChangeValue(property, progressBarAttribute, container, progressBar, curValue, uiToolkitPayload.MetaInfo.Min,
                uiToolkitPayload.MetaInfo.Max, onValueChangedCallback, info, parent);
        }

        private static float ChangeValue(SerializedProperty property, ProgressBarAttribute progressBarAttribute,
            VisualElement container, ProgressBar progressBar, float curValue, float minValue, float maxValue,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            // UIToolkitPayload uiToolkitPayload = (UIToolkitPayload)progressBar.userData;

            bool isInt = property.propertyType == SerializedPropertyType.Integer;

            float newValue = BoundValue(curValue, minValue, maxValue, progressBarAttribute.Step, isInt);

            // float wrapNewValue =

            // Debug.Log($"curValue={curValue}, newValue={newValue}");
            float propValue = isInt
                ? property.intValue
                : property.floatValue;

            // Debug.Log($"curValue={curValue}, newValue={newValue}, propValue={propValue}");
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (propValue == newValue)
            {
                return propValue;
            }

            if (property.propertyType == SerializedPropertyType.Integer)
            {
                int intValue = (int)newValue;
                ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent,
                    intValue);
                property.intValue = intValue;
                property.serializedObject.ApplyModifiedProperties();
                progressBar.SetValueWithoutNotify(intValue - minValue);
                onValueChangedCallback.Invoke(intValue);
            }
            else
            {
                ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent,
                    newValue);
                property.floatValue = newValue;
                property.serializedObject.ApplyModifiedProperties();
                progressBar.SetValueWithoutNotify(newValue - minValue);
                onValueChangedCallback.Invoke(newValue);
            }

            (string error, string title) = GetTitle(property, progressBarAttribute.TitleCallback,
                progressBarAttribute.Step, newValue, minValue, maxValue, parent);
            // Debug.Log($"change title to {title}");
            progressBar.title = title;
            UpdateHelpBox(property, container, error);

            return newValue;
        }

        private static void UpdateHelpBox(SerializedProperty property, VisualElement container, string error)
        {
            HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property));
            if (helpBox.text == error)
            {
                return;
            }

            helpBox.text = error;
            helpBox.style.display = string.IsNullOrEmpty(error) ? DisplayStyle.None : DisplayStyle.Flex;
        }
    }
}
#endif

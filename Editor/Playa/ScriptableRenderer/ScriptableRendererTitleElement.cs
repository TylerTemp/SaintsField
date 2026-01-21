using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.ScriptableRenderer
{
#if UNITY_6000_0_OR_NEWER
    [UxmlElement]
#endif
    public partial class ScriptableRendererTitleElement: VisualElement
    {
#if !UNITY_6000_0_OR_NEWER
        // public new class UxmlTraits : BindableElement.UxmlTraits { }
        public new class UxmlFactory : UxmlFactory<ScriptableRendererTitleElement, UxmlTraits> { }
#endif

        private static VisualTreeAsset _template;
        private static readonly Dictionary<string, bool> CustomViewDataCache = new Dictionary<string, bool>();

        private readonly VisualElement _foldoutIcon;
        private bool _expanded = true;

        public ScriptableRendererTitleElement() : this(null, null)
        {
        }

        public ScriptableRendererTitleElement(SerializedObject rendererFeatureSo, Action onRemove)
        {
            _template ??= Util.LoadResource<VisualTreeAsset>("UIToolkit/ScriptableRenderer/Title.uxml");
            TemplateContainer element = _template.CloneTree();
            hierarchy.Add(element);

            contentContainer = element.Q<VisualElement>(name: "content");

            Button titleButton = element.Q<Button>(name: "titleButton");
            titleButton.clicked += () =>
            {
                _expanded = !_expanded;
                RefreshExpand();
            };

            _foldoutIcon = element.Q<VisualElement>(name: "foldoutIcon");

            #region contextMenuButton
            Button contextMenuButton = element.Q<Button>(name: "contextMenuButton");
            contextMenuButton.clicked += () =>
            {
                GenericDropdownMenu genericDropdownMenu = new GenericDropdownMenu();

                genericDropdownMenu.AddItem("Remove", false, onRemove.Invoke);

#if SAINTSFIELD_RENDER_PIPELINE_UNIVERSAL_17
                if(rendererFeatureSo?.targetObject?.GetType() == typeof(FullScreenPassRendererFeature))
                {
                    genericDropdownMenu.AddSeparator("");
                    genericDropdownMenu.AddItem("Show All Advanced Properties", UnityEditor.Rendering.AdvancedProperties.enabled, () => UnityEditor.Rendering.AdvancedProperties.enabled = !UnityEditor.Rendering.AdvancedProperties.enabled);
                }
#endif

                genericDropdownMenu.DropDown(
                    contextMenuButton.worldBound,
                    contextMenuButton,
#if UNITY_6000_3_OR_NEWER
                    DropdownMenuSizeMode.Auto
#else
                    true
#endif
                );
            };
            #endregion

            Label titleLabel = element.Q<Label>(name: "titleLabel");

            if (rendererFeatureSo == null)
            {
                _expanded = true;
                RefreshExpand();
                titleLabel.text = "Missing";
                return;
            }

            SerializedProperty activeProperty = rendererFeatureSo.FindProperty("m_Active");
            Toggle toggleActive = element.Q<Toggle>(name: "toggleActive");
            toggleActive.bindingPath = activeProperty.propertyPath;
            toggleActive.TrackPropertyValue(activeProperty, p =>
            {
                bool active = p.boolValue;
                titleLabel.style.color = active ? Color.white : Color.gray;
            });
            titleLabel.style.color = activeProperty.boolValue ? Color.white : Color.gray;

            SerializedProperty nameProperty = rendererFeatureSo.FindProperty("m_Name");
            // Debug.Log(nameProperty.stringValue);
            titleLabel.TrackPropertyValue(nameProperty, p =>
            {
                // string newName = p.stringValue;
                titleLabel.text = GetCustomTitle(rendererFeatureSo.targetObject);
            });
            titleLabel.text = GetCustomTitle(rendererFeatureSo.targetObject);

            Button helpButton = element.Q<Button>(name: "helpButton");
            string helpURL = TryGetHelpURL(rendererFeatureSo.targetObject.GetType());
            if (string.IsNullOrEmpty(helpURL))
            {
                helpButton.style.display = DisplayStyle.None;
            }
            else
            {
                helpButton.style.display = DisplayStyle.Flex;
                helpButton.clicked += () => Application.OpenURL(helpURL);
            }

            this.Bind(rendererFeatureSo);
        }

        // private static string FormatName(string namePropertyStringValue, string typeTitle)
        // {
        //     if (string.IsNullOrWhiteSpace(namePropertyStringValue))
        //     {
        //         return typeTitle;
        //     }
        //
        //     return $"{namePropertyStringValue} ({typeTitle})";
        // }

        private string GetCustomTitle(UnityEngine.Object rendererFeatureObjRef)
        {
            string title = null;
            DisallowMultipleRendererFeature isSingleFeature = rendererFeatureObjRef.GetType().GetCustomAttribute<DisallowMultipleRendererFeature>();
            if (isSingleFeature != null)
            {
                title = isSingleFeature.customTitle;
            }

            if (string.IsNullOrEmpty(title))
            {
                title = ObjectNames.GetInspectorTitle(rendererFeatureObjRef);
            }

            return title;
        }

        public override VisualElement contentContainer { get; }

        private string _customViewDataKey;

        public void SetCustomViewData(string key)
        {
            _customViewDataKey = key;
            if (CustomViewDataCache.TryGetValue(key, out bool expand))
            {
                _expanded = expand;
                RefreshExpand();
            }
        }

        private void RefreshExpand()
        {
            contentContainer.style.display = _expanded? DisplayStyle.Flex: DisplayStyle.None;
            _foldoutIcon.style.rotate = _expanded ? new StyleRotate(new Rotate(90)) : new StyleRotate(StyleKeyword.None);
            if (!string.IsNullOrEmpty(_customViewDataKey))
            {
                CustomViewDataCache[_customViewDataKey] = _expanded;
            }
        }

        private static string TryGetHelpURL(Type type)
        {
            HelpURLAttribute attribute = type.GetCustomAttribute<HelpURLAttribute>(false);
            return attribute?.URL;
        }
    }
}

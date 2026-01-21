using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using SaintsField.Editor.Utils;
using SaintsField.ScriptableRenderer.Urp;
using UnityEditor;
using UnityEditor.Rendering.Universal;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.ScriptableRenderer
{
    public class TitleGroupElement : VisualElement
    {
        public override VisualElement contentContainer { get; }

        public TitleGroupElement(string label, string tooltip)
        {
            style.marginBottom = 7;

            hierarchy.Add(new Label(label)
            {
                tooltip = tooltip,
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                },
            });
            hierarchy.Add(contentContainer = new VisualElement
            {
                style =
                {
                    marginLeft = SaintsPropertyDrawer.IndentWidth,
                },
            });
        }

    }

    [CustomEditor(typeof(SaintsUniversalRendererData), true)]
    public class SaintsUniversalRendererDataEditor: UniversalRendererDataEditor
    {
        private UIToolkitUtils.DropdownButtonField _depthAttachmentFormatBtn;
        private PropertyField _mAccurateGbufferNormalsField;
        private PropertyField _mDepthPrimingModeField;
        private HelpBox _depthPrimingMSAAWarningHelpBox;
        private HelpBox _depthPrimingModeInfoHelpBox;
        private HelpBox _invalidStencilOverride;

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            root.Bind(serializedObject);

            SerializedProperty mDefaultStencilState = serializedObject.FindProperty("m_DefaultStencilState");
            SerializedProperty overrideStencil = mDefaultStencilState.FindPropertyRelative("overrideStencilState");

            VisualElement filteringContainer = new TitleGroupElement("Filtering", "Settings that controls and define which layers the renderer draws.");
            root.Add(filteringContainer);

            // bool hasRenderGraphSettings = GraphicsSettings.TryGetRenderPipelineSettings(out RenderGraphSettings renderGraphSettings);

            PropertyField mPrepassLayerMaskField =
                new PropertyField(serializedObject.FindProperty("m_PrepassLayerMask"), "Prepass Layer Mask")
                {
                    tooltip = "Controls which prepass layers this renderer draws. It applies to any prepass."
                };
            filteringContainer.Add(mPrepassLayerMaskField);
#if URP_COMPATIBILITY_MODE // || true
            {
                void UpdatePrepassLayerMaskDisplay()
                {
                    bool display =
                        GraphicsSettings.TryGetRenderPipelineSettings(out RenderGraphSettings renderGraphSettings)
                        && !renderGraphSettings.enableRenderCompatibilityMode;
                    UIToolkitUtils.SetDisplayStyle(mPrepassLayerMaskField, display? DisplayStyle.Flex: DisplayStyle.None);
                }

                mPrepassLayerMaskField.schedule.Execute(UpdatePrepassLayerMaskDisplay).Every(150);
            }
#endif

            filteringContainer.Add(new PropertyField(serializedObject.FindProperty("m_OpaqueLayerMask"), "Opaque Layer Mask") { tooltip = "Controls which opaque layers this renderer draws."});
            filteringContainer.Add(new PropertyField(serializedObject.FindProperty("m_TransparentLayerMask"), "Transparent Layer Mask") { tooltip = "Controls which transparent layers this renderer draws."});

            #region Rendering
            SerializedProperty mDepthPrimingMode = serializedObject.FindProperty("m_DepthPrimingMode");

            TitleGroupElement renderingSection = new TitleGroupElement("Rendering", "Settings related to rendering and lighting.");
            root.Add(renderingSection);
            SerializedProperty mRenderingMode = serializedObject.FindProperty("m_RenderingMode");
            PropertyField mRenderingModePropField = new PropertyField(mRenderingMode, "Rendering Path")
            {
                tooltip = "Select a rendering path.",
            };
            renderingSection.Add(mRenderingModePropField);
            mRenderingModePropField.TrackPropertyValue(mRenderingMode, p => OnRenderingModeChanged(p, mDepthPrimingMode, overrideStencil, mDefaultStencilState));
#if URP_COMPATIBILITY_MODE
            {
                HelpBox deferredPlusIncompatibleWarning = new HelpBox(
                    "Deferred+ is only available with Render Graph. In compatibility mode, Deferred+ falls back to Forward+.",
                    HelpBoxMessageType.Error);
                renderingSection.Add(deferredPlusIncompatibleWarning);
                deferredPlusIncompatibleWarning.TrackPropertyValue(mRenderingMode, UpdateDeferredPlusIncompatibleWarning);
                UpdateDeferredPlusIncompatibleWarning(mRenderingMode);

                void UpdateDeferredPlusIncompatibleWarning(SerializedProperty renderingModeProp)
                {
                    bool error = renderingModeProp.intValue == (int)RenderingMode.DeferredPlus
                                 && GraphicsSettings.GetRenderPipelineSettings<RenderGraphSettings>().enableRenderCompatibilityMode
                                 ;
                    UIToolkitUtils.SetDisplayStyle(deferredPlusIncompatibleWarning, error? DisplayStyle.Flex: DisplayStyle.None);
                }
            }
#endif

            #region m_AccurateGbufferNormals

            SerializedProperty mAccurateGbufferNormals = serializedObject.FindProperty("m_AccurateGbufferNormals");
            _mAccurateGbufferNormalsField = new PropertyField(mAccurateGbufferNormals, "Accurate G-buffer Normals")
            {
                tooltip = "Normals in G-buffer use octahedron encoding/decoding. This improves visual quality but might reduce performance.",
            };
            renderingSection.Add(_mAccurateGbufferNormalsField);

            #endregion

            #region m_DepthPrimingMode
            _mDepthPrimingModeField = new PropertyField(mDepthPrimingMode, "Depth Priming Mode")
            {
                tooltip = "With depth priming enabled, Unity uses the depth buffer generated in the depth prepass to determine if a fragment should be rendered or skipped during the Base Camera opaque pass. Disabled: Unity does not perform depth priming. Auto: If there is a Render Pass that requires a depth prepass, Unity performs the depth prepass and depth priming. Forced: Unity performs the depth prepass and depth priming.",
            };
            renderingSection.Add(_mDepthPrimingModeField);
            _mDepthPrimingModeField.TrackPropertyValue(mDepthPrimingMode, p => OnRenderingModeChanged(mRenderingMode, p, overrideStencil, mDefaultStencilState));

            _depthPrimingMSAAWarningHelpBox = new HelpBox("Depth priming is not supported because MSAA is enabled.", HelpBoxMessageType.Warning);
            renderingSection.Add(_depthPrimingMSAAWarningHelpBox);
            _depthPrimingModeInfoHelpBox = new HelpBox("On Android, iOS, and Apple TV, Unity performs depth priming only in Forced mode.", HelpBoxMessageType.Info);
            renderingSection.Add(_depthPrimingModeInfoHelpBox);

            #region RenderPass
#if URP_COMPATIBILITY_MODE
            {
                TitleGroupElement renderPassSectionLabel = new TitleGroupElement("RenderPass", "This section contains properties related to render passes.");
                root.Add(renderPassSectionLabel);
                SerializedProperty mUseNativeRenderPassProp = serializedObject.FindProperty("m_UseNativeRenderPass");
                PropertyField mUseNativeRenderPassField = new PropertyField(mUseNativeRenderPassProp, "Native RenderPass")
                {
                    tooltip = "Enables URP to use RenderPass API.",
                };
                renderPassSectionLabel.Add(mUseNativeRenderPassField);

                void UpdateUseNativeRenderPassField()
                {
                    bool display = GraphicsSettings.TryGetRenderPipelineSettings(out RenderGraphSettings renderGraphSettings)
                                   && renderGraphSettings != null
                                   // ReSharper disable once MergeIntoPattern
                                   && renderGraphSettings.enableRenderCompatibilityMode;
                    UIToolkitUtils.SetDisplayStyle(mUseNativeRenderPassField, display ? DisplayStyle.None : DisplayStyle.Flex);
                }

                mUseNativeRenderPassField.schedule.Execute(UpdateUseNativeRenderPassField).Every(150);
            }
#endif
            #endregion

            #endregion

            // renderingSection.Add(MakeFallbackElement("m_CopyDepthMode", "Depth Texture Mode", "Controls after which pass URP copies the scene depth. It has a significant impact on mobile devices bandwidth usage. It also allows to force a depth prepass to generate it."));
            renderingSection.Add(new PropertyField(serializedObject.FindProperty("m_CopyDepthMode"), "Depth Texture Mode")
            {
                tooltip = "Controls after which pass URP copies the scene depth. It has a significant impact on mobile devices bandwidth usage. It also allows to force a depth prepass to generate it.",
            });
            // m_DepthAttachmentFormat
            _depthAttachmentFormatBtn = UIToolkitUtils.MakeDropdownButtonUIToolkit("Depth Attachment Format");
            renderingSection.Add(_depthAttachmentFormatBtn);
            _depthAttachmentFormatBtn.labelElement.tooltip = "Which format to use (if it is supported) when creating _CameraDepthAttachment.";
            SerializedProperty depthAttachmentFormatProp = serializedObject.FindProperty("m_DepthAttachmentFormat");
            UpdateDepthAttachmentFormatLabel(depthAttachmentFormatProp);
            _depthAttachmentFormatBtn.TrackPropertyValue(depthAttachmentFormatProp, UpdateDepthAttachmentFormatLabel);
            _depthAttachmentFormatBtn.ButtonElement.clicked += OnDepthAttachmentFormatClick;
            UIToolkitUtils.AddContextualMenuManipulator(_depthAttachmentFormatBtn, depthAttachmentFormatProp, () => {});

            renderingSection.Add(new PropertyField(serializedObject.FindProperty("m_DepthTextureFormat"), "Depth Texture Format")
            {
                tooltip = "Which format to use (if it is supported) when creating _CameraDepthTexture.",
            });
            #endregion


            #region ShadowsSection
            TitleGroupElement shadowsSection = new TitleGroupElement("Shadows", "This section contains properties related to rendering shadows.");
            root.Add(shadowsSection);
            shadowsSection.Add(new PropertyField(serializedObject.FindProperty("m_ShadowTransparentReceive"), "Transparent Receive Shadows")
            {
                tooltip = "When disabled, none of the transparent objects will receive shadows.",
            });
            #endregion

            #region PostProcessing

            TitleGroupElement postProcessingSection = new TitleGroupElement("Post-processing", "This section contains properties related to rendering post-processing.");
            root.Add(postProcessingSection);

            PostProcessDataToggleField toggle = new PostProcessDataToggleField("Enabled", new PostProcessDataToggle())
            {
                bindingPath = serializedObject.FindProperty("postProcessData").propertyPath,
            };
            toggle.AddToClassList(PostProcessDataToggleField.alignedFieldUssClassName);
            postProcessingSection.Add(toggle);

            postProcessingSection.Add(new PropertyField(serializedObject.FindProperty("postProcessData"), "Data")
            {
                tooltip = "The asset containing references to shaders and Textures that the Renderer uses for post-processing.",
            });

            #endregion

            #region OverridesSection

            TitleGroupElement overridesSection = new TitleGroupElement("Overrides",
                "This section contains Render Pipeline properties that this Renderer overrides.");
            root.Add(overridesSection);


            PropertyField mDefaultStencilStateField = new PropertyField(mDefaultStencilState, "Default Stencil State")
            {
                tooltip = "Configure the stencil state for the opaque and transparent render passes.",
            };
            overridesSection.Add(mDefaultStencilStateField);
            mDefaultStencilStateField.RegisterValueChangeCallback(_ => OnRenderingModeChanged(mRenderingMode, mDepthPrimingMode, overrideStencil, mDefaultStencilState));

            #endregion

            _invalidStencilOverride = new HelpBox(
                "Error: When using the deferred rendering path, the Renderer requires the control over the 4 highest bits of the stencil buffer to store Material types. The current combination of the stencil override options prevents the Renderer from controlling the required bits. Try changing one of the options to Replace.",
                HelpBoxMessageType.Error);
            root.Add(_invalidStencilOverride);

            OnRenderingModeChanged(mRenderingMode, mDepthPrimingMode, overrideStencil, mDefaultStencilState);


            #region SaintsEditor

            root.Add(new SaintsUniversalRendererDataEditorCore(this, false).CreateInspectorGUI());

            #endregion

            root.Add(new ScriptableRendererDataCore(this).CreateInspectorGUI());

            return root;
        }

        private class PostProcessDataToggle: BindableElement, INotifyValueChanged<PostProcessData>
        {
            private PostProcessData _cachedValue;

            private readonly Toggle _toggle;

            public PostProcessDataToggle()
            {
                Add(_toggle = new Toggle(null));
                _toggle.RegisterValueChangedCallback(evt =>
                {
                    if (evt.newValue)
                    {
                        Type type = typeof(PostProcessData);
                        MethodInfo method = type.GetMethod(
                            "GetDefaultPostProcessData",
                            BindingFlags.Static | BindingFlags.NonPublic
                        );
                        if (method == null)
                            throw new MissingMethodException(
                                type.FullName,
                                "GetDefaultPostProcessData"
                            );
                        value = (PostProcessData)method.Invoke(null, null);
                    }
                    else
                    {
                        value = null;
                    }
                });
            }

            public void SetValueWithoutNotify(PostProcessData newValue)
            {
                _cachedValue = newValue;
                _toggle.SetValueWithoutNotify(newValue != null);
            }

            public PostProcessData value
            {
                get => _cachedValue;
                set
                {
                    if (_cachedValue == value)
                    {
                        return;
                    }

                    PostProcessData previous = this.value;
                    SetValueWithoutNotify(value);

                    using ChangeEvent<PostProcessData> evt = ChangeEvent<PostProcessData>.GetPooled(previous, value);
                    evt.target = this;
                    SendEvent(evt);
                }
            }
        }

        private class PostProcessDataToggleField : BaseField<PostProcessData>
        {
            private readonly PostProcessDataToggle _processDataToggle;

            public PostProcessDataToggleField(string label, PostProcessDataToggle visualInput) : base(label, visualInput)
            {
                style.flexGrow = 1;
                style.flexShrink = 1;
                _processDataToggle = visualInput;
            }

            public override PostProcessData value
            {
                get => _processDataToggle.value;
                set => _processDataToggle.value = value;
            }

            public override void SetValueWithoutNotify(PostProcessData newValue)
            {
                _processDataToggle.SetValueWithoutNotify(newValue);
            }
        }

        private void OnDepthAttachmentFormatClick()
        {
            SerializedProperty mDepthAttachmentFormatProp = serializedObject.FindProperty("m_DepthAttachmentFormat");
            int mDepthAttachmentFormat = mDepthAttachmentFormatProp.intValue;

            AdvancedDropdownList<DepthFormat> dropdownList = new AdvancedDropdownList<DepthFormat>();
            foreach (DepthFormat depthAttachmentFormatOption in GetDepthAttachmentFormatOptions())
            {
                dropdownList.Add(depthAttachmentFormatOption.ToString(), depthAttachmentFormatOption);
            }

            AdvancedDropdownMetaInfo metaInfo = new AdvancedDropdownMetaInfo
            {
                CurValues = new object[] { (DepthFormat)mDepthAttachmentFormat },
                DropdownListValue = dropdownList,
            };
            (Rect worldBound, float maxHeight) = SaintsAdvancedDropdownUIToolkit.GetProperPos(_depthAttachmentFormatBtn.worldBound);
            SaintsTreeDropdownUIToolkit sa = new SaintsTreeDropdownUIToolkit(
                metaInfo,
                _depthAttachmentFormatBtn.worldBound.width,
                maxHeight,
                false,
                (curItem, _) =>
                {
                    DepthFormat newValue = (DepthFormat)curItem;
                    mDepthAttachmentFormatProp.intValue = (int)newValue;
                    mDepthAttachmentFormatProp.serializedObject.ApplyModifiedProperties();
                    // UpdateDepthAttachmentFormatLabel();
                    return null;
                }
            );
            UnityEditor.PopupWindow.Show(worldBound, sa);
        }

        private void UpdateDepthAttachmentFormatLabel(SerializedProperty mDepthAttachmentFormatProp)
        {
            int mDepthAttachmentFormat = mDepthAttachmentFormatProp.intValue;
            foreach (DepthFormat depthAttachmentFormatOption in GetDepthAttachmentFormatOptions())
            {
                if ((int)depthAttachmentFormatOption == mDepthAttachmentFormat)
                {
                    _depthAttachmentFormatBtn.ButtonLabelElement.text = depthAttachmentFormatOption.ToString();
                    _depthAttachmentFormatBtn.ButtonElement.tooltip = depthAttachmentFormatOption.ToString();
                    return;
                }
            }

            _depthAttachmentFormatBtn.ButtonLabelElement.text = $"<color=red>?</color> {mDepthAttachmentFormat}";
            _depthAttachmentFormatBtn.ButtonElement.tooltip = $"Invalid: {mDepthAttachmentFormat}";
        }

        private MethodInfo _populateCompatibleDepthFormatsMethod;
        private FieldInfo _mDepthFormatStringsField;

        private IEnumerable<DepthFormat> GetDepthAttachmentFormatOptions()
        {
            int renderingMode = serializedObject.FindProperty("m_RenderingMode").intValue;

            if (_populateCompatibleDepthFormatsMethod == null)
            {
                Type type = typeof(UniversalRendererDataEditor);
                _populateCompatibleDepthFormatsMethod = type.GetMethod(
                    "PopulateCompatibleDepthFormats",
                    BindingFlags.Instance | BindingFlags.NonPublic
                );

                if (_populateCompatibleDepthFormatsMethod == null)
                {
                    Debug.LogError("Unity internal changed for PopulateCompatibleDepthFormats, please report to SaintsField.");
                    yield break;
                }
            }

            _populateCompatibleDepthFormatsMethod.Invoke(this, new object[] { renderingMode });

            if (_mDepthFormatStringsField == null)
            {
                Type type = typeof(UniversalRendererDataEditor);
                _mDepthFormatStringsField = type.GetField(
                    "m_DepthFormatStrings",
                    BindingFlags.Instance |
                    BindingFlags.NonPublic
                );

                if (_mDepthFormatStringsField == null)
                {
                    Debug.LogError("Unity internal changed for PopulateCompatibleDepthFormats, please report to SaintsField.");
                    yield break;
                }
            }

            List<string> result = (List<string>)_mDepthFormatStringsField.GetValue(this);
            foreach (string depthFormatName in result)
            {
                if (Enum.TryParse(depthFormatName, out DepthFormat depthFormat))
                {
                    yield return depthFormat;
                }
                else
                {
                    Debug.LogError($"failed to parse depth format `{depthFormatName}`.");
                }
            }
        }

        private void OnRenderingModeChanged(SerializedProperty renderingModeProp, SerializedProperty mDepthPrimingMode, SerializedProperty overrideStencil, SerializedProperty mDefaultStencilState)
        {
            // ReSharper disable once MergeIntoLogicalPattern
            bool mAccurateGbufferNormalsFieldDisplay = renderingModeProp.intValue == (int)RenderingMode.Deferred ||
                                                renderingModeProp.intValue == (int)RenderingMode.DeferredPlus;
            UIToolkitUtils.SetDisplayStyle(_mAccurateGbufferNormalsField, mAccurateGbufferNormalsFieldDisplay? DisplayStyle.Flex: DisplayStyle.None);

            // ReSharper disable once MergeIntoLogicalPattern
            bool mDepthPrimingModeFieldDisplay = (renderingModeProp.intValue == (int)RenderingMode.Forward ||
                                                   renderingModeProp.intValue == (int)RenderingMode.ForwardPlus);
            UIToolkitUtils.SetDisplayStyle(_mDepthPrimingModeField, mDepthPrimingModeFieldDisplay? DisplayStyle.Flex: DisplayStyle.None);

            bool depthPrimingMSAAWarningDisplay = false;
            bool depthPrimingModeInfoDisplay = false;
            if (mDepthPrimingModeFieldDisplay && mDepthPrimingMode.intValue != (int)DepthPrimingMode.Disabled)
            {
                if (GraphicsSettings.currentRenderPipeline != null
                    && GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset asset
                    // ReSharper disable once MergeIntoPattern
                    && asset.msaaSampleCount > 1)
                {
                    depthPrimingMSAAWarningDisplay = true;
                }
                else
                {
                    depthPrimingModeInfoDisplay = true;
                }
            }

            UIToolkitUtils.SetDisplayStyle(_depthPrimingMSAAWarningHelpBox, depthPrimingMSAAWarningDisplay? DisplayStyle.Flex: DisplayStyle.None);
            UIToolkitUtils.SetDisplayStyle(_depthPrimingModeInfoHelpBox, depthPrimingModeInfoDisplay? DisplayStyle.Flex: DisplayStyle.None);

            bool usesDeferredLighting = renderingModeProp.intValue == (int)RenderingMode.Deferred;
            usesDeferredLighting |= renderingModeProp.intValue == (int)RenderingMode.DeferredPlus;

            bool invalidStencilOverrideError = false;
            if (overrideStencil.boolValue && usesDeferredLighting)
            {
                CompareFunction stencilFunction = (CompareFunction)mDefaultStencilState.FindPropertyRelative("stencilCompareFunction").enumValueIndex;
                StencilOp stencilPass = (StencilOp)mDefaultStencilState.FindPropertyRelative("passOperation").enumValueIndex;
                StencilOp stencilFail = (StencilOp)mDefaultStencilState.FindPropertyRelative("failOperation").enumValueIndex;
                StencilOp stencilZFail = (StencilOp)mDefaultStencilState.FindPropertyRelative("zFailOperation").enumValueIndex;
                // ReSharper disable once MergeIntoLogicalPattern
                bool invalidFunction = stencilFunction == CompareFunction.Disabled || stencilFunction == CompareFunction.Never;
                bool invalidOp = stencilPass != StencilOp.Replace && stencilFail != StencilOp.Replace && stencilZFail != StencilOp.Replace;

                if (invalidFunction || invalidOp)
                {
                    invalidStencilOverrideError = true;
                }
            }
            UIToolkitUtils.SetDisplayStyle(_invalidStencilOverride, invalidStencilOverrideError? DisplayStyle.Flex: DisplayStyle.None);
        }

        // public IEnumerable<IReadOnlyList<AbsRenderer>> MakeRenderer(SerializedObject so, SaintsFieldWithInfo fieldWithInfo)
        // {
        //     return SaintsEditor.HelperMakeRenderer(so, fieldWithInfo);
        // }
    }
}

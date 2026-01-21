using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Playa.ScriptableRenderer
{
    public class SaintsUniversalRendererDataEditorCore: SaintsEditorCore
    {
        public SaintsUniversalRendererDataEditorCore(UnityEditor.Editor editor, bool editorShowMonoScript) : base(editor, editorShowMonoScript)
        {
        }

        public override IEnumerable<IReadOnlyList<AbsRenderer>> MakeRenderer(SerializedObject so, SaintsFieldWithInfo fieldWithInfo)
        {
            // Debug.Log(fieldWithInfo.FieldInfo?.Name);
            // ReSharper disable once ConvertSwitchStatementToSwitchExpression
            switch (fieldWithInfo.FieldInfo?.Name)
            {
                // case null:
                case "debugShaders":
                case "probeVolumeResources":
                case "m_RendererFeatures":
                case "m_RendererFeatureMap":
                case "m_UseNativeRenderPass":
                case "xrSystemData":
                case "postProcessData":
                case "m_OpaqueLayerMask":
                case "m_DefaultStencilState":
                case "m_ShadowTransparentReceive":
                case "m_RenderingMode":
                case "m_DepthPrimingMode":
                case "m_CopyDepthMode":
                case "m_DepthAttachmentFormat":
                case "m_DepthTextureFormat":
                case "m_AccurateGbufferNormals":
                case "m_IntermediateTextureMode":
                case "m_AssetVersion":
                case "m_PrepassLayerMask":
                case "m_TransparentLayerMask":
                case "shaders":
                    return Array.Empty<IReadOnlyList<AbsRenderer>>();
                default:
                    // Debug.Log(fieldWithInfo.FieldInfo.Name);
                    return base.MakeRenderer(so, fieldWithInfo);
            }

        }
    }
}

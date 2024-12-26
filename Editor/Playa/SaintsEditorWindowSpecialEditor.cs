using System.Linq;
using SaintsField.Editor.Playa.Renderer;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa
{
    public class SaintsEditorWindowSpecialEditor: SaintsEditor
    {
        public override bool RequiresConstantRepaint() =>
#if SAINTSFIELD_SAINTS_EDITOR_IMGUI_CONSTANT_REPAINT_DISABLE
            false
#else
            true
#endif
        ;

        public override AbsRenderer MakeRenderer(SerializedObject so, SaintsFieldWithInfo fieldWithInfo)
        {
            if(fieldWithInfo.RenderType == SaintsRenderType.SerializedField && fieldWithInfo.FieldInfo.Name == "m_SerializedDataModeController")
            {
                return null;
            }

            if (fieldWithInfo.PlayaAttributes.Any(each => each is SaintsEditorWindow.WindowInlineEditorAttribute))
            {
                // Debug.Log(fieldWithInfo);
                return new WindowInlineEditorRenderer(fieldWithInfo);
            }

            // Debug.Log($"{fieldWithInfo.RenderType}/{fieldWithInfo.FieldInfo?.Name}/{string.Join(",", fieldWithInfo.PlayaAttributes)}");
            return base.MakeRenderer(so, fieldWithInfo);
            // return null;
        }

        public class WindowInlineEditorRenderer : AbsRenderer
        {
            private readonly SaintsFieldWithInfo _fieldWithInfo;
            public WindowInlineEditorRenderer(SaintsFieldWithInfo fieldWithInfo): base(fieldWithInfo)
            {
                _fieldWithInfo = fieldWithInfo;
            }

            public override void OnDestroy()
            {

            }

            private VisualElement _container;
            private UnityEngine.Object _value;

            protected override (VisualElement target, bool needUpdate) CreateTargetUIToolkit()
            {
                _value = GetValue();

                // SaintsEditorWindowSpecialEditor editor = (SaintsEditorWindowSpecialEditor)UnityEditor.Editor.CreateEditor(value);
                // editor.EditorShowMonoScript = false;
                _container = new VisualElement();
                InspectorElement element = new InspectorElement(_value);
                _container.Add(element);
                Debug.Log($"created for {element}");
                return (_container, true);
            }

            private UnityEngine.Object GetValue()
            {
                object v = _fieldWithInfo.FieldInfo != null
                    ? _fieldWithInfo.FieldInfo.GetValue(_fieldWithInfo.Target)
                    : _fieldWithInfo.PropertyInfo.GetValue(_fieldWithInfo.Target);
                return v as Object;
            }

            protected override PreCheckResult OnUpdateUIToolKit()
            {
                var result = base.OnUpdateUIToolKit();
                var newV = GetValue();
                if (!Util.GetIsEqual(_value, newV))
                {
                    _value = newV;
                    _container.Clear();
                    _container.Add(new InspectorElement(_value));
                }
                return result;
            }

            protected override void RenderTargetIMGUI(PreCheckResult preCheckResult)
            {
                throw new System.NotImplementedException();
            }

            protected override float GetFieldHeightIMGUI(float width, PreCheckResult preCheckResult)
            {
                throw new System.NotImplementedException();
            }

            protected override void RenderPositionTarget(Rect position, PreCheckResult preCheckResult)
            {
                throw new System.NotImplementedException();
            }
        }
    }


}

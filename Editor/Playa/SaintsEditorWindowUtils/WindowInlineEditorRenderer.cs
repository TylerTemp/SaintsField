using System;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using UnityEditor;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Playa.SaintsEditorWindowUtils
{
    public partial class WindowInlineEditorRenderer : AbsRenderer
    {
        private readonly SaintsFieldWithInfo _fieldWithInfo;
        private readonly Type _editorType;

        public WindowInlineEditorRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo, Type editorType): base(serializedObject, fieldWithInfo)
        {
            _fieldWithInfo = fieldWithInfo;
            _editorType = editorType;
        }

        public override void OnDestroy()
        {

        }

        public override void OnSearchField(string searchString)
        {
        }

        //
        // public override VisualElement CreateVisualElement()
        // {
        //     VisualElement root = new VisualElement();
        //     // _rootElement = CreateRootElement();
        //     VisualElement result = base.CreateVisualElement();
        //     if (result != null)
        //     {
        //         root.Add(result);
        //     }
        //
        //     root.TrackPropertyValue(FieldWithInfo.SerializedProperty, changedProp =>
        //     {
        //         Debug.Log(changedProp.objectReferenceValue);
        //     });
        //
        //     return root;
        // }

        private Object GetValue()
        {
            object v = _fieldWithInfo.FieldInfo != null
                ? _fieldWithInfo.FieldInfo.GetValue(_fieldWithInfo.Targets[0])
                : _fieldWithInfo.PropertyInfo.GetValue(_fieldWithInfo.Targets[0]);
            return v as Object;
        }
    }
}

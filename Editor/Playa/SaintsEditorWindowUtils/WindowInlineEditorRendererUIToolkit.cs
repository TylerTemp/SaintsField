#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using SaintsField.Editor.Utils;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.SaintsEditorWindowUtils
{
    public partial class WindowInlineEditorRenderer
    {
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
    }
}
#endif

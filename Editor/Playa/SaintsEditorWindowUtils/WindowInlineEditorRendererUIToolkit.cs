#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using SaintsField.Editor.Utils;
using SaintsField.Utils;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.SaintsEditorWindowUtils
{
    public partial class WindowInlineEditorRenderer
    {
        private VisualElement _container;
        private Object _value;

        protected override (VisualElement target, bool needUpdate) CreateTargetUIToolkit(VisualElement container)
        {
            _value = GetValue();

            // SaintsEditorWindowSpecialEditor editor = (SaintsEditorWindowSpecialEditor)UnityEditor.Editor.CreateEditor(value);
            // editor.EditorShowMonoScript = false;
            _container = new VisualElement();
            if(!RuntimeUtil.IsNull(_value))
            {
                ReCreateEditor();
            }
            // Debug.Log($"created for {element}");
            return (_container, true);
        }

        private void ReCreateEditor()
        {
            _container.Clear();
            InspectorElement inspectorElement;
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (_editorType == null)
            {
                inspectorElement = new InspectorElement(_value);
            }
            else
            {
                // Debug.Log($"create editor for {_value} with {_editorType}");
                inspectorElement = new InspectorElement(UnityEditor.Editor.CreateEditor(_value, _editorType));
            }
            _container.Add(inspectorElement);
        }


        // protected override PreCheckResult OnUpdateUIToolKit()
        // {
        //     PreCheckResult result = HelperOnUpdateUIToolKitRawBase();
        //     Object newV = GetValue();
        //     if (!ReferenceEquals(_value, newV))
        //     {
        //         Debug.Log($"Recreate {_value} -> {newV}");
        //         _value = newV;
        //         ReCreateEditor();
        //     }
        //     return result;
        // }
    }
}
#endif

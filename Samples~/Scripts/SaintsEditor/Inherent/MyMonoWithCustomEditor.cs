#if UNITY_EDITOR
using System.Collections.Generic;
using SaintsField.Editor.Playa;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Samples.Scripts.SaintsEditor.Inherent
{
    [CustomEditor(typeof(MyMonoWithCustom), true)]
    public class MyMonoWithCustomEditor : SaintsField.Editor.SaintsEditor
    {
        public override IEnumerable<IReadOnlyList<AbsRenderer>> MakeRenderer(SerializedObject so, SaintsFieldWithInfo fieldWithInfo)
        {
            if (fieldWithInfo.FieldInfo != null && fieldWithInfo.FieldInfo.Name == "toggle")
            {
                yield break;  // returns nothing to show nothing
            }

            if (fieldWithInfo.FieldInfo != null && fieldWithInfo.FieldInfo.Name == "input")
            {
                yield return new[]{new ToggleInputRenderer(so, fieldWithInfo)};  // custom rendering
                yield break;
            }

            // default rendering
            foreach (IReadOnlyList<AbsRenderer> defaultRenderer in base.MakeRenderer(so, fieldWithInfo))
            {
                yield return defaultRenderer;
            }
        }
    }

    public class ToggleInputRenderer: AbsRenderer
    {
        private readonly SerializedObject _serializedObject;
        public ToggleInputRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo) : base(serializedObject, fieldWithInfo)
        {
            _serializedObject = serializedObject;
        }

        protected override bool AllowGuiColor => true;

        public override void OnDestroy()
        {
        }

        public override void OnSearchField(string searchString)
        {
        }

        protected override void RenderTargetIMGUI(float width, PreCheckResult preCheckResult)
        {
            // IMGUI renderer, if you want IMGUI you can write here
        }

        protected override float GetFieldHeightIMGUI(float width, PreCheckResult preCheckResult)
        {
            // IMGUI renderer, if you want IMGUI you can write here
            return 0;
        }

        protected override void RenderPositionTargetIMGUI(Rect position, PreCheckResult preCheckResult)
        {
            // IMGUI renderer, if you want IMGUI you can write here
        }

        protected override (VisualElement target, bool needUpdate) CreateTargetUIToolkit(VisualElement container)
        {
            VisualElement root = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                },
            };

            Toggle toggle = new Toggle("Input")
            {
                bindingPath = _serializedObject.FindProperty("toggle").propertyPath,
            };
            toggle.AddToClassList(Toggle.alignedFieldUssClassName);
            root.Add(toggle);

            TextField input = new TextField
            {
                bindingPath = _serializedObject.FindProperty("input").propertyPath,
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                },
            };
            root.Add(input);

            return (root, true);
        }

        protected override PreCheckResult OnUpdateUIToolKit(VisualElement root)
        {
            // If needUpdate=true, this function is called every 0.1s. You can do some ticking update here.
            TextField textField = root.Q<TextField>();
            textField.SetEnabled(_serializedObject.FindProperty("toggle").boolValue);

            // return the required value
            return base.OnUpdateUIToolKit(root);
        }
    }
}
#endif

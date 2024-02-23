using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
#if UNITY_2021_3_OR_NEWER
using System;
using UnityEngine.UIElements;
#endif
using Component = UnityEngine.Component;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(MaterialToggleAttribute))]
    public class MaterialToggleAttributeDrawer: SaintsPropertyDrawer
    {
        private const string SelectedStr = "●";
        private const string NonSelectedStr = "○";

        private static (string error, Renderer renderer) GetRenderer(SerializedProperty property, ISaintsAttribute saintsAttribute, object target)
        {
            if(!(target is Object targetObject))
            {
                return ("target is not UnityEngine.Object", null);
            }

            MaterialToggleAttribute toggleAttribute = (MaterialToggleAttribute)saintsAttribute;
            string rendererCompName = toggleAttribute.CompName;

            SerializedObject targetSer = new SerializedObject(targetObject);

            Renderer renderer;

            if (rendererCompName == null)
            {
                renderer = ((Component)targetObject).GetComponent<Renderer>();
            }
            else
            {
                renderer =
                    (Renderer)(targetSer.FindProperty(rendererCompName) ?? SerializedUtils.FindPropertyByAutoPropertyName(targetSer, rendererCompName))?.objectReferenceValue;
            }

            return renderer == null
                ? ($"target {rendererCompName ?? "Renderer"} not found", null)
                : ("", renderer);
        }

        #region IMGUI

        private string _error = "";

        private Renderer _renderer;
        private Material _material;

        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, object parent)
        {
            MaterialToggleAttribute toggleAttribute = (MaterialToggleAttribute)saintsAttribute;
            string rendererCompName = toggleAttribute.CompName;

            (string error, Renderer renderer) = GetRenderer(property, saintsAttribute, parent);
            _renderer = renderer;
            _error = error;

            if(_error != "")
            {
                return 0;
            }

            GUIStyle style = new GUIStyle("Button");

            float width = Mathf.Max(style.CalcSize(new GUIContent(SelectedStr)).x, style.CalcSize(new GUIContent(NonSelectedStr)).x);

            return width;
        }

        protected override bool DrawPostFieldImGui(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, bool valueChanged, FieldInfo info, object parent)
        {
            (string error, Renderer renderer) = GetRenderer(property, saintsAttribute, (Object)parent);
            _error = error;
            _renderer = renderer;
            if (error != "")
            {
                return false;
            }

            MaterialToggleAttribute toggleAttribute = (MaterialToggleAttribute)saintsAttribute;

            Material usingMat = _renderer.sharedMaterials[toggleAttribute.Index];
            Material thisMat = (Material) property.objectReferenceValue;

            bool isToggled = ReferenceEquals(usingMat, thisMat);

            GUIStyle style = new GUIStyle("Button");

            using (new EditorGUI.DisabledScope(isToggled || thisMat == null))
            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                bool nowToggled = GUI.Toggle(position, isToggled, isToggled? "●": "○", style);
                // ReSharper disable once InvertIf
                if (nowToggled && changed.changed)
                {
                    Undo.RecordObject(_renderer, "MaterialToggle");
                    Material[] sharedMats = _renderer.sharedMaterials.ToArray();
                    sharedMats[toggleAttribute.Index] = thisMat;
                    _renderer.sharedMaterials = sharedMats;
                }
            }

            return true;
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            FieldInfo info,
            object parent) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent) => _error == "" ? 0 : ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent) => _error == "" ? position : ImGuiHelpBox.Draw(position, _error, MessageType.Error);
        #endregion

#if UNITY_2021_3_OR_NEWER

        #region UIToolkit

        private static string NameHelpBox(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__MaterialToggle_HelpBox";
        private static string NameButton(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__MaterialToggle_Button";
        // private static string NameButtonLabel(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__MaterialToggle_ButtonLabel";

        protected override VisualElement CreatePostFieldUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, object parent)
        {
            MaterialToggleAttribute toggleAttribute = (MaterialToggleAttribute)saintsAttribute;

            RadioButton button = new RadioButton
            {
                name = NameButton(property, index),
                style =
                {
                    height = EditorGUIUtility.singleLineHeight,
                    paddingLeft = 1,
                    paddingRight = 1,
                },
            };

            button.RegisterValueChangedCallback(changed =>
            {
                if (!changed.newValue)
                {
                    return;
                }

                (string error, Renderer renderer) = GetRenderer(property, saintsAttribute, (Object)parent);

                HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property, index));
                if (helpBox.text != error)
                {
                    helpBox.style.display = error == "" ? DisplayStyle.None : DisplayStyle.Flex;
                    helpBox.text = error;
                }

                // ReSharper disable once InvertIf
                if (error == "")
                {
                    Material[] sharedMats = renderer.sharedMaterials.ToArray();
                    sharedMats[toggleAttribute.Index] = (Material)property.objectReferenceValue;

                    Undo.RecordObject(renderer, $"MaterialToggle {property.propertyPath} {index}");
                    renderer.sharedMaterials = sharedMats;
                }

            });
            return button;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
        {
            return new HelpBox("", HelpBoxMessageType.Error)
            {
                name = NameHelpBox(property, index),
                style =
                {
                    display = DisplayStyle.None,
                },
            };
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChangedCallback, object parent)
        {
            UpdateToggleDisplay(property, index, saintsAttribute, container, parent);
        }

        protected override void OnValueChanged(SerializedProperty property, ISaintsAttribute saintsAttribute, int index, VisualElement container,
            object parent, object newValue)
        {
            UpdateToggleDisplay(property, index, saintsAttribute, container, parent);
        }

        private static void UpdateToggleDisplay(SerializedProperty property, int index,
            ISaintsAttribute saintsAttribute, VisualElement container, object parent)
        {
            (string error, Renderer renderer) = GetRenderer(property, saintsAttribute, (Object)parent);

            HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property, index));
            if(helpBox.text != error)
            {
                helpBox.style.display = error == "" ? DisplayStyle.None : DisplayStyle.Flex;
                helpBox.text = error;
            }

            if (error != "")
            {
                return;
            }

            MaterialToggleAttribute toggleAttribute = (MaterialToggleAttribute)saintsAttribute;

            Material thisMat = (Material) property.objectReferenceValue;
            Material usingMat = renderer.sharedMaterials[toggleAttribute.Index];

            bool isToggled = thisMat == usingMat;
            RadioButton radioButton = container.Q<RadioButton>(NameButton(property, index));
            if (radioButton.value != isToggled)
            {
                radioButton.SetValueWithoutNotify(isToggled);
            }
        }

        #endregion

#endif
    }
}

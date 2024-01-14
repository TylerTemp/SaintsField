using System;
using System.ComponentModel;
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Component = UnityEngine.Component;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(MaterialToggleAttribute))]
    public class MaterialToggleAttributeDrawer: SaintsPropertyDrawer
    {
        private const string SelectedStr = "●";
        private const string NonSelectedStr = "○";

        private static (string error, Renderer renderer) GetRenderer(SerializedProperty property, ISaintsAttribute saintsAttribute, Object targetObject)
        {
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

        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            MaterialToggleAttribute toggleAttribute = (MaterialToggleAttribute)saintsAttribute;
            string rendererCompName = toggleAttribute.CompName;

            Object targetObject = (Object) GetParentTarget(property);
            SerializedObject targetSer = new SerializedObject(targetObject);

            if (rendererCompName == null)
            {
                _renderer = ((Component)targetObject).GetComponent<Renderer>();
            }
            else
            {
                _renderer =
                    (Renderer)(targetSer.FindProperty(rendererCompName) ??
                               targetSer.FindProperty($"<{rendererCompName}>k__BackingField"))?.objectReferenceValue;
            }

            if(_renderer == null)
            {
                _error = $"target {rendererCompName ?? "Renderer"} not found";
                return 0;
            }

            _error = "";

            GUIStyle style = new GUIStyle("Button");

            float width = Mathf.Max(style.CalcSize(new GUIContent(SelectedStr)).x, style.CalcSize(new GUIContent(NonSelectedStr)).x);

            return width;
        }

        protected override bool DrawPostFieldImGui(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, bool valueChanged)
        {
            (string error, Renderer renderer) = GetRenderer(property, saintsAttribute, (Object)GetParentTarget(property));
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

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute) => _error == "" ? 0 : ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => _error == "" ? position : ImGuiHelpBox.Draw(position, _error, MessageType.Error);
        #endregion

        #region UIToolkit

        private static string NameLabelError(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__MaterialToggle_LabelError";
        private static string NameButton(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__MaterialToggle_Button";
        private static string NameButtonLabel(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__MaterialToggle_ButtonLabel";

        protected override VisualElement CreatePostFieldUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, object parent,
            Action<object> onChange)
        {
            MaterialToggleAttribute toggleAttribute = (MaterialToggleAttribute)saintsAttribute;

            Button button = new Button(() =>
            {
                (string error, Renderer renderer) = GetRenderer(property, saintsAttribute, (Object)parent);

                HelpBox helpBox = container.Q<HelpBox>(NameLabelError(property, index));
                helpBox.style.display = error == ""? DisplayStyle.None: DisplayStyle.Flex;
                helpBox.text = error;

                if (error == "")
                {
                    Undo.RecordObject(renderer, "MaterialToggle");
                    Material[] sharedMats = renderer.sharedMaterials.ToArray();
                    sharedMats[toggleAttribute.Index] = (Material) property.objectReferenceValue;

                    renderer.sharedMaterials = sharedMats;

                    container.Q<Label>(NameButtonLabel(property, index)).text = SelectedStr;
                    // Debug.Log(SelectedStr);
                    // onChange?.Invoke(property.colorValue);
                }
            })
            {
                name = NameButton(property, index),
                style =
                {
                    height = EditorGUIUtility.singleLineHeight,
                },
            };

            VisualElement labelContainer = new Label(NonSelectedStr)
            {
                name = NameButtonLabel(property, index),
                userData = null,
            };

            button.Add(labelContainer);
            // button.AddToClassList();
            return button;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, object parent)
        {
            HelpBox helpBox = new HelpBox("", HelpBoxMessageType.Error)
            {
                name = NameLabelError(property, index),
                style =
                {
                    display = DisplayStyle.None,
                },
            };

            return helpBox;
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container, object parent)
        {
            (string error, Renderer renderer) = GetRenderer(property, saintsAttribute, (Object)parent);

            HelpBox helpBox = container.Q<HelpBox>(NameLabelError(property, index));
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
            Label label = container.Q<Label>(NameButtonLabel(property, index));
            string labelValue = isToggled ? SelectedStr : NonSelectedStr;
            if (label.text != labelValue)
            {
                label.text = labelValue;
            }
        }

        #endregion
    }
}

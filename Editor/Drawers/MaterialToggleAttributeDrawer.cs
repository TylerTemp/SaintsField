using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using System;
#if UNITY_2021_3_OR_NEWER
using UnityEngine.UIElements;
#endif
using Component = UnityEngine.Component;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(MaterialToggleAttribute))]
    public class MaterialToggleAttributeDrawer: SaintsPropertyDrawer
    {
        private const string SelectedStr = "●";
        private const string NonSelectedStr = "○";

        private static (string error, Renderer renderer) GetRenderer(SerializedProperty property, ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            MaterialToggleAttribute toggleAttribute = (MaterialToggleAttribute)saintsAttribute;
            string compName = toggleAttribute.CompName;

            Renderer resultRenderer;

            if(compName != null)
            {
                SerializedProperty targetProperty =
                    property.serializedObject.FindProperty(compName) ??
                    SerializedUtils.FindPropertyByAutoPropertyName(property.serializedObject, compName);

                // ReSharper disable once MergeIntoPattern
                if (targetProperty?.objectReferenceValue is Renderer propRenderer)
                {
                    resultRenderer = propRenderer;
                }
                else
                {
                    (string error, Renderer result) =
                        Util.GetOf<Renderer>(compName, null, property, info, parent);
                    if (error != "")
                    {
                        return (error, result);
                    }

                    resultRenderer = result;
                }
            }

            else
            {
                Component thisComponent;
                try
                {
                    thisComponent = (Component)property.serializedObject.targetObject;
                }
                catch (InvalidCastException e)
                {
                    Debug.LogException(e);
                    return ($"target {property.serializedObject.targetObject} is not a Component", null);
                }

                resultRenderer = thisComponent.GetComponent<Renderer>();
                if (resultRenderer == null)
                {
                    return ($"target {thisComponent} has no Renderer", null);
                }
            }

            return resultRenderer == null
                ? ("No renderer found.", null)
                : ("", resultRenderer);
        }

        #region IMGUI

        private string _error = "";

        private Renderer _renderer;
        private Material _material;

        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, OnGUIPayload onGuiPayload, FieldInfo info, object parent)
        {
            (string error, Renderer renderer) = GetRenderer(property, saintsAttribute, info, parent);
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
            ISaintsAttribute saintsAttribute, int index, OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            (string error, Renderer renderer) = GetRenderer(property, saintsAttribute, info, parent);
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
            int index,
            FieldInfo info,
            object parent) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent) => _error == "" ? 0 : ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent) => _error == "" ? position : ImGuiHelpBox.Draw(position, _error, MessageType.Error);
        #endregion

#if UNITY_2021_3_OR_NEWER

        #region UIToolkit

        private static string NameHelpBox(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__MaterialToggle_HelpBox";
        private static string NameButton(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__MaterialToggle_Button";
        // private static string NameButtonLabel(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__MaterialToggle_ButtonLabel";

        protected override VisualElement CreatePostFieldUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
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

                (string error, Renderer renderer) = GetRenderer(property, saintsAttribute, info, parent);

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

            button.AddToClassList(ClassAllowDisable);

            return button;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
        {
            HelpBox helpBox = new HelpBox("", HelpBoxMessageType.Error)
            {
                name = NameHelpBox(property, index),
                style =
                {
                    display = DisplayStyle.None,
                },
            };

            helpBox.AddToClassList(ClassAllowDisable);
            return helpBox;
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info)
        {
            object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
            UpdateToggleDisplay(property, index, saintsAttribute, container, info, parent);
        }

        protected override void OnValueChanged(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container,
            FieldInfo info,
            object parent, Action<object> onValueChangedCallback, object newValue)
        {
            UpdateToggleDisplay(property, index, saintsAttribute, container, info, parent);
        }

        private static void UpdateToggleDisplay(SerializedProperty property, int index,
            ISaintsAttribute saintsAttribute, VisualElement container, FieldInfo info, object parent)
        {
            (string error, Renderer renderer) = GetRenderer(property, saintsAttribute, info, parent);

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

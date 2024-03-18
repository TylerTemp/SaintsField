using System;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
#if UNITY_2021_3_OR_NEWER
using UnityEngine.UIElements;
#endif
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(ColorToggleAttribute))]
    public class ColorToggleAttributeDrawer: SaintsPropertyDrawer
    {
        #region IMGUI

        private string _error = "";

        private enum FieldType
        {
            NotFoundOrValid,
            Image,
            SpriteRenderer,
            Button,
            Renderer,
        }

        private struct Container
        {
            public string Error;
            public FieldType FieldType;
            public Image Image;
            public SpriteRenderer SpriteRenderer;
            public Button Button;
            public Renderer Renderer;
        }

        private Container _container;

        // private SerializedProperty _containerProperty;
        // private bool _isUiImage;
        // private Image _image;
        // private SpriteRenderer _spriteRenderer;

        private const string SelectedStr = "●";
        private const string NonSelectedStr = "○";

        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            _container = GetContainer(saintsAttribute, parent);

            if (_container.Error != "")
            {
                _error = _container.Error;
                return 0;
            }

            GUIStyle style = new GUIStyle("Button");

            float width = Mathf.Max(style.CalcSize(new GUIContent(SelectedStr)).x, style.CalcSize(new GUIContent(NonSelectedStr)).x);

            return width;
        }

        private static Container GetContainer(ISaintsAttribute saintsAttribute, object parent)
        {
            ColorToggleAttribute toggleAttribute = (ColorToggleAttribute)saintsAttribute;
            string imageCompName = toggleAttribute.CompName;

            Object targetObject = (Object) parent;
            SerializedObject targetSer = new SerializedObject(targetObject);

            if(imageCompName != null)
            {
                SerializedProperty targetProperty =
                    targetSer.FindProperty(imageCompName) ??
                    SerializedUtils.FindPropertyByAutoPropertyName(targetSer, imageCompName);

                if (targetProperty != null)
                {
                    return SignObject(targetProperty.objectReferenceValue);
                }

                Type targetType = targetObject.GetType();

                (ReflectUtils.GetPropType getPropType, object fieldOrMethodInfo) propInfo =
                    ReflectUtils.GetProp(targetType, imageCompName);

                // ReSharper disable once ConvertIfStatementToSwitchStatement
                if (propInfo.Item1 == ReflectUtils.GetPropType.NotFound)
                {
                    return new Container
                    {
                        Error = $"target {imageCompName} not found",
                    };
                }

                if (propInfo.Item1 == ReflectUtils.GetPropType.Field)
                {
                    return SignObject(((FieldInfo)propInfo.Item2).GetValue(targetObject));
                }
                if (propInfo.Item1 == ReflectUtils.GetPropType.Property)
                {
                    return SignObject(((PropertyInfo)propInfo.Item2).GetValue(targetObject));
                }
                if (propInfo.Item1 == ReflectUtils.GetPropType.Method)
                {
                    return SignObject(((MethodInfo)propInfo.Item2).Invoke(targetObject, null));
                }

                throw new Exception("Should not reach here");
            }

            Component thisComponent;
            try
            {
                thisComponent = (Component)targetObject;
            }
            catch (InvalidCastException e)
            {
                Debug.LogException(e);
                return new Container
                {
                    Error = $"target {targetObject} is not a Component",
                };
            }

            Image image = thisComponent.GetComponent<Image>();
            if (image != null)
            {
                return SignObject(image);
            }

            Button button = thisComponent.GetComponent<Button>();
            if (button)
            {
                return SignObject(button);
            }

            SpriteRenderer spriteRenderer = thisComponent.GetComponent<SpriteRenderer>();
            if (spriteRenderer)
            {
                return SignObject(spriteRenderer);
            }

            Renderer renderer = thisComponent.GetComponent<Renderer>();
            if (renderer)
            {
                return SignObject(renderer);
            }

            return new Container
            {
                Error = $"target {targetObject} has no Image, SpriteRenderer, Button or Renderer",
            };
        }

        private static Container SignObject(object foundObj)
        {
            switch (foundObj)
            {
                case Image image:
                    return new Container
                    {
                        FieldType = FieldType.Image,
                        Image = image,
                        Error = "",
                    };
                case SpriteRenderer spriteRenderer:
                    return new Container
                    {
                        FieldType = FieldType.SpriteRenderer,
                        SpriteRenderer = spriteRenderer,
                        Error = "",
                    };
                case Button button:
                    return new Container
                    {
                        FieldType = FieldType.Button,
                        Button = button,
                        Error = "",
                    };
                case Renderer renderer:
                    return new Container
                    {
                        FieldType = FieldType.Renderer,
                        Renderer = renderer,
                        Error = "",
                    };
                default:
                    string error = $"Not supported type: {(foundObj == null ? "null" : foundObj.GetType().ToString())}";
                    return new Container
                    {
                        FieldType = FieldType.NotFoundOrValid,
                        Error = error,
                    };
                    // break;
            }
        }

        protected override bool DrawPostFieldImGui(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            if (_container.FieldType == FieldType.NotFoundOrValid || _error != "")
            {
                return false;
            }

            ColorToggleAttribute toggleAttribute = (ColorToggleAttribute)saintsAttribute;

            Color thisColor = property.colorValue;
            Color usingColor = GetColor(_container, toggleAttribute.Index);

            bool isToggled = thisColor == usingColor;

            GUIStyle style = new GUIStyle("Button");

            using (new EditorGUI.DisabledScope(isToggled))
            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                bool nowToggled = GUI.Toggle(position, isToggled, isToggled? "●": "○", style);
                // ReSharper disable once InvertIf
                if (nowToggled && changed.changed)
                {
                    // Debug.Log("Changed!");
                    SetColor(_container, thisColor, toggleAttribute.Index);
                    // SerializedObject containerSer = _isUiImage
                    //     ? new SerializedObject(_image)
                    //     : new SerializedObject(_spriteRenderer);
                    // containerSer.FindProperty("m_Sprite").objectReferenceValue = thisSprite;
                    // containerSer.ApplyModifiedProperties();
                }
            }

            return true;
        }

        private static void SetColor(Container container, Color thisColor, int index)
        {
            SerializedObject serializedObject;

            switch (container.FieldType)
            {
                case FieldType.Image:
                    serializedObject = new SerializedObject(container.Image);
                    serializedObject.FindProperty("m_Color").colorValue = thisColor;
                    serializedObject.ApplyModifiedProperties();
                    break;
                case FieldType.SpriteRenderer:
                    serializedObject = new SerializedObject(container.SpriteRenderer);
                    serializedObject.FindProperty("m_Color").colorValue = thisColor;
                    serializedObject.ApplyModifiedProperties();
                    break;
                case FieldType.Button:
                    // Undo.RecordObject(container.Button.targetGraphic, "ColorToggle");
                    // container.Button.targetGraphic.color = thisColor;
                    serializedObject = new SerializedObject(container.Button.targetGraphic);
                    serializedObject.FindProperty("m_Color").colorValue = thisColor;
                    serializedObject.ApplyModifiedProperties();
                    break;
                case FieldType.Renderer:
                    // Undo.RecordObject(container.Renderer, "ColorToggle");
                    Material[] sharedMats = container.Renderer.sharedMaterials.ToArray();
                    Undo.RecordObject(sharedMats[index], "ColorToggle");
                    sharedMats[index].color = thisColor;
                    break;
                case FieldType.NotFoundOrValid:
                default:
                    throw new ArgumentOutOfRangeException(nameof(container.FieldType), container.FieldType, null);
            }
        }

        private static Color GetColor(Container container, int index)
        {
            // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (container.FieldType)
            {
                case FieldType.Image:
                    return container.Image.color;
                case FieldType.SpriteRenderer:
                    return container.SpriteRenderer.color;
                case FieldType.Button:
                    return container.Button.targetGraphic.color;
                case FieldType.Renderer:
                    return container.Renderer.sharedMaterials[index].color;
                default:
                    throw new ArgumentOutOfRangeException(nameof(container.FieldType), container.FieldType, null);
            }
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

        private static string NameHelpBox(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__ColorToggle_HelpBox";
        private static string NameButton(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__ColorToggle_Button";
        private static string ClassButtonLabel(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__ColorToggle_ButtonLabel";

        protected override VisualElement CreatePostFieldUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
        {
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

            button.RegisterValueChangedCallback(evt =>
            {
                if (!evt.newValue)
                {
                    return;
                }

                Container dataContainer = GetContainer(saintsAttribute, parent);
                string error = dataContainer.Error;
                HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property, index));
                if(helpBox.text != error)
                {
                    helpBox.style.display = error == "" ? DisplayStyle.None : DisplayStyle.Flex;
                    helpBox.text = error;
                }

                if (error == "")
                {
                    SetColor(dataContainer, property.colorValue, ((ColorToggleAttribute)saintsAttribute).Index);
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
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            UpdateToggleDisplay(property, saintsAttribute, index, container, parent);
        }

        protected override void OnValueChanged(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container,
            FieldInfo info,
            object parent, object newValue)
        {
            UpdateToggleDisplay(property, saintsAttribute, index, container, parent);
        }

        private static void UpdateToggleDisplay(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, object parent)
        {
            Container dataContainer = GetContainer(saintsAttribute, parent);
            string error = dataContainer.Error;
            HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property, index));
            if (error != helpBox.text)
            {
                helpBox.style.display = error == "" ? DisplayStyle.None : DisplayStyle.Flex;
                helpBox.text = error;
            }

            if (error != "")
            {
                return;
            }

            ColorToggleAttribute toggleAttribute = (ColorToggleAttribute)saintsAttribute;

            Color thisColor = property.colorValue;
            Color usingColor = GetColor(dataContainer, toggleAttribute.Index);

            bool isToggled = thisColor == usingColor;
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

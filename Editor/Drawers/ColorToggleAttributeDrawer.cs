using System;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_2021_3_OR_NEWER
using UnityEngine.UIElements;
#endif
using Button = UnityEngine.UI.Button;

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
            Graphic,
            SpriteRenderer,
            Button,
            Renderer,
        }

        private struct Container
        {
            // ReSharper disable InconsistentNaming
            public string Error;
            public FieldType FieldType;
            public Graphic Graphic;
            public SpriteRenderer SpriteRenderer;
            public Button Button;
            public Renderer Renderer;
            // ReSharper enable InconsistentNaming
        }

        private Container _container;

        private const string SelectedStr = "●";
        private const string NonSelectedStr = "○";

        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, OnGUIPayload onGuiPayload, FieldInfo info, object parent)
        {
            _container = GetContainer(property, saintsAttribute, info, parent);

            if (_container.Error != "")
            {
                _error = _container.Error;
                return 0;
            }

            GUIStyle style = new GUIStyle("Button");

            float width = Mathf.Max(style.CalcSize(new GUIContent(SelectedStr)).x, style.CalcSize(new GUIContent(NonSelectedStr)).x);

            return width;
        }

        private static Container GetContainer(SerializedProperty property, ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            ColorToggleAttribute toggleAttribute = (ColorToggleAttribute)saintsAttribute;
            string compName = toggleAttribute.CompName;

            if(compName != null)
            {
                SerializedProperty targetProperty =
                    property.serializedObject.FindProperty(compName) ??
                    SerializedUtils.FindPropertyByAutoPropertyName(property.serializedObject, compName);

                if (targetProperty != null)
                {
                    if(targetProperty.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        return SignObject(targetProperty.objectReferenceValue);
                    }
                    if(targetProperty.propertyType == SerializedPropertyType.Generic)
                    {
                        (string _, IWrapProp getResult) = Util.GetOf<IWrapProp>(compName, null, property, info, parent);
                        // Debug.Log(getResult);
                        if (getResult != null)
                        {
                            object actualValue = Util.GetWrapValue(getResult);
                            // Debug.Log(actualValue);
                            return SignObject(actualValue);
                        }
                    }
                    // return SignObject(targetProperty.objectReferenceValue);
                }

                (string error, object foundObj) =
                    Util.GetOf<object>(compName, null, property, info, parent);

                if (error != "")
                {
                    return new Container
                    {
                        Error = $"target {compName} not found",
                    };
                }

                return SignObject(foundObj);
            }

            Component thisComponent;
            try
            {
                thisComponent = (Component)property.serializedObject.targetObject;
            }
            catch (InvalidCastException e)
            {
                Debug.LogException(e);
                return new Container
                {
                    Error = $"target {property.serializedObject.targetObject} is not a Component",
                };
            }

            Graphic graphic = thisComponent.GetComponent<Graphic>();
            if (graphic != null)
            {
                return SignObject(graphic);
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
                Error = $"target {thisComponent} has no Image, SpriteRenderer, Button or Renderer",
            };
        }

        private static Container SignObject(object foundObj)
        {
            switch (foundObj)
            {
                case Graphic graphic:
                    return new Container
                    {
                        FieldType = FieldType.Graphic,
                        Graphic = graphic,
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
                case GameObject _:
                case Component _:
                {
                    UnityEngine.Object obj = (UnityEngine.Object)foundObj;
                    UnityEngine.Object actualObj = Util.GetTypeFromObj(obj, typeof(Graphic))
                                                   ?? Util.GetTypeFromObj(obj, typeof(SpriteRenderer))
                                                   ?? Util.GetTypeFromObj(obj, typeof(Button))
                                                   ?? Util.GetTypeFromObj(obj, typeof(Renderer));
                    // Debug.Log($"obj={obj} actual={actualObj}, renderer={((Component)foundObj).GetComponent<Renderer>()}");
                    // ReSharper disable once TailRecursiveCall
                    return SignObject(
                        actualObj
                    );
                }
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
                }
            }

            return true;
        }

        private static void SetColor(Container container, Color thisColor, int index)
        {
            SerializedObject serializedObject;

            switch (container.FieldType)
            {
                case FieldType.Graphic:
                    serializedObject = new SerializedObject(container.Graphic);
                    SerializedProperty fontColorProp = serializedObject.FindProperty("m_fontColor");  // TMP_Pro
                    if (fontColorProp != null)
                    {
                        fontColorProp.colorValue = thisColor;
                        serializedObject.ApplyModifiedProperties();
                    }
                    else
                    {
                        SerializedProperty colorProp = serializedObject.FindProperty("m_Color");
                        if(colorProp != null)
                        {
                            colorProp.colorValue = thisColor;
                            serializedObject.ApplyModifiedProperties();
                        }
                    }

                    Undo.RecordObject(container.Graphic, "ColorToggle");
                    container.Graphic.color = thisColor;
                    break;
                case FieldType.SpriteRenderer:
                    serializedObject = new SerializedObject(container.SpriteRenderer);
                    serializedObject.FindProperty("m_Color").colorValue = thisColor;
                    serializedObject.ApplyModifiedProperties();

                    Undo.RecordObject(container.SpriteRenderer, "ColorToggle");
                    container.SpriteRenderer.color = thisColor;
                    break;
                case FieldType.Button:
                    // Undo.RecordObject(container.Button.targetGraphic, "ColorToggle");
                    // container.Button.targetGraphic.color = thisColor;
                    serializedObject = new SerializedObject(container.Button.targetGraphic);
                    serializedObject.FindProperty("m_Color").colorValue = thisColor;
                    serializedObject.ApplyModifiedProperties();

                    Undo.RecordObject(container.Button.targetGraphic, "ColorToggle");
                    container.Button.targetGraphic.color = thisColor;
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
                case FieldType.Graphic:
                    return container.Graphic.color;
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

        private static string NameHelpBox(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__ColorToggle_HelpBox";
        private static string NameButton(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__ColorToggle_Button";
        // private static string ClassButtonLabel(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__ColorToggle_ButtonLabel";

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

                Container dataContainer = GetContainer(property, saintsAttribute, info, parent);
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
            UpdateToggleDisplay(property, saintsAttribute, index, container, info, parent);
        }

        protected override void OnValueChanged(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container,
            FieldInfo info,
            object parent, Action<object> onValueChangedCallback, object newValue)
        {
            UpdateToggleDisplay(property, saintsAttribute, index, container, info, parent);
        }

        private static void UpdateToggleDisplay(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, FieldInfo info, object parent)
        {
            Container dataContainer = GetContainer(property, saintsAttribute, info, parent);
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

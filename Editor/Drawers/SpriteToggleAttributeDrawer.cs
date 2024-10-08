using System;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
#if UNITY_2021_3_OR_NEWER
using UnityEngine.UIElements;
#endif
using Image = UnityEngine.UI.Image;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(SpriteToggleAttribute))]
    public class SpriteToggleAttributeDrawer: SaintsPropertyDrawer
    {
        private const string SelectedStr = "●";
        private const string NonSelectedStr = "○";

        private enum FieldType
        {
            NotFoundOrValid,
            Image,
            SpriteRenderer,
            // Button,
        }

        private struct Container
        {
            // ReSharper disable InconsistentNaming
            public string Error;
            public FieldType FieldType;
            public Image Image;
            public SpriteRenderer SpriteRenderer;
            // ReSharper enable InconsistentNaming
            // public UnityEngine.UI.Button Button;
        }

        private static Container GetContainer(SerializedProperty property, ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            SpriteToggleAttribute toggleAttribute = (SpriteToggleAttribute)saintsAttribute;
            string imageCompName = toggleAttribute.CompName;

            if(imageCompName != null)
            {
                // SerializedProperty targetProperty =
                //     property.serializedObject.FindProperty(imageCompName) ??
                //     SerializedUtils.FindPropertyByAutoPropertyName(property.serializedObject, imageCompName);
                //
                // if (targetProperty != null)
                // {
                //     if (property.propertyType == SerializedPropertyType.Generic)
                //     {
                //         (string _, IWrapProp getResult) = Util.GetOf<IWrapProp>(property.name, null, property, info, parent);
                //         // Debug.Log(getResult);
                //         if (getResult != null)
                //         {
                //             object actualValue = Util.GetWrapValue(getResult);
                //             // Debug.Log(actualValue);
                //             return SignObject(actualValue);
                //         }
                //         return new Container
                //         {
                //             Error = $"target {imageCompName} not found",
                //         };
                //     }
                //
                //     return SignObject(targetProperty.objectReferenceValue);
                // }

                (string error, object foundObj) =
                    Util.GetOf<object>(imageCompName, null, property, info, parent);

                if (error != "")
                {
                    return new Container
                    {
                        Error = $"target {imageCompName} not found",
                    };
                }

                if (foundObj is IWrapProp wrapProp)
                {
                    foundObj = Util.GetWrapValue(wrapProp);
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

            Image image = thisComponent.GetComponent<Image>();
            if (image != null)
            {
                return SignObject(image);
            }

            UnityEngine.UI.Button button = thisComponent.GetComponent<UnityEngine.UI.Button>();
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
                case GameObject _:
                case Component _:
                {
                    UnityEngine.Object obj = (UnityEngine.Object)foundObj;
                    UnityEngine.Object actualObj = Util.GetTypeFromObj(obj, typeof(Image))
                                                   ?? Util.GetTypeFromObj(obj, typeof(SpriteRenderer))
                                                   // ?? Util.GetTypeFromObj(obj, typeof(Button))
                                                   // ?? Util.GetTypeFromObj(obj, typeof(Renderer))
                                                   ;
                    // Debug.Log($"obj={obj} actual={actualObj}, renderer={((Component)foundObj).GetComponent<Renderer>()}");
                    // ReSharper disable once TailRecursiveCall
                    return SignObject(
                        actualObj
                    );
                }
                // case UnityEngine.UI.Button button:
                //     return new Container
                //     {
                //         FieldType = FieldType.Button,
                //         Button = button,
                //         Error = "",
                //     };
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

        #region IMGUI
        private string _error = "";

        // private SerializedProperty _containerProperty;

        private Container _container;
        // private bool _isUiImage;
        // private Image _image;
        // private SpriteRenderer _spriteRenderer;

        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, OnGUIPayload onGuiPayload, FieldInfo info, object parent)
        {
            // SpriteToggleAttribute toggleAttribute = (SpriteToggleAttribute)saintsAttribute;
            // string imageCompName = toggleAttribute.CompName;

            // Object targetObject = (Object)GetTargetObjectWithProperty(property);
            // Object targetObject = (Object)GetParentTarget(property);

            _container = GetContainer(property, saintsAttribute, info, parent);
            _error = _container.Error;

            // Debug.Log(property.objectReferenceValue);

            if (_error != "")
            {
                // ReSharper disable once Unity.NoNullPropagation
                // _error = $"expect Sprite, get {property.propertyType}({property.objectReferenceValue?.GetType().ToString() ?? "null"})";
                return 0;
            }

            GUIStyle style = new GUIStyle("Button");

            float width = Mathf.Max(style.CalcSize(new GUIContent(SelectedStr)).x, style.CalcSize(new GUIContent(NonSelectedStr)).x);

            return width;
        }

        protected override bool DrawPostFieldImGui(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            // if (_containerProperty == null)
            // {
            //     return false;
            // }

            // if (!(property.objectReferenceValue is Sprite))
            // {
            //     return (false, position);
            // }
            if (_container.Error != "")
            {
                return false;
            }

            Sprite usingSprite = _container.FieldType == FieldType.Image? _container.Image.sprite: _container.SpriteRenderer.sprite;
            Sprite thisSprite = (Sprite) property.objectReferenceValue;

            bool isToggled = ReferenceEquals(usingSprite, thisSprite);

            GUIStyle style = new GUIStyle("Button");

            using (new EditorGUI.DisabledScope(isToggled || thisSprite == null))
            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                bool nowToggled = GUI.Toggle(position, isToggled, isToggled? "●": "○", style);
                // ReSharper disable once InvertIf
                if (nowToggled && changed.changed)
                {
                    if (_container.FieldType == FieldType.Image)
                    {
                        Undo.RecordObject(_container.Image, "SpriteToggle");
                        _container.Image.sprite = thisSprite;
                    }
                    else if (_container.FieldType == FieldType.SpriteRenderer)
                    {
                        Undo.RecordObject(_container.SpriteRenderer, "SpriteToggle");
                        _container.SpriteRenderer.sprite = thisSprite;
                    }
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

        private static string NameHelpBox(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__SpriteToggle_HelpBox";
        private static string NameButton(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__SpriteToggle_Button";
        // private static string NameButtonLabel(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__SpriteToggle_ButtonLabel";

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

            button.RegisterValueChangedCallback(changed =>
            {
                if (!changed.newValue)
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
                    Sprite thisSprite = (Sprite)property.objectReferenceValue;

                    if (dataContainer.FieldType == FieldType.Image)
                    {
                        Undo.RecordObject(dataContainer.Image, "SpriteToggle");
                        dataContainer.Image.sprite = thisSprite;
                    }
                    else if (dataContainer.FieldType == FieldType.SpriteRenderer)
                    {
                        Undo.RecordObject(dataContainer.SpriteRenderer, "SpriteToggle");
                        dataContainer.SpriteRenderer.sprite = thisSprite;
                    }
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
            UpdateToggleDisplay(property, index, saintsAttribute, container, info);
        }

        protected override void OnValueChanged(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container,
            FieldInfo info,
            object parent, Action<object> onValueChangedCallback, object newValue)
        {
            UpdateToggleDisplay(property, index, saintsAttribute, container, info);
        }

        private static void UpdateToggleDisplay(SerializedProperty property, int index, ISaintsAttribute saintsAttribute, VisualElement container, FieldInfo info)
        {
            object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
            if (parent == null)
            {
                Debug.LogWarning($"{property.propertyPath} parent disposed unexpectedly.");
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

            if (error != "")
            {
                return;
            }

            Sprite thisSprite = (Sprite)property.objectReferenceValue;

            Sprite usingSprite = dataContainer.FieldType == FieldType.Image
                ? dataContainer.Image.sprite
                : dataContainer.SpriteRenderer.sprite;

            bool isToggled = ReferenceEquals(thisSprite, usingSprite);
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

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
using Object = UnityEngine.Object;

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
            public string Error;
            public FieldType FieldType;
            public UnityEngine.UI.Image Image;
            public SpriteRenderer SpriteRenderer;
            // public UnityEngine.UI.Button Button;
        }

        private static Container GetContainer(ISaintsAttribute saintsAttribute, object parent)
        {
            SpriteToggleAttribute toggleAttribute = (SpriteToggleAttribute)saintsAttribute;
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

        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            // SpriteToggleAttribute toggleAttribute = (SpriteToggleAttribute)saintsAttribute;
            // string imageCompName = toggleAttribute.CompName;

            // Object targetObject = (Object)GetTargetObjectWithProperty(property);
            Object targetObject = (Object)GetParentTarget(property);
            // SerializedObject targetSer = new SerializedObject(targetObject);

            // _containerProperty =
            //     targetSer.FindProperty(imageCompName) ?? targetSer.FindProperty($"<{imageCompName}>k__BackingField");
            //
            // if(_containerProperty == null)
            // {
            //     _error = $"target {imageCompName} not found";
            //     return 0;
            // }

            // Debug.Log(_containerProperty.objectReferenceValue);
            // Debug.Log(_containerProperty.objectReferenceValue is Image);
            // Debug.Log(_containerProperty.objectReferenceValue is SpriteRenderer);

            _container = GetContainer(saintsAttribute, targetObject);
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
            ISaintsAttribute saintsAttribute, bool valueChanged)
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

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute) => _error == "" ? 0 : ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => _error == "" ? position : ImGuiHelpBox.Draw(position, _error, MessageType.Error);
        #endregion

#if UNITY_2021_3_OR_NEWER

        #region UIToolkit

        private static string NameLabelError(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__SpriteToggle_LabelError";
        private static string NameButton(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__SpriteToggle_Button";
        private static string NameButtonLabel(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__SpriteToggle_ButtonLabel";

        protected override VisualElement CreatePostFieldUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, object parent)
        {
            Button button = new Button(() =>
            {
                Container dataContainer = GetContainer(saintsAttribute, parent);
                string error = dataContainer.Error;
                HelpBox helpBox = container.Q<HelpBox>(NameLabelError(property, index));
                helpBox.style.display = error == ""? DisplayStyle.None: DisplayStyle.Flex;
                helpBox.text = error;

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
                    container.Q<Label>(NameButtonLabel(property, index)).text = SelectedStr;
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

            return button;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, object parent)
        {
            return new HelpBox("", HelpBoxMessageType.Error)
            {
                name = NameLabelError(property, index),
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

        private static void UpdateToggleDisplay(SerializedProperty property, int index, ISaintsAttribute saintsAttribute, VisualElement container, object parent)
        {
            Container dataContainer = GetContainer(saintsAttribute, parent);
            string error = dataContainer.Error;
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

            Sprite thisSprite = (Sprite)property.objectReferenceValue;
            Sprite usingSprite = dataContainer.FieldType == FieldType.Image
                ? dataContainer.Image.sprite
                : dataContainer.SpriteRenderer.sprite;

            bool isToggled = ReferenceEquals(thisSprite, usingSprite);
            string expectedString = isToggled ? SelectedStr : NonSelectedStr;
            Label label = container.Q<Label>(NameButtonLabel(property, index));
            if (label.text != expectedString)
            {
                label.text = expectedString;
            }
        }

        #endregion

#endif
    }
}

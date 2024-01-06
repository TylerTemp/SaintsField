using System;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(ColorToggleAttribute))]
    public class ColorToggleAttributeDrawer: SaintsPropertyDrawer
    {
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

        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            ColorToggleAttribute toggleAttribute = (ColorToggleAttribute)saintsAttribute;
            string imageCompName = toggleAttribute.CompName;

            Object targetObject = (Object) GetParentTarget(property);
            SerializedObject targetSer = new SerializedObject(targetObject);

            _error = "";
            if(imageCompName != null)
            {
                SerializedProperty targetProperty =
                    targetSer.FindProperty(imageCompName) ??
                    SerializedUtils.FindPropertyByAutoPropertyName(targetSer, imageCompName);

                if (targetProperty != null)
                {
                    SignObject(targetProperty.objectReferenceValue);
                }
                else
                {
                    Type targetType = targetObject.GetType();

                    // (ReflectUtils.GetPropType getPropType, object fieldOrMethodInfo) propInfo =
                    //     ReflectUtils.GetProp(targetType, imageCompName);
                    // switch (propInfo)
                    // {
                    //     case (ReflectUtils.GetPropType.NotFound, _):
                    //         _error = $"target {imageCompName} not found";
                    //         return 0;
                    //     case (ReflectUtils.GetPropType.Field, FieldInfo foundFieldInfo):
                    //         SignObject(foundFieldInfo.GetValue(targetObject));
                    //         break;
                    //     case (ReflectUtils.GetPropType.Property, PropertyInfo foundPropertyInfo):
                    //         SignObject(foundPropertyInfo.GetValue(targetObject));
                    //         break;
                    //     case (ReflectUtils.GetPropType.Method, MethodInfo foundMethodInfo):
                    //         SignObject(foundMethodInfo.Invoke(targetObject, null));
                    //         break;
                    // }

                    (ReflectUtils.GetPropType getPropType, object fieldOrMethodInfo) propInfo =
                        ReflectUtils.GetProp(targetType, imageCompName);

                    if (propInfo.Item1 == ReflectUtils.GetPropType.NotFound)
                    {
                        _error = $"target {imageCompName} not found";
                        return 0;
                    }
                    else if (propInfo.Item1 == ReflectUtils.GetPropType.Field && propInfo.Item2 is FieldInfo foundFieldInfo)
                    {
                        SignObject(foundFieldInfo.GetValue(targetObject));
                    }
                    else if (propInfo.Item1 == ReflectUtils.GetPropType.Property && propInfo.Item2 is PropertyInfo foundPropertyInfo)
                    {
                        SignObject(foundPropertyInfo.GetValue(targetObject));
                    }
                    else if (propInfo.Item1 == ReflectUtils.GetPropType.Method && propInfo.Item2 is MethodInfo foundMethodInfo)
                    {
                        SignObject(foundMethodInfo.Invoke(targetObject, null));
                    }

                }
            }
            else
            {
                Component thisComponent;
                try
                {
                    thisComponent = (Component)targetObject;
                }
                catch (InvalidCastException e)
                {
                    Debug.LogException(e);
                    _error = $"target {targetObject} is not a Component";
                    return 0;
                }

                Image image = thisComponent.GetComponent<Image>();
                if (image != null)
                {
                    SignObject(image);
                }
                else
                {
                    Button button = thisComponent.GetComponent<Button>();
                    if (button)
                    {
                        SignObject(button);
                    }

                    else
                    {
                        SpriteRenderer spriteRenderer = thisComponent.GetComponent<SpriteRenderer>();
                        if (spriteRenderer)
                        {
                            SignObject(spriteRenderer);
                        }
                        else
                        {
                            Renderer renderer = thisComponent.GetComponent<Renderer>();
                            if (renderer)
                            {
                                SignObject(renderer);
                            }
                            else
                            {
                                _error = $"target {targetObject} has no Image, SpriteRenderer, Button or Renderer";
                                return 0;
                            }
                        }
                    }
                }
            }

            if (_error != "")
            {
                return 0;
            }

            GUIStyle style = new GUIStyle("Button");

            float width = Mathf.Max(style.CalcSize(new GUIContent(SelectedStr)).x, style.CalcSize(new GUIContent(NonSelectedStr)).x);

            return width;
        }

        private void SignObject(object foundObj)
        {
            switch (foundObj)
            {
                case Image image:
                    _container = new Container
                    {
                        FieldType = FieldType.Image,
                        Image = image,
                    };
                    break;
                case SpriteRenderer spriteRenderer:
                    _container = new Container
                    {
                        FieldType = FieldType.SpriteRenderer,
                        SpriteRenderer = spriteRenderer,
                    };
                    break;
                case Button button:
                    _container = new Container
                    {
                        FieldType = FieldType.Button,
                        Button = button,
                    };
                    break;
                case Renderer renderer:
                    _container = new Container
                    {
                        FieldType = FieldType.Renderer,
                        Renderer = renderer,
                    };
                    break;
                default:
                    _error = $"Not supported type: {(foundObj == null ? "null" : foundObj.GetType().ToString())}";
                    _container = new Container { FieldType = FieldType.NotFoundOrValid };
                    break;
            }
        }

        protected override bool DrawPostFieldImGui(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, bool valueChanged)
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

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute) => _error == "" ? 0 : ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => _error == "" ? position : ImGuiHelpBox.Draw(position, _error, MessageType.Error);
    }
}

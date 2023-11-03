using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(SpriteToggleAttribute))]
    public class SpriteToggleAttributeDrawer: SaintsPropertyDrawer
    {
        private string _error = "";

        private SerializedProperty _containerProperty;
        private bool _isUiImage;
        private Image _image;
        private SpriteRenderer _spriteRenderer;

        private const string SelectedStr = "●";
        private const string NonSelectedStr = "○";

        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            SpriteToggleAttribute toggleAttribute = (SpriteToggleAttribute)saintsAttribute;
            string imageCompName = toggleAttribute.CompName;

            // Object targetObject = (Object)GetTargetObjectWithProperty(property);
            Object targetObject = property.serializedObject.targetObject;
            SerializedObject targetSer = new SerializedObject(targetObject);

            _containerProperty =
                targetSer.FindProperty(imageCompName) ?? targetSer.FindProperty($"<{imageCompName}>k__BackingField");

            if(_containerProperty == null)
            {
                _error = $"target {imageCompName} not found";
                return 0;
            }

            // Debug.Log(_containerProperty.objectReferenceValue);
            // Debug.Log(_containerProperty.objectReferenceValue is Image);
            // Debug.Log(_containerProperty.objectReferenceValue is SpriteRenderer);

            switch (_containerProperty.objectReferenceValue)
            {
                case Image image:
                    _isUiImage = true;
                    _image = image;
                    _spriteRenderer = null;
                    break;
                case SpriteRenderer spriteRenderer:
                    _isUiImage = false;
                    _spriteRenderer = spriteRenderer;
                    _image = null;
                    break;
                default:
                    _error =
                        // ReSharper disable once Unity.NoNullPropagation
                        $"expect target is Image or SpriteRenderer, get {_containerProperty.propertyType}({_containerProperty.objectReferenceValue?.GetType().ToString() ?? "null"})";
                    return 0;
            }

            // Debug.Log(property.objectReferenceValue);

            if (!(property.objectReferenceValue is Sprite || property.objectReferenceValue == null))
            {
                // ReSharper disable once Unity.NoNullPropagation
                _error = $"expect Sprite, get {property.propertyType}({property.objectReferenceValue?.GetType().ToString() ?? "null"})";
                return 0;
            }

            _error = "";

            GUIStyle style = new GUIStyle("Button");

            float width = Mathf.Max(style.CalcSize(new GUIContent(SelectedStr)).x, style.CalcSize(new GUIContent(NonSelectedStr)).x);

            return width;
        }

        protected override bool DrawPostField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, bool valueChanged)
        {
            if (_containerProperty == null)
            {
                return false;
            }

            // if (!(property.objectReferenceValue is Sprite))
            // {
            //     return (false, position);
            // }
            if ((_isUiImage && _image == null) || (!_isUiImage && _spriteRenderer == null))
            {
                return false;
            }

            Sprite usingSprite = _isUiImage? _image.sprite: _spriteRenderer.sprite;
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
                    SerializedObject containerSer = _isUiImage
                        ? new SerializedObject(_image)
                        : new SerializedObject(_spriteRenderer);
                    containerSer.FindProperty("m_Sprite").objectReferenceValue = thisSprite;
                    containerSer.ApplyModifiedProperties();
                }
            }

            return true;
        }

        protected override bool WillDrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute) => _error == "" ? 0 : HelpBox.GetHeight(_error, width, MessageType.Error);

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => _error == "" ? position : HelpBox.Draw(position, _error, MessageType.Error);
    }
}

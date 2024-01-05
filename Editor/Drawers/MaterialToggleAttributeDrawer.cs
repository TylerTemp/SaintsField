using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(MaterialToggleAttribute))]
    public class MaterialToggleAttributeDrawer: SaintsPropertyDrawer
    {
        private string _error = "";

        private Renderer _renderer;
        private Material _material;

        private const string SelectedStr = "●";
        private const string NonSelectedStr = "○";

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
            if (_renderer == null)
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
                    // SerializedObject containerSer = new SerializedObject(_renderer);
                    // containerSer.FindProperty("m_Sprite").objectReferenceValue = thisSprite;
                    // containerSer.ApplyModifiedProperties();
                }
            }

            return true;
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute) => _error == "" ? 0 : HelpBox.GetHeight(_error, width, MessageType.Error);

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => _error == "" ? position : HelpBox.Draw(position, _error, MessageType.Error);
    }
}

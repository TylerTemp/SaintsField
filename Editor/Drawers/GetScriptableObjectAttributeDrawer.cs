using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(GetScriptableObjectAttribute))]
    public class GetScriptableObjectAttributeDrawer: SaintsPropertyDrawer
    {
        #region IMGUI
        private string _error = "";

        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => 0;

        protected override bool DrawPostFieldImGui(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute,
            bool valueChanged)
        {
            (string error, Object result) = DoCheckComponent(property, saintsAttribute);
            if (error != "")
            {
                _error = error;
                return false;
            }
            if(result != null)
            {
                SetValueChanged(property);
            }
            return true;
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            ISaintsAttribute saintsAttribute) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute) => _error == ""? 0: ImGuiHelpBox.GetHeight(_error, width, EMessageType.Error);
        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => _error == ""? position: ImGuiHelpBox.Draw(position, _error, EMessageType.Error);

        #endregion

        private static (string error, UnityEngine.Object result) DoCheckComponent(SerializedProperty property, ISaintsAttribute saintsAttribute)
        {
            if (property.objectReferenceValue != null)
            {
                return ("", null);
            }

            GetScriptableObjectAttribute getScriptableObjectAttribute = (GetScriptableObjectAttribute) saintsAttribute;

            Type fieldType = SerializedUtils.GetType(property);

            IEnumerable<string> paths = AssetDatabase.FindAssets($"t:{fieldType.Name}")
                .Select(AssetDatabase.GUIDToAssetPath);

            if (getScriptableObjectAttribute.PathSuffix != null)
            {
                paths = paths.Where(each => each.EndsWith(getScriptableObjectAttribute.PathSuffix));
            }
            Object result = paths
                .Select(each => AssetDatabase.LoadAssetAtPath(each, fieldType))
                .FirstOrDefault(each => each != null);

            if (result == null)
            {
                return ($"Can not find {fieldType} type asset", null);
            }

            property.objectReferenceValue = result;
            return ("", result);
        }

        #region UIToolkit

        private static string NamePlaceholder(SerializedProperty property, int index) =>
            $"{property.propertyPath}_{index}__GetScriptableObject";

        protected override VisualElement CreatePostFieldUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, object parent,
            Action<object> onChange)
        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_GET_SCRIPTABLE_OBJECT
            Debug.Log($"GetScriptableObject DrawPostFieldUIToolkit for {property.propertyPath}");
#endif
            (string error, Object result) = DoCheckComponent(property, saintsAttribute);
            if (error != "")
            {
                return new VisualElement
                {
                    style =
                    {
                        width = 0,
                    },
                    name = NamePlaceholder(property, index),
                    userData = error,
                };
            }

            property.serializedObject.ApplyModifiedProperties();

            onChange?.Invoke(result);

            return new VisualElement
            {
                style =
                {
                    width = 0,
                },
                name = NamePlaceholder(property, index),
                userData = "",
            };
        }

        // NOTE: ensure the post field is added to the container!
        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, object parent)
        {
            string error = (string)(container.Q<VisualElement>(NamePlaceholder(property, index))!.userData ?? "");
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_GET_SCRIPTABLE_OBJECT
            Debug.Log($"GetScriptableObject error {error}");
#endif
            return string.IsNullOrEmpty(error)
                ? null
                : new HelpBox(_error, HelpBoxMessageType.Error);
        }
        #endregion
    }
}

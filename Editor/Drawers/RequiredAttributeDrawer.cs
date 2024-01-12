using System;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(RequiredAttribute))]
    public class RequiredAttributeDrawer: SaintsPropertyDrawer
    {
        #region IMGUI
        private string _error = "";

        protected override bool DrawPostFieldImGui(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, bool valueChanged)
        {
            property.serializedObject.ApplyModifiedProperties();
            if (Truly(property))
            {
                _error = "";
                return true;
            }

            string errorMessage = ((RequiredAttribute)saintsAttribute).ErrorMessage;
            _error = errorMessage ?? $"{property.displayName} is required";
            return true;
        }

        private static bool Truly(SerializedProperty property)
        {
            UnityEngine.Object target = property.serializedObject.targetObject;

            (ReflectUtils.GetPropType getPropType, object fieldOrMethodInfo) found = ReflectUtils.GetProp(target.GetType(), property.name);

            if (found.getPropType == ReflectUtils.GetPropType.Property && found.fieldOrMethodInfo is PropertyInfo propertyInfo)
            {
                return ReflectUtils.Truly(propertyInfo.GetValue(target));
            }

            if (found.getPropType == ReflectUtils.GetPropType.Field && found.fieldOrMethodInfo is FieldInfo foundFieldInfo)
            {
                return ReflectUtils.Truly(foundFieldInfo.GetValue(target));
            }
            if (found.getPropType == ReflectUtils.GetPropType.NotFound || found.getPropType == ReflectUtils.GetPropType.Method)
            {
                throw new ArgumentOutOfRangeException(nameof(found.getPropType), found.getPropType, null);
            }
            // Handle any other cases here, if needed
            throw new NotImplementedException("Unexpected case");
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute) =>
            (_error = ValidateType(property)) != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute) =>
            (_error = ValidateType(property)) == "" ? 0 : ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) =>
            (_error = ValidateType(property)) == "" ? position : ImGuiHelpBox.Draw(position, _error, MessageType.Error);

        private static string ValidateType(SerializedProperty property)
        {
            if (property.propertyType == SerializedPropertyType.Integer)
            {
                return $"`{property.displayName}` can not be a valued type: int";
            }
            if (property.propertyType == SerializedPropertyType.Float)
            {
                return $"`{property.displayName}` can not be a valued type: float";
            }

            // ReSharper disable once ConvertIfStatementToReturnStatement
            Type curType = SerializedUtils.GetType(property);
            if (curType.IsValueType)
            {
                return $"`{property.displayName}` can not be a valued type: {curType}";
            }

            return "";
        }

        #endregion

        private static string NameRequiredBox(SerializedProperty property, int index) =>
            $"{property.propertyPath}_{index}__Required";

        private struct MetaInfo
        {
            public bool TypeError;
            public bool IsTruly;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container, object parent)
        {
            string typeError = ValidateType(property);

            Debug.Log(typeError);
            return new HelpBox(typeError, HelpBoxMessageType.Error)
            {
                style =
                {
                    display = typeError == ""? DisplayStyle.None : DisplayStyle.Flex,
                },
                name = NameRequiredBox(property, index),
                userData = new MetaInfo
                {
                    TypeError = typeError != "",
                    IsTruly = true,
                },
            };
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container, object parent)
        {
            HelpBox helpBox = container.Q<HelpBox>(NameRequiredBox(property, index));
            MetaInfo metaInfo = (MetaInfo)helpBox.userData;

            if (metaInfo.TypeError)
            {
                return;
            }

            bool isTruly = Truly(property);

            // ReSharper disable once InvertIf
            if(isTruly != metaInfo.IsTruly)
            {

                Debug.Log($"isTruly={isTruly}; meta.isTruly={metaInfo.IsTruly}");
                helpBox.style.display = isTruly ? DisplayStyle.None : DisplayStyle.Flex;

                string errorMessage = ((RequiredAttribute)saintsAttribute).ErrorMessage;
                string error = errorMessage ?? $"{property.displayName} is required";
                helpBox.text = error;

                helpBox.userData = new MetaInfo
                {
                    TypeError = false,
                    IsTruly = isTruly,
                };
            }


        }
    }
}

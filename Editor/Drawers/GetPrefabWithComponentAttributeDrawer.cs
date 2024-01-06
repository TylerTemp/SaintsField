using System;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(GetPrefabWithComponentAttribute))]
    public class GetPrefabWithComponentAttributeDrawer: SaintsPropertyDrawer
    {
        private string _error = "";

        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => 0;

        protected override bool DrawPostFieldImGui(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute,
            bool valueChanged)
        {
            _error = "";

            if (property.objectReferenceValue != null)
            {
                return false;
            }

            GetPrefabWithComponentAttribute getPrefabWithComponentAttribute = (GetPrefabWithComponentAttribute) saintsAttribute;
            Type fieldType = SerializedUtils.GetType(property);

            if (getPrefabWithComponentAttribute.CompType == typeof(GameObject))
            {
                _error = "You can not use GetPrefabWithComponentAttribute with GameObject type";
                return false;
            }

            Type type = getPrefabWithComponentAttribute.CompType ?? fieldType;

            Component prefabWithComponent = null;

            string[] guids = AssetDatabase.FindAssets("t:Prefab");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject toCheck = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (toCheck is null)
                {
                    continue;
                }

                Component findSelfComponent = toCheck.GetComponent(type);
                if (findSelfComponent != null)
                {
                    prefabWithComponent = findSelfComponent;
                    break;
                }

                // Component findComponent = rootGameObject.GetComponentInChildren(type, includeInactive);
                // // ReSharper disable once InvertIf
                // if (findComponent != null)
                // {
                //     prefabWithComponent = findComponent;
                //     break;
                // }
            }

            if (prefabWithComponent == null)
            {
                _error = $"No {type} found with prefab";
                return false;
            }

            UnityEngine.Object result = prefabWithComponent;

            if (fieldType != type)
            {
                if(fieldType == typeof(GameObject))
                {
                    result = prefabWithComponent.gameObject;
                }
                else
                {
                    result = prefabWithComponent.GetComponent(fieldType);
                }
            }

            property.objectReferenceValue = result;
            SetValueChanged(property);
            return true;
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            ISaintsAttribute saintsAttribute) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute) => _error == ""? 0: ImGuiHelpBox.GetHeight(_error, width, EMessageType.Error);
        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => _error == ""? position: ImGuiHelpBox.Draw(position, _error, EMessageType.Error);
    }
}

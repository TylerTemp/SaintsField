using System;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(GetComponentInSceneAttribute))]
    public class GetComponentInSceneAttributeDrawer: SaintsPropertyDrawer
    {
        private string _error = "";

        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => 0;

        protected override bool DrawPostField(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute,
            bool valueChanged)
        {
            _error = "";

            if (property.objectReferenceValue != null)
            {
                return false;
            }

            GetComponentInSceneAttribute getComponentInSceneAttribute = (GetComponentInSceneAttribute) saintsAttribute;
            Type fieldType = SerializedUtils.GetType(property);

            if (getComponentInSceneAttribute.CompType == typeof(GameObject))
            {
                _error = "You can not use GetComponentInChildrenAttribute with GameObject type";
                return false;
            }

            Type type = getComponentInSceneAttribute.CompType ?? fieldType;

            Component componentInScene = null;

            Scene scene = SceneManager.GetActiveScene();
            bool includeInactive = getComponentInSceneAttribute.IncludeInactive;
            foreach (GameObject rootGameObject in scene.GetRootGameObjects())
            {
                if (!includeInactive && !rootGameObject.activeSelf)
                {
                    continue;
                }

                Component findSelfComponent = rootGameObject.GetComponent(type);
                if (findSelfComponent != null)
                {
                    componentInScene = findSelfComponent;
                    break;
                }

                Component findComponent = rootGameObject.GetComponentInChildren(type, includeInactive);
                // ReSharper disable once InvertIf
                if (findComponent != null)
                {
                    componentInScene = findComponent;
                    break;
                }
            }

            if (componentInScene == null)
            {
                _error = $"No {type} found in scene";
                return false;
            }

            UnityEngine.Object result = componentInScene;

            if (fieldType != type)
            {
                if(fieldType == typeof(GameObject))
                {
                    result = componentInScene.gameObject;
                }
                else
                {
                    result = componentInScene.GetComponent(fieldType);
                }
            }

            property.objectReferenceValue = result;
            SetValueChanged(property);
            return true;
        }

        protected override bool WillDrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute) => _error == ""? 0: HelpBox.GetHeight(_error, width, EMessageType.Error);
        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => _error == ""? position: HelpBox.Draw(position, _error, EMessageType.Error);
    }
}

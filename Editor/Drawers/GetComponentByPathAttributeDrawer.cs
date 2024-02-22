using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = System.Object;
#if UNITY_2021_3_OR_NEWER
using System.Reflection;
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(GetComponentByPathAttribute))]
    public class GetComponentByPathAttributeDrawer: SaintsPropertyDrawer
    {
        #region IMGUI
        #endregion

#if UNITY_2021_3_OR_NEWER
        #region UIToolkit
        private static string NamePlaceholder(SerializedProperty property, int index) =>
            $"{property.propertyPath}_{index}__GetComponentByPath";

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
        {
            return new HelpBox("", HelpBoxMessageType.Error)
            {
                style =
                {
                    display = DisplayStyle.None,
                },
                name = NamePlaceholder(property, index),
            };
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChangedCallback, object parent)
        {
            if (property.objectReferenceValue == null)
            {
                Check(property, (GetComponentByPathAttribute)saintsAttribute, index, container, onValueChangedCallback);
            }
        }

        private static void Check(SerializedProperty property, GetComponentByPathAttribute getComponentByPathAttribute,
            int index,
            VisualElement container, Action<object> onValueChangedCallback)
        {
// #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DROW_PROCESS_GET_COMPONENT_BY_PATH
// #endif
            (string error, Object result) = DoCheckComponent(property, getComponentByPathAttribute);
            HelpBox helpBox = container.Q<HelpBox>(NamePlaceholder(property, index));
            if (error != helpBox.text)
            {
                helpBox.style.display = error == "" ? DisplayStyle.None : DisplayStyle.Flex;
                helpBox.text = error;
            }
            else
            {
                property.serializedObject.ApplyModifiedProperties();
                onValueChangedCallback.Invoke(result);
            }
        }
        #endregion
#endif

        private static (string error, Object result) DoCheckComponent(SerializedProperty property, GetComponentByPathAttribute getComponentByPathAttribute)
        {
            Type fieldType = SerializedUtils.GetType(property);
            Type type = getComponentByPathAttribute.CompType ?? fieldType;

            Transform transform;
            switch (property.serializedObject.targetObject)
            {
                case Component component:
                    transform = component.transform;
                    break;
                case GameObject gameObject:
                    transform = gameObject.transform;
                    break;
                default:
                    return ("GetComponentInChildrenAttribute can only be used on Component or GameObject", null);
            }

            IReadOnlyList<IReadOnlyList<GetComponentByPathAttribute.Token>> tokensPaths = getComponentByPathAttribute.Paths;
            Object result = tokensPaths
                .Select(tokens => FindObjectByPath(tokens, type, transform))
                .FirstOrDefault(each => each != null);

            return ("test error", null);
        }

        private static Object FindObjectByPath(IReadOnlyList<GetComponentByPathAttribute.Token> tokens, Type type, Transform current)
        {
            bool isGameObject = type == typeof(GameObject);
            return IteratePath(new Queue<GetComponentByPathAttribute.Token>(tokens), new[] { current })
                .Select(each => (Object)(isGameObject ? each.gameObject : each.GetComponent(type)))
                .FirstOrDefault(each => each != null);
        }

        private static IReadOnlyList<Transform> IteratePath(Queue<GetComponentByPathAttribute.Token> tokens, IReadOnlyList<Transform> currents)
        {
            if (tokens.Count == 0)
            {
                return currents;
            }

            GetComponentByPathAttribute.Token token = tokens.Dequeue();

            // TODO: indexing

            switch (token.Locate)
            {
                case GetComponentByPathAttribute.Locate.Root:
                {
                    // TODO: support root for prefab
                    return IteratePath(
                        tokens,
                        SceneManager.GetActiveScene().GetRootGameObjects().Select(each => each.transform).ToArray());
                }
                case GetComponentByPathAttribute.Locate.Descendant:
                {
                    return IteratePath(
                        tokens,
                        currents
                            .SelectMany(each => each.GetComponentsInChildren<Transform>())
                            .Where(each => each.gameObject.name == token.Node)
                            .ToArray());
                }
                case GetComponentByPathAttribute.Locate.Child:
                {
                    return IteratePath(
                        tokens,
                        currents
                            .SelectMany(each => each.Cast<Transform>())
                            .Where(each => each.gameObject.name == token.Node)
                            .ToArray());
                }
            }
        }
    }
}

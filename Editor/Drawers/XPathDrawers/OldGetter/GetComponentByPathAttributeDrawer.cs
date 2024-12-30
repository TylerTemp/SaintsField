using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using SaintsField.Editor.Core;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_2021_3_OR_NEWER
using UnityEngine.UIElements;
#endif
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.XPathDrawers.OldGetter
{
    [CustomPropertyDrawer(typeof(GetComponentByPathAttribute))]
    public class GetComponentByPathAttributeDrawer: SaintsPropertyDrawer
    {
        #region IMGUI
        private Texture2D _refreshIcon;
        private string _error = "";

        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, OnGUIPayload onGuiPayload, FieldInfo info, object parent)
        {
            GetComponentByPathAttribute getComponentByPathAttribute = (GetComponentByPathAttribute)saintsAttribute;
            if (!getComponentByPathAttribute.ResignButton)
            {
                return 0;
            }

            (string error, SerializedProperty targetProperty, IReadOnlyList<Object> results) = DoCheckComponent(property, getComponentByPathAttribute, info, parent);
            _error = error;
            if (error != "")
            {
                return 0;
            }

            int indexInArray = SerializedUtils.PropertyPathIndex(property.propertyPath);
            Object result = indexInArray == -1
                // ReSharper disable once ArrangeRedundantParentheses
                ? (results.Count > 0? results[0]: null)
                : results[indexInArray];

            return ReferenceEquals(targetProperty.objectReferenceValue, result) ? 0 : SingleLineHeight;
        }

        // private bool _firstOpen = true;
        private readonly Dictionary<string, bool> _drawerAlreadyOpenedOnceCache = new Dictionary<string, bool>();

        protected override bool DrawPostFieldImGui(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute,
            int index,
            OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            // Debug.Log($"init first open {_firstOpen}");
            _error = "";
            GetComponentByPathAttribute getComponentByPathAttribute = (GetComponentByPathAttribute)saintsAttribute;

            // match?
            (string error, SerializedProperty targetProperty, IReadOnlyList<Object> results) = DoCheckComponent(property, getComponentByPathAttribute, info, parent);
            _error = error;

            if (error != "")
            {
                return false;
            }

            int indexInArray = SerializedUtils.PropertyPathIndex(property.propertyPath);
            string cacheKey = $"{indexInArray}_{index}";
            _drawerAlreadyOpenedOnceCache.TryGetValue(cacheKey, out bool alreadyOpenedOnce);
            bool firstOpen = !alreadyOpenedOnce;

            Object result = indexInArray == -1
                // ReSharper disable once ArrangeRedundantParentheses
                ? (results.Count > 0? results[0]: null)
                : results[indexInArray];

            // Debug.Log($"{indexInArray} firstOpen={firstOpen} cur={targetProperty.objectReferenceValue} result={result}; keys={string.Join(",", _drawerAlreadyOpenedOnceCache.Keys)}");

            // Debug.Log($"fr={getComponentByPathAttribute.ForceResign}, equal={ReferenceEquals(property.objectReferenceValue, result)}");
            if(((firstOpen && targetProperty.objectReferenceValue == null) || getComponentByPathAttribute.ForceResign) && !ReferenceEquals(targetProperty.objectReferenceValue, result))
            {
                // Debug.Log($"firstOpen={firstOpen}; auto sign to {result}");
                targetProperty.objectReferenceValue = result;
                // property.serializedObject.ApplyModifiedProperties();
                // valueChanged = true;
            }

            bool willDraw = false;
            if(!ReferenceEquals(targetProperty.objectReferenceValue, result) && getComponentByPathAttribute.ResignButton)
            {
                if (_refreshIcon == null)
                {
                    _refreshIcon = Util.LoadResource<Texture2D>("refresh.png");
                }

                willDraw = true;
                if (GUI.Button(position, _refreshIcon))
                {
                    property.objectReferenceValue = result;
                    // property.serializedObject.ApplyModifiedProperties();
                    // valueChanged = true;
                }
            }

            // Debug.Log($"opened: {cacheKey}");
            _drawerAlreadyOpenedOnceCache[cacheKey] = true;

            return willDraw;
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info, object parent) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info, object parent)
        {
            if (_error == "")
            {
                return 0;
            }

            return ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info, object parent)
        {
            return ImGuiHelpBox.Draw(position, _error, MessageType.Error);
        }

        #endregion

#if UNITY_2021_3_OR_NEWER
        #region UIToolkit
        private static string NameHelpBox(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__GetComponentByPath_HelpBox";
        private static string NameResignButton(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__GetComponentByPath_ResignButton";

        protected override VisualElement CreatePostFieldUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
        {
            GetComponentByPathAttribute getComponentByPathAttribute = (GetComponentByPathAttribute)saintsAttribute;

            if (!getComponentByPathAttribute.ResignButton)
            {
                return base.CreatePostFieldUIToolkit(property, saintsAttribute, index, container, info, parent);
            }

            Texture2D refreshIcon = Util.LoadResource<Texture2D>("refresh.png");
            Image image = new Image {image = refreshIcon};
            Button button = new Button
            {
                // style =
                // {
                //     backgroundImage = refreshIcon,
                // },
                // text = "⟳",
                style =
                {
                    height = SingleLineHeight,
                    width = SingleLineHeight,
                    display = DisplayStyle.None,
                    paddingLeft = 0,
                    paddingRight = 0,
                    paddingTop = 0,
                    paddingBottom = 0,
                    marginLeft = 0,
                    marginRight = 0,
                    marginTop = 0,
                    marginBottom = 0,
                },
                name = NameResignButton(property, index),
            };
            button.Add(image);
            button.AddToClassList(ClassAllowDisable);

            return button;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
        {
            HelpBox helpBox = new HelpBox("", HelpBoxMessageType.Error)
            {
                style =
                {
                    display = DisplayStyle.None,
                },
                name = NameHelpBox(property, index),
            };
            helpBox.AddToClassList(ClassAllowDisable);
            return helpBox;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            GetComponentByPathAttribute getComponentByPathAttribute = (GetComponentByPathAttribute)saintsAttribute;
            Button button = getComponentByPathAttribute.ResignButton? container.Q<Button>(NameResignButton(property, index)): null;
            HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property, index));
            (string _, SerializedProperty targetProperty, Type __, Type ___) = GetPropAndType(property, info, parent);
            // ReSharper disable once MergeIntoPattern
            if (targetProperty != null && targetProperty.propertyType == SerializedPropertyType.ObjectReference && targetProperty.objectReferenceValue == null)
            {
                Check(property, getComponentByPathAttribute, info, button, helpBox, onValueChangedCallback, true, parent);
            }

            if(button != null)
            {
                button.clicked += () => Check(property, (GetComponentByPathAttribute)saintsAttribute, info, button, helpBox,
                    onValueChangedCallback, true, parent);
            }
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info)
        {
            GetComponentByPathAttribute getComponentByPathAttribute = (GetComponentByPathAttribute)saintsAttribute;
            // ReSharper disable once InvertIf
            if(getComponentByPathAttribute.ForceResign || getComponentByPathAttribute.ResignButton)
            {
                object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
                if (parent == null)
                {
                    Debug.LogWarning($"{property.propertyPath} parent disposed unexpectedly.");
                    return;
                }

                Button button = getComponentByPathAttribute.ResignButton? container.Q<Button>(NameResignButton(property, index)): null;
                HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property, index));
                Check(property, getComponentByPathAttribute, info, button, helpBox, onValueChangedCallback, false, parent);
            }
        }

        private static void Check(SerializedProperty property, GetComponentByPathAttribute getComponentByPathAttribute,
            FieldInfo info,
            // ReSharper disable once SuggestBaseTypeForParameter
            Button button, HelpBox helpBox, Action<object> onValueChangedCallback, bool forceResign, object parent)
        {
            if (EditorApplication.isPlaying)
            {
                return;
            }

            (string error, SerializedProperty targetProperty, IReadOnlyList<Object> results) = DoCheckComponent(property, getComponentByPathAttribute, info, parent);
            // HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property, index));
            if (error != helpBox.text)
            {
                helpBox.style.display = error == "" ? DisplayStyle.None : DisplayStyle.Flex;
                helpBox.text = error;

                if (getComponentByPathAttribute.ForceResign && property.objectReferenceValue != null)
                {
                    targetProperty.objectReferenceValue = null;
                    property.serializedObject.ApplyModifiedProperties();
                    onValueChangedCallback.Invoke(null);
                }

                if (getComponentByPathAttribute.ResignButton)
                {
                    button.style.display = DisplayStyle.None;
                }
                return;
            }

            int indexInArray = SerializedUtils.PropertyPathIndex(property.propertyPath);

            // Debug.Log(indexInArray);
            Object result = indexInArray == -1
                // ReSharper disable once ArrangeRedundantParentheses
                ? (results.Count > 0? results[0]: null)
                : results[indexInArray];

            // Debug.Log(result);

            if (error == "" && !ReferenceEquals(targetProperty.objectReferenceValue, result))
            {
                // Debug.Log($"not equal: {targetProperty.objectReferenceValue}, {result}");
                if (getComponentByPathAttribute.ForceResign || forceResign)
                {
                    targetProperty.objectReferenceValue = result;
                    property.serializedObject.ApplyModifiedProperties();
                    onValueChangedCallback.Invoke(result);
                }

                if (getComponentByPathAttribute.ResignButton)
                {
                    button.style.display = ReferenceEquals(targetProperty.objectReferenceValue, result) ?DisplayStyle.None :DisplayStyle.Flex;
                }
            }


            // // ReSharper disable once InvertIf
            // if(changed)
            // {
            //     property.serializedObject.ApplyModifiedProperties();
            //     onValueChangedCallback.Invoke(result);
            // }
        }
        #endregion
#endif

        private static (string error, SerializedProperty targetProperty, Type fieldType, Type interfaceType) GetPropAndType(SerializedProperty property, FieldInfo info, object parent)
        {
            SerializedProperty targetProperty = property;
            Type fieldType = ReflectUtils.GetElementType(info.FieldType);
            Type interfaceType = null;
            if (property.propertyType == SerializedPropertyType.Generic)
            {
                (string error, int _, object propertyValue) = Util.GetValue(property, info, parent);

                if (error == "" && propertyValue is IWrapProp wrapProp)
                {
                    Util.SaintsInterfaceInfo saintsInterfaceInfo = Util.GetSaintsInterfaceInfo(property, wrapProp);

                    if(saintsInterfaceInfo.Error != "")
                    {
                        return (saintsInterfaceInfo.Error, targetProperty, fieldType, null);
                    }

                    fieldType = saintsInterfaceInfo.FieldType;
                    targetProperty = saintsInterfaceInfo.TargetProperty;
                    interfaceType = saintsInterfaceInfo.InterfaceType;

                    if (interfaceType != null && fieldType != typeof(Component) && !fieldType.IsSubclassOf(typeof(Component)) && typeof(Component).IsSubclassOf(fieldType))
                    {
                        fieldType = typeof(Component);
                    }
                }
            }

            if (targetProperty.propertyType != SerializedPropertyType.ObjectReference)
            {
                return ($"{targetProperty.propertyType} type is not supported by GetComponentByPath", targetProperty, fieldType, interfaceType);
            }


            return ("", targetProperty, fieldType, interfaceType);
        }

        private static (string error, SerializedProperty targetProperty, IReadOnlyList<Object> results) DoCheckComponent(SerializedProperty property, GetComponentByPathAttribute getComponentByPathAttribute, FieldInfo info, object parent)
        {
            (string error, SerializedProperty targetProperty, Type fieldType, Type interfaceType) = GetPropAndType(property, info, parent);

            if (error != "")
            {
                return (error, targetProperty, Array.Empty<Object>());
            }

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
                {
                    if(getComponentByPathAttribute.ForceResign && property.objectReferenceValue != null)
                    {
                        // changed = true;
                        targetProperty.objectReferenceValue = null;
                        property.serializedObject.ApplyModifiedProperties();
                    }
                }
                    return ("GetComponentByPath can only be used on Component or GameObject", targetProperty, Array.Empty<Object>());
            }

            IReadOnlyList<IReadOnlyList<GetComponentByPathAttribute.Token>> tokensPaths = getComponentByPathAttribute.Paths;
            Object[] results = tokensPaths
                .SelectMany(tokens => FindObjectByPath(tokens, fieldType, interfaceType, transform))
                .Where(each => each != null)
                .ToArray();

            // ReSharper disable once InvertIf
            if (results.Length == 0)
            {
                // if(getComponentByPathAttribute.ForceResign && property.objectReferenceValue != null)
                // {
                //     // changed = true;
                //     property.objectReferenceValue = null;
                // }

                string pathList = getComponentByPathAttribute.RawPaths.Count <= 1
                    ? getComponentByPathAttribute.RawPaths[0]
                    : string.Join("", getComponentByPathAttribute.RawPaths.Select(each => "\n* " + each));
                return ($"No component found in path: {pathList}", targetProperty, Array.Empty<Object>());
            }

            int indexInArray = SerializedUtils.PropertyPathIndex(property.propertyPath);
            if (indexInArray == 0)
            {
                SerializedProperty arrayProp = SerializedUtils.GetArrayProperty(property).property;
                if (arrayProp.arraySize != results.Length)
                {
                    arrayProp.arraySize = results.Length;
                    property.serializedObject.ApplyModifiedProperties();
                }
            }

            // if (!ReferenceEquals(property.objectReferenceValue, result))
            // {
            //     property.objectReferenceValue = result;
            //     changed = true;
            // }

            return ("", targetProperty, results);
        }

        public static int HelperGetArraySize(SerializedProperty property, GetComponentByPathAttribute getComponentByPathAttribute, FieldInfo info)
        {
            if (EditorApplication.isPlaying)
            {
                return -1;
            }

            Type fieldType = info.FieldType.IsGenericType? info.FieldType.GetGenericArguments()[0]: info.FieldType.GetElementType();
            if (fieldType == null)
            {
                return -1;
            }

            Type interfaceType = null;

            if (typeof(IWrapProp).IsAssignableFrom(fieldType))
            {
                Type mostBaseType = ReflectUtils.GetMostBaseType(fieldType);
                if (mostBaseType.IsGenericType && mostBaseType.GetGenericTypeDefinition() == typeof(SaintsInterface<,>))
                {
                    IReadOnlyList<Type> genericArguments = mostBaseType.GetGenericArguments();
                    if (genericArguments.Count == 2)
                    {
                        fieldType = genericArguments[0];
                        interfaceType = genericArguments[1];
                    }
                }

                if (interfaceType != null && fieldType != typeof(Component) && !fieldType.IsSubclassOf(typeof(Component)) && typeof(Component).IsSubclassOf(fieldType))
                {
                    fieldType = typeof(Component);
                }
            }

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
                    return -1;
            }

            IReadOnlyList<IReadOnlyList<GetComponentByPathAttribute.Token>> tokensPaths = getComponentByPathAttribute.Paths;
            bool found = tokensPaths
                .SelectMany(tokens => FindObjectByPath(tokens, fieldType, interfaceType, transform))
                .Any(each => each != null);

            return found ? 1 : 0;
        }

        private static IEnumerable<Object> FindObjectByPath(IEnumerable<GetComponentByPathAttribute.Token> tokens, Type type, Type interfaceType, Transform current)
        {
            bool isGameObject = type == typeof(GameObject);
            return IteratePath(new Queue<GetComponentByPathAttribute.Token>(tokens), new[] { current })
                .SelectMany(each =>
                {
                    if (isGameObject)
                    {
                        return new[]{(Object)each.gameObject};
                    }

                    if (interfaceType == null)
                    {
                        return each.GetComponents(type);
                    }
                    return each.GetComponents(type).Where(interfaceType.IsInstanceOfType).ToArray();
                })
                .Where(each => each != null);
        }

        private static IReadOnlyList<Transform> IteratePath(Queue<GetComponentByPathAttribute.Token> tokens, IReadOnlyList<Transform> currents)
        {
            if (tokens.Count == 0)
            {
                return currents;
            }

            GetComponentByPathAttribute.Token token = tokens.Dequeue();

            switch (token.Locate)
            {
                case GetComponentByPathAttribute.Locate.Root:
                {
                    // TODO: support root for prefab
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DROW_PROCESS_GET_COMPONENT_BY_PATH
                    Debug.Log($"#GetComponentByPath# <Root>::{token.Node}::{token.Index} {string.Join(", ", SceneManager.GetActiveScene().GetRootGameObjects().Select(each => each.name))}");
#endif
                    return IteratePath(
                        tokens,
                        IndexFilter(
                            SceneManager
                                .GetActiveScene()
                                .GetRootGameObjects()
                                .Select(each => each.transform)
                                .Where(each => token.Node == "*" || each.gameObject.name == token.Node)
                                .ToArray(),
                            token.Index));
                }
                case GetComponentByPathAttribute.Locate.Descendant:
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DROW_PROCESS_GET_COMPONENT_BY_PATH
                    Debug.Log($"#GetComponentByPath# <Descendant::{token.Node}> {string.Join(", ", currents.SelectMany(GetAllDescendant).Where(each => each.gameObject.name == token.Node))}");
#endif
                    return IteratePath(
                        tokens,
                        IndexFilter(currents
                            // .SelectMany(each => each.GetComponentsInChildren<Transform>())
                            .SelectMany(GetAllDescendant)
                            .Where(each => each.gameObject.name == token.Node)
                            .ToArray(), token.Index));
                }
                case GetComponentByPathAttribute.Locate.Child:
                {
                    // ReSharper disable once ConvertSwitchStatementToSwitchExpression
                    switch (token.Node)
                    {
                        case ".":
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DROW_PROCESS_GET_COMPONENT_BY_PATH
                            Debug.Log($"#GetComponentByPath# Child::<CUR> {string.Join(", ", currents)}");
#endif
                            // ReSharper disable once TailRecursiveCall
                            return IteratePath(
                                tokens,
                                currents);
                        case "..":
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DROW_PROCESS_GET_COMPONENT_BY_PATH
                            Debug.Log($"#GetComponentByPath# Child::<PARENT> {string.Join(", ", currents.Select(each => each.parent).Where(each => each != null))}");
#endif
                            return IteratePath(
                                tokens,
                                IndexFilter(currents
                                    .Select(each => each.parent)
                                    .Where(each => each != null)
                                    .ToArray(), token.Index));
                        case "*":
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DROW_PROCESS_GET_COMPONENT_BY_PATH
                            Debug.Log($"#GetComponentByPath# Child::<ALL_CHILDREN> {string.Join(", ", currents.Select(each => each.parent).Where(each => each != null))}");
#endif
                            return IteratePath(
                                tokens,
                                IndexFilter(currents
                                    .SelectMany(each => each.Cast<Transform>())
                                    .ToArray(), token.Index));
                        default:
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DROW_PROCESS_GET_COMPONENT_BY_PATH
                            Debug.Log($"#GetComponentByPath# Child::{token.Node} in {string.Join(", ", currents.SelectMany(each => each.Cast<Transform>()).Select(each => each.gameObject.name))}");
#endif
                            return IteratePath(
                                tokens,
                                IndexFilter(currents
                                    .SelectMany(each => each.Cast<Transform>())
                                    .Where(each => each.gameObject.name == token.Node)
                                    .ToArray(), token.Index));
                    }
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(token.Locate), token.Locate, null);
            }
        }

        private static readonly Regex CompareRegex = new Regex(@"([^\d^\s^-]+)\s*([-\d])+");

        private static IReadOnlyList<Transform> IndexFilter(IReadOnlyList<Transform> targets, string indexFilter)
        {
            if (targets.Count == 0  || indexFilter ==  "")
            {
                return targets;
            }

            if (indexFilter == "last()")
            {
                return new[] { targets.Last() };
            }

            if (indexFilter.StartsWith("index()"))
            {
                string rawControl = indexFilter.Replace("index()", "").Trim();
                // if (controls.Length != 2)
                // {
                //     throw new ArgumentException($"Invalid position() index filter: {indexFilter}");
                // }

                Match match = CompareRegex.Match(rawControl);
                if (!match.Success)
                {
                    throw new ArgumentException($"Invalid index() filter: {indexFilter}");
                }

                string compare = match.Groups[1].Value;
                int position = int.Parse(match.Groups[2].Value);

                foreach ((Transform value, int index) in targets.WithIndex())
                {
                    switch (compare)
                    {
                        case "=":
                        {
                            if(index == position)
                            {
                                return new[] { value };
                            }
                        }
                            break;
                        case ">":
                        {
                            if(index > position)
                            {
                                return new[] { value };
                            }
                        }
                            break;
                        case ">=":
                        {
                            if (index >= position)
                            {
                                return new[] { value };
                            }
                        }
                            break;
                        case "<":
                        {
                            if (index < position)
                            {
                                return new[] { value };
                            }
                        }
                            break;
                        case "<=":
                        {
                            if (index <= position)
                            {
                                return new[] { value };
                            }
                        }
                            break;
                        case "!=":
                        {
                            if (index != position)
                            {
                                return new[] { value };
                            }
                        }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(compare), compare, null);
                    }
                }
                return Array.Empty<Transform>();
            }

            // number
            int fixedIndex = int.Parse(indexFilter);
            return new[] { targets[fixedIndex] };
        }

        private static IEnumerable<Transform> GetAllDescendant(Transform root)
        {
            foreach (Transform directChild in root.Cast<Transform>())
            {
                yield return directChild;

                foreach (Transform directChildDescendant in GetAllDescendant(directChild))
                {
                    yield return directChildDescendant;
                }
            }
        }
    }
}

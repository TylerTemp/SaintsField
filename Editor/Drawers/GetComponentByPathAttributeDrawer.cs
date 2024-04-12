using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Reflection;
using System.Text.RegularExpressions;
using SaintsField.Editor.Linq;
using Object = UnityEngine.Object;
#if UNITY_2021_3_OR_NEWER
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(GetComponentByPathAttribute))]
    public class GetComponentByPathAttributeDrawer: SaintsPropertyDrawer
    {
        #region IMGUI
        private Texture2D _refreshIcon;
        private string _error = "";

        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            GetComponentByPathAttribute getComponentByPathAttribute = (GetComponentByPathAttribute)saintsAttribute;
            if (!getComponentByPathAttribute.ResignButton)
            {
                return 0;
            }

            (string error, Object result) = DoCheckComponent(property, getComponentByPathAttribute, info);
            _error = error;
            if (error != "")
            {
                return 0;
            }

            return ReferenceEquals(property.objectReferenceValue, result) ? 0 : SingleLineHeight;
        }

        // private bool _firstOpen = true;
        private readonly Dictionary<int, bool> _drawerNotFirstOpenCache = new Dictionary<int, bool>();

        protected override bool DrawPostFieldImGui(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute,
            int index,
            OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            // Debug.Log($"init first open {_firstOpen}");
            _error = "";
            GetComponentByPathAttribute getComponentByPathAttribute = (GetComponentByPathAttribute)saintsAttribute;

            // match?
            (string error, Object result) = DoCheckComponent(property, getComponentByPathAttribute, info);
            _error = error;

            if (error != "")
            {
                return false;
            }

            _drawerNotFirstOpenCache.TryGetValue(index, out bool notFirstOpen);
            bool firstOpen = !notFirstOpen;

            // Debug.Log($"fr={getComponentByPathAttribute.ForceResign}, equal={ReferenceEquals(property.objectReferenceValue, result)}");
            if(((firstOpen && property.objectReferenceValue == null) || getComponentByPathAttribute.ForceResign) && !ReferenceEquals(property.objectReferenceValue, result))
            {
                // Debug.Log($"firstOpen={_firstOpen}; auto sign to {result}");
                property.objectReferenceValue = result;
                // property.serializedObject.ApplyModifiedProperties();
                // valueChanged = true;
            }

            bool willDraw = false;
            if(!ReferenceEquals(property.objectReferenceValue, result) && getComponentByPathAttribute.ResignButton)
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

            _drawerNotFirstOpenCache[index] = true;

            return willDraw;
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            FieldInfo info, object parent) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute,
            FieldInfo info, object parent)
        {
            if (_error == "")
            {
                return 0;
            }

            return ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute,
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
            if (property.objectReferenceValue == null)
            {
                Check(property, getComponentByPathAttribute, info, button, helpBox, onValueChangedCallback, true);
            }

            if(button != null)
            {
                button.clicked += () => Check(property, (GetComponentByPathAttribute)saintsAttribute, info, button, helpBox,
                    onValueChangedCallback, true);
            }
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            GetComponentByPathAttribute getComponentByPathAttribute = (GetComponentByPathAttribute)saintsAttribute;
            // ReSharper disable once InvertIf
            if(getComponentByPathAttribute.ForceResign || getComponentByPathAttribute.ResignButton)
            {
                Button button = getComponentByPathAttribute.ResignButton? container.Q<Button>(NameResignButton(property, index)): null;
                HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property, index));
                Check(property, getComponentByPathAttribute, info, button, helpBox, onValueChangedCallback, false);
            }
        }

        private static void Check(SerializedProperty property, GetComponentByPathAttribute getComponentByPathAttribute,
            FieldInfo info,
            // ReSharper disable once SuggestBaseTypeForParameter
            Button button, HelpBox helpBox, Action<object> onValueChangedCallback, bool forceResign)
        {
            (string error, Object result) = DoCheckComponent(property, getComponentByPathAttribute, info);
            // HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property, index));
            if (error != helpBox.text)
            {
                helpBox.style.display = error == "" ? DisplayStyle.None : DisplayStyle.Flex;
                helpBox.text = error;

                if (getComponentByPathAttribute.ForceResign && property.objectReferenceValue != null)
                {
                    property.objectReferenceValue = null;
                    property.serializedObject.ApplyModifiedProperties();
                    onValueChangedCallback.Invoke(null);
                }

                if (getComponentByPathAttribute.ResignButton)
                {
                    button.style.display = DisplayStyle.None;
                }
                return;
            }

            if (!ReferenceEquals(property.objectReferenceValue, result))
            {
                if (getComponentByPathAttribute.ForceResign || forceResign)
                {
                    property.objectReferenceValue = result;
                    property.serializedObject.ApplyModifiedProperties();
                    onValueChangedCallback.Invoke(result);
                }

                if (getComponentByPathAttribute.ResignButton)
                {
                    button.style.display = ReferenceEquals(property.objectReferenceValue, result) ?DisplayStyle.None :DisplayStyle.Flex;
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

        private static (string error, Object result) DoCheckComponent(SerializedProperty property, GetComponentByPathAttribute getComponentByPathAttribute, FieldInfo info)
        {
            Type fieldType = info.FieldType;
            // Type type = getComponentByPathAttribute.CompType ?? fieldType;

            // bool changed = false;

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
                        property.objectReferenceValue = null;
                    }
                }
                    return ("GetComponentInChildrenAttribute can only be used on Component or GameObject", null);
            }

            IReadOnlyList<IReadOnlyList<GetComponentByPathAttribute.Token>> tokensPaths = getComponentByPathAttribute.Paths;
            Object result = tokensPaths
                .Select(tokens => FindObjectByPath(tokens, fieldType, transform))
                .FirstOrDefault(each => each != null);

            // ReSharper disable once InvertIf
            if (result == null)
            {
                // if(getComponentByPathAttribute.ForceResign && property.objectReferenceValue != null)
                // {
                //     // changed = true;
                //     property.objectReferenceValue = null;
                // }

                string pathList = getComponentByPathAttribute.RawPaths.Count <= 1
                    ? getComponentByPathAttribute.RawPaths[0]
                    : string.Join("", getComponentByPathAttribute.RawPaths.Select(each => "\n* " + each));
                return ($"No component found in path: {pathList}", null);
            }

            // if (!ReferenceEquals(property.objectReferenceValue, result))
            // {
            //     property.objectReferenceValue = result;
            //     changed = true;
            // }

            return ("", result);
        }

        private static Object FindObjectByPath(IEnumerable<GetComponentByPathAttribute.Token> tokens, Type type, Transform current)
        {
            bool isGameObject = type == typeof(GameObject);
            return IteratePath(new Queue<GetComponentByPathAttribute.Token>(tokens), new[] { current })
                .Select(each => isGameObject ? (Object)each.gameObject : each.GetComponent(type))
                .FirstOrDefault(each => each != null);
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

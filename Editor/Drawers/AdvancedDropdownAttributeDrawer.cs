using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.DropdownBase;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Analytics;
using UnityAdvancedDropdown = UnityEditor.IMGUI.Controls.AdvancedDropdown;
using UnityAdvancedDropdownItem = UnityEditor.IMGUI.Controls.AdvancedDropdownItem;
#if UNITY_2021_3_OR_NEWER
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Drawers
{
    #region IMGUI Pop

    public class SaintsAdvancedDropdown : UnityAdvancedDropdown
    {

        private readonly IAdvancedDropdownList _dropdownListValue;

        private readonly Dictionary<UnityAdvancedDropdownItem, object> _itemToValue = new Dictionary<UnityAdvancedDropdownItem, object>();
        private readonly Action<object> _setValueCallback;
        private readonly Func<string, Texture2D> _getIconCallback;
        private readonly Rect _showRect;

        public SaintsAdvancedDropdown(IAdvancedDropdownList dropdownListValue, Vector2 size, Rect showRect, AdvancedDropdownState state, Action<object> setValueCallback, Func<string, Texture2D> getIconCallback) : base(state)
        {
            _dropdownListValue = dropdownListValue;
            _setValueCallback = setValueCallback;
            _getIconCallback = getIconCallback;
            _showRect = showRect;

            minimumSize = size;
        }

        protected override UnityAdvancedDropdownItem BuildRoot()
        {
            AdvancedDropdownItem root = MakeUnityAdvancedDropdownItem(_dropdownListValue);

            if(_dropdownListValue.children.Count == 0)
            {
                // root.AddChild(new UnityAdvancedDropdownItem("Empty"));
                return root;
            }

            MakeChildren(root, _dropdownListValue.children);

            return root;
        }

        private UnityAdvancedDropdownItem MakeUnityAdvancedDropdownItem(IAdvancedDropdownList item)
        {
            // if (item.isSeparator)
            // {
            //     return new UnityAdvancedDropdownItem("SEPARATOR");
            // }

            return new UnityAdvancedDropdownItem(item.displayName)
            {
                icon = string.IsNullOrEmpty(item.icon) ? null : _getIconCallback(item.icon),
                enabled = !item.disabled,
            };
        }

        private void MakeChildren(AdvancedDropdownItem parent, IEnumerable<IAdvancedDropdownList> children)
        {
            foreach (IAdvancedDropdownList childItem in children)
            {
                if (childItem.isSeparator)
                {
                    parent.AddSeparator();
                }
                else if (childItem.children.Count == 0)
                {
                    // Debug.Log($"{parent.name}/{childItem.displayName}");
                    AdvancedDropdownItem item = MakeUnityAdvancedDropdownItem(childItem);
                    _itemToValue[item] = childItem.value;
                    // Debug.Log($"add {childItem.displayName} => {childItem.value}");
                    parent.AddChild(item);
                }
                else
                {
                    AdvancedDropdownItem subParent = MakeUnityAdvancedDropdownItem(childItem);
                    // Debug.Log($"{parent.name}/{childItem.displayName}[...]");
                    MakeChildren(subParent, childItem.children);
                    parent.AddChild(subParent);
                }
            }
        }

        protected override void ItemSelected(UnityAdvancedDropdownItem item)
        {
            if (!item.enabled)  // WTF Unity?
            {
                // Show(new Rect(_showRect)
                // {
                //     y = 0,
                //     height = 0,
                // });
                // Show(new Rect(_showRect)
                // {
                //     x = 0,
                //     y = -_showRect.y - _showRect.height,
                //     height = 0,
                // });

                // ReSharper disable once InvertIf
                if(_bindWindowPos)
                {
                    Show(_showRect);
                    EditorWindow curFocusedWindow = EditorWindow.focusedWindow;
                    if (curFocusedWindow == null || curFocusedWindow.GetType().ToString() !=
                        "UnityEditor.IMGUI.Controls.AdvancedDropdownWindow")
                    {
                        return;
                    }
                    curFocusedWindow.position = _windowPosition;
                }

                return;
            }

            // Debug.Log($"select {item.name}: {(_itemToValue.TryGetValue(item, out object r) ? r.ToString() : "[NULL]")}");
            if (_itemToValue.TryGetValue(item, out object result))
            {
                _setValueCallback(result);
            }
        }

        private bool _bindWindowPos;
        // private EditorWindow _thisEditorWindow;
        private Rect _windowPosition;

        // hack for Unity allow to click on disabled item...
        public void BindWindowPosition()
        {
            if (_bindWindowPos)
            {
                return;
            }

            EditorWindow window = EditorWindow.focusedWindow;
            if (window == null || window.GetType().ToString() != "UnityEditor.IMGUI.Controls.AdvancedDropdownWindow")
            {
                return;
            }

            _bindWindowPos = true;
            _windowPosition = window.position;
        }
    }

    #endregion

    #region UIToolkit Pop
    public class SaintsAdvancedDropdownUiToolkit : PopupWindowContent
    {
        private readonly float _width;
        private readonly AdvancedDropdownAttributeDrawer.MetaInfo _metaInfo;
        private readonly Action<object> _setValue;

        public SaintsAdvancedDropdownUiToolkit(AdvancedDropdownAttributeDrawer.MetaInfo metaInfo, float width, Action<object> setValue)
        {
            _width = width;
            _metaInfo = metaInfo;
            _setValue = setValue;
        }

        public override void OnGUI(Rect rect)
        {
            // Intentionally left empty
        }

        //Set the window size
        public override Vector2 GetWindowSize()
        {
            return new Vector2(_width, 200);
        }

        public override void OnOpen()
        {
            VisualElement element = CloneTree(_metaInfo, _setValue);
            // VisualElement root = editorWindow.rootVisualElement;
            editorWindow.rootVisualElement.Add(element);
        }

        public static VisualElement CloneTree(AdvancedDropdownAttributeDrawer.MetaInfo metaInfo, Action<object> setValue)
        {
            StyleSheet ussStyle = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/SaintsField/Editor/Editor Default Resources/SaintsField/UIToolkit/SaintsAdvancedDropdown/Style.uss");

            VisualTreeAsset popUpAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/SaintsField/Editor/Editor Default Resources/SaintsField/UIToolkit/SaintsAdvancedDropdown/Popup.uxml");
            VisualElement root = popUpAsset.CloneTree();

            root.styleSheets.Add(ussStyle);

            // root.Q<TemplateContainer>("itemRow").RemoveFromHierarchy();

            ToolbarBreadcrumbs toolbarBreadcrumbs = root.Q<ToolbarBreadcrumbs>();
            toolbarBreadcrumbs.PushItem("myItemGrandParent", () => Debug.Log("1"));
            toolbarBreadcrumbs.PushItem("myItemParent", () => Debug.Log("2"));
            toolbarBreadcrumbs.PushItem("myItem", () => Debug.Log("3"));
            toolbarBreadcrumbs.PushItem("myItem", () => Debug.Log("3"));
            toolbarBreadcrumbs.PushItem("myItem", () => Debug.Log("3"));

            VisualTreeAsset separatorAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/SaintsField/Editor/Editor Default Resources/SaintsField/UIToolkit/SaintsAdvancedDropdown/Separator.uxml");

            VisualTreeAsset itemAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/SaintsField/Editor/Editor Default Resources/SaintsField/UIToolkit/SaintsAdvancedDropdown/ItemRow.uxml");

            VisualElement scrollViewContainer = root.Q<VisualElement>("saintsfield-advanced-dropdown-scollview-container");
            // ScrollView scrollView = root.Q<ScrollView>();

            // Texture2D icon = RichTextDrawer.LoadTexture("eye.png");
            Texture2D next = RichTextDrawer.LoadTexture("arrow-next.png");
            Texture2D check = RichTextDrawer.LoadTexture("check.png");

            IReadOnlyList<AdvancedDropdownAttributeDrawer.SelectStack> selectStack = metaInfo.SelectStacks;
            IReadOnlyList<IAdvancedDropdownList> displayPage = metaInfo.DropdownListValue.children;
            if (selectStack.Count > 0)
            {
                Debug.Log($"selectStack={string.Join("->", selectStack.Select(each => $"{each.Display}/{each.Index}"))}");
                displayPage = GetPage(metaInfo.DropdownListValue, selectStack);
            }

            SwapPage(scrollViewContainer, displayPage, selectStack, separatorAsset, itemAsset, next, check, setValue);
            return root;
        }

        private static void SwapPage(VisualElement scrollViewContainer,
            IReadOnlyList<IAdvancedDropdownList> displayPage,
            IReadOnlyList<AdvancedDropdownAttributeDrawer.SelectStack> selectStack,
            VisualTreeAsset separatorAsset,
            VisualTreeAsset itemAsset,
            Texture2D next,
            Texture2D check, Action<object> setValue)
        {
            ScrollView scrollView = scrollViewContainer.Q<ScrollView>();

            VisualElement fadeOutOriChildren = new VisualElement();
            foreach (VisualElement scrollViewChildren in scrollView.Children().ToArray())
            {
                fadeOutOriChildren.Add(scrollViewChildren);
            }

            fadeOutOriChildren.AddToClassList("saintsfield-advanced-dropdown-fade-out-container");
            fadeOutOriChildren.AddToClassList("saintsfield-advanced-dropdown-anim");
            fadeOutOriChildren.RegisterCallback<GeometryChangedEvent>(GeoAnimOutLeftDestroy);
            scrollViewContainer.Add(fadeOutOriChildren);

            scrollView.Clear();

            VisualElement scrollContent = new VisualElement();
            scrollContent.AddToClassList("saintsfield-advanced-dropdown-anim");
            scrollContent.AddToClassList("saintsfield-advanced-dropdown-anim-right");

            foreach ((IAdvancedDropdownList dropdownItem, int index) in displayPage.WithIndex())
            {
                if (dropdownItem.isSeparator)
                {
                    VisualElement separator = separatorAsset.CloneTree();
                    scrollView.Add(separator);
                    continue;
                }

                VisualElement elementItem = itemAsset.CloneTree();

                Button itemContainer =
                    elementItem.Q<Button>(className: "saintsfield-advanced-dropdown-item");

                Image selectImage = itemContainer.Q<Image>("item-checked-image");
                selectImage.image = check;

                itemContainer.Q<Label>("item-content").text = dropdownItem.displayName;

                if(!string.IsNullOrEmpty(dropdownItem.icon))
                {
                    itemContainer.Q<Image>("item-icon-image").image = RichTextDrawer.LoadTexture(dropdownItem.icon);
                }

                if(dropdownItem.children.Count > 0)
                {
                    itemContainer.Q<Image>("item-next-image").image = next;
                    itemContainer.clicked += () => SwapPage(scrollViewContainer, dropdownItem.children, selectStack, separatorAsset, itemAsset, next, check, setValue);
                }
                else
                {
                    itemContainer.clicked += () => setValue(dropdownItem.value);

                    bool isSelected = selectStack.Count > 0 && selectStack[selectStack.Count - 1].Index == index;
                    Debug.Log($"isSelected={isSelected}, {index}");
                    if (isSelected)
                    {
                        selectImage.visible = true;
                        itemContainer.AddToClassList("saintsfield-advanced-dropdown-item-selected");
                    }

                    if (dropdownItem.disabled)
                    {
                        itemContainer.SetEnabled(false);
                        itemContainer.AddToClassList("saintsfield-advanced-dropdown-item-disabled");
                        itemContainer.RemoveFromClassList("saintsfield-advanced-dropdown-item-active");
                    }
                }

                // itemContainer.RegisterCallback<GeometryChangedEvent>(GeoAnimIntoView);

                scrollContent.Add(elementItem);
            }

            scrollContent.RegisterCallback<GeometryChangedEvent>(GeoAnimIntoView);;

            scrollView.Add(scrollContent);
        }

        private static void GeoAnimIntoView(GeometryChangedEvent evt)
        {
            VisualElement targetItem = (VisualElement)evt.target;
            targetItem.UnregisterCallback<GeometryChangedEvent>(GeoAnimIntoView);
            // VisualElement targetItem = targetRoot.Q<VisualElement>(className: "saintsfield-advanced-dropdown-item");
            // Debug.Log($"attached: {targetRoot}");
            targetItem.RemoveFromClassList("saintsfield-advanced-dropdown-anim-right");
            targetItem.RemoveFromClassList("saintsfield-advanced-dropdown-anim-left");
            targetItem.AddToClassList("saintsfield-advanced-dropdown-anim-in-view");
        }

        private static void GeoAnimOutLeftDestroy(GeometryChangedEvent evt)
        {
            GeoAnimOutDestroy(evt, "saintsfield-advanced-dropdown-anim-left");
        }

        private static void GeoAnimOutRightDestroy(GeometryChangedEvent evt)
        {
            GeoAnimOutDestroy(evt, "saintsfield-advanced-dropdown-anim-right");
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        private static void GeoAnimOutDestroy(GeometryChangedEvent evt, string className)
        {
            VisualElement targetItem = (VisualElement)evt.target;
            targetItem.UnregisterCallback<GeometryChangedEvent>(GeoAnimOutLeftDestroy);
            targetItem.UnregisterCallback<GeometryChangedEvent>(GeoAnimOutRightDestroy);

            targetItem.RemoveFromClassList("saintsfield-advanced-dropdown-anim-in-view");
            targetItem.AddToClassList(className);
            targetItem.RegisterCallback<TransitionEndEvent>(TransitionDestroy);
        }

        private static void TransitionDestroy(TransitionEndEvent evt) =>
            ((VisualElement)evt.target).RemoveFromHierarchy();

        private static IReadOnlyList<IAdvancedDropdownList> GetPage(IAdvancedDropdownList dropdownList, IReadOnlyList<AdvancedDropdownAttributeDrawer.SelectStack> selectStack)
        {
            if (selectStack.Count <= 1)
            {
                return dropdownList.children;
            }

            int index = selectStack[0].Index;
            // ReSharper disable once TailRecursiveCall
            return GetPage(dropdownList.children[index], selectStack.Skip(1).ToArray());
        }

        public override void OnClose()
        {
            Debug.Log("Popup closed: " + this);
        }
    }
    #endregion

    [CustomPropertyDrawer(typeof(AdvancedDropdownAttribute))]
    public class AdvancedDropdownAttributeDrawer: SaintsPropertyDrawer
    {
        private string _error = "";

        private readonly Dictionary<string, Texture2D> _iconCache = new Dictionary<string, Texture2D>();

        ~AdvancedDropdownAttributeDrawer()
        {
            foreach (Texture2D iconCacheValue in _iconCache.Values)
            {
                UnityEngine.Object.DestroyImmediate(iconCacheValue);
            }
        }

        #region Util

        public struct SelectStack
        {
            // ReSharper disable InconsistentNaming
            public int Index;
            public string Display;
            // public object Value;
            // ReSharper enable InconsistentNaming
        }

        public struct MetaInfo
        {
            // ReSharper disable InconsistentNaming
            public string Error;

            public FieldInfo FieldInfo;

            public string CurDisplay;
            public object CurValue;
            public IAdvancedDropdownList DropdownListValue;
            public IReadOnlyList<SelectStack> SelectStacks;
            // ReSharper enable InconsistentNaming
        }

        private static MetaInfo GetMetaInfo(SerializedProperty property, AdvancedDropdownAttribute advancedDropdownAttribute, object parentObj)
        {
            string funcName = advancedDropdownAttribute.FuncName;
            Debug.Assert(parentObj != null);
            Type parentType = parentObj.GetType();
            (ReflectUtils.GetPropType getPropType, object fieldOrMethodInfo) =
                ReflectUtils.GetProp(parentType, funcName);

            #region Get List Items
            IAdvancedDropdownList dropdownListValue;

            switch (getPropType)
            {
                case ReflectUtils.GetPropType.NotFound:
                    return new MetaInfo
                    {
                        Error = $"not found `{funcName}` on target `{parentObj}`",
                    };
                case ReflectUtils.GetPropType.Property:
                {
                    PropertyInfo foundPropertyInfo = (PropertyInfo)fieldOrMethodInfo;
                    dropdownListValue = foundPropertyInfo.GetValue(parentObj) as IAdvancedDropdownList;
                    if (dropdownListValue == null)
                    {
                        return new MetaInfo
                        {
                            Error = $"dropdownListValue is null from `{funcName}` on target `{parentObj}`",
                        };
                    }
                }
                    break;
                case ReflectUtils.GetPropType.Field:
                {
                    FieldInfo foundFieldInfo = (FieldInfo)fieldOrMethodInfo;
                    dropdownListValue = foundFieldInfo.GetValue(parentObj) as IAdvancedDropdownList;
                    if (dropdownListValue == null)
                    {
                        return new MetaInfo
                        {
                            Error = $"dropdownListValue is null from `{funcName}` on target `{parentObj}`",
                        };
                    }
                }
                    break;
                case ReflectUtils.GetPropType.Method:
                {
                    MethodInfo methodInfo = (MethodInfo)fieldOrMethodInfo;
                    ParameterInfo[] methodParams = methodInfo.GetParameters();
                    Debug.Assert(methodParams.All(p => p.IsOptional));

                    try
                    {
                        dropdownListValue =
                            methodInfo.Invoke(parentObj, methodParams.Select(p => p.DefaultValue).ToArray()) as IAdvancedDropdownList;
                    }
                    catch (TargetInvocationException e)
                    {
                        Debug.LogException(e);
                        Debug.Assert(e.InnerException != null);
                        return new MetaInfo
                        {
                            Error = e.InnerException.Message,
                        };
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        return new MetaInfo
                        {
                            Error = e.Message,
                        };
                    }

                    if (dropdownListValue == null)
                    {
                        return new MetaInfo
                        {
                            Error = $"dropdownListValue is null from `{funcName}()` on target `{parentObj}`",
                        };
                    }
                }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(getPropType), getPropType, null);
            }

            #endregion

            #region Get Cur Value
            const BindingFlags bindAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
                                          BindingFlags.Public | BindingFlags.DeclaredOnly;
            // Object target = property.serializedObject.targetObject;
            FieldInfo field = parentType.GetField(property.name, bindAttr);
            Debug.Assert(field != null, $"{property.name}/{parentObj}");
            object curValue = field.GetValue(parentObj);
            // Debug.Log($"get cur value {curValue}, {parentObj}->{field}");
            // string curDisplay = "";
            IReadOnlyList<SelectStack> curSelected = GetSelected(curValue, Array.Empty<SelectStack>(), dropdownListValue);
            #endregion

            return new MetaInfo
            {
                Error = "",
                FieldInfo = field,
                // CurDisplay = curDisplay,
                CurValue = curValue,
                DropdownListValue = dropdownListValue,
                SelectStacks = curSelected,
            };
        }

        private static IReadOnlyList<SelectStack> GetSelected(object curValue, IReadOnlyList<SelectStack> curStacks, IReadOnlyList<IAdvancedDropdownList> dropdownPage)
        {
            foreach ((IAdvancedDropdownList item, int index) in dropdownPage.WithIndex())
            {
                if (item.isSeparator)
                {
                    continue;
                }

                if (item.children.Count > 0)  // it's a group
                {
                    Debug.Log($"GetSelected group {item.displayName}");
                    var subResult = GetSelected(curValue, curStacks.Append(new SelectStack
                    {
                        Display = item.displayName,
                        Index = index,
                    }).ToArray(), item.children);
                    if (subResult.Count > 0)
                    {
                        return subResult;
                    }

                    continue;
                }

                IEnumerable<SelectStack> thisLoopResult = curStacks.Append(new SelectStack
                {
                    Display = item.displayName,
                    Index = index,
                });

                // ReSharper disable once ConvertIfStatementToSwitchStatement
                if (curValue == null && item.value == null)
                {
                    Debug.Log($"GetSelected null {item.displayName}/{index}");
                    return thisLoopResult.ToArray();
                }
                if (curValue is UnityEngine.Object curValueObj
                    && curValueObj == item.value as UnityEngine.Object)
                {
                    Debug.Log($"GetSelected {curValue} {item.displayName}/{index}");
                    return thisLoopResult.ToArray();
                }
                if (item.value == null)
                {
                    Debug.Log($"GetSelected nothing null {item.displayName}/{index}");
                    // nothing
                }
                else if (item.value.Equals(curValue))
                {
                    Debug.Log($"GetSelected {curValue} {item.displayName}/{index}");
                    return thisLoopResult.ToArray();
                }
            }

            Debug.Log($"GetSelected end in empty");
            // nothing selected
            return Array.Empty<SelectStack>();
        }

        #endregion

        #region IMGUI

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute,
            bool hasLabelWidth)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        private static IEnumerable<(string, object)> FlattenChild(string prefix, IEnumerable<IAdvancedDropdownList> children)
        {
            foreach (IAdvancedDropdownList child in children)
            {
                if (child.Count > 0)
                {
                    // List<(string, object, List<object>, bool, string, bool)> grandChildren = child.Item3.Cast<(string, object, List<object>, bool, string, bool)>().ToList();
                    foreach ((string, object) grandChild in FlattenChild(prefix, child.children))
                    {
                        yield return grandChild;
                    }
                }
                else
                {
                    yield return (Prefix(prefix, child.displayName), child.value);
                }
            }
        }

        private static IEnumerable<(string, object)> Flatten(string prefix, IAdvancedDropdownList roots)
        {
            foreach (IAdvancedDropdownList root in roots)
            {
                if (root.Count > 0)
                {
                    // IAdvancedDropdownList children = root.Item3.Cast<(string, object, List<object>, bool, string, bool)>().ToList();
                    foreach ((string, object) child in FlattenChild(Prefix(prefix, root.displayName), root.children))
                    {
                        yield return child;
                    }
                }
                else
                {
                    yield return (Prefix(prefix, root.displayName), root.value);
                }
            }

            // AdvancedDropdownItem<T> result = new AdvancedDropdownItem<T>(root.name, root.Value, root.Icon);
            // foreach (AdvancedDropdownItem<T> child in root.children)
            // {
            //     result.AddChild(flatten(child));
            // }
            //
            // return result;
        }

        private static string Prefix(string prefix, string value) => string.IsNullOrEmpty(prefix)? value : $"{prefix}/{value}";

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, object parent)
        {
            AdvancedDropdownAttribute advancedDropdownAttribute = (AdvancedDropdownAttribute) saintsAttribute;

            // SaintsAdvancedDropdown dropdown = new SaintsAdvancedDropdown(new AdvancedDropdownState());
            // dropdown.Show(position);

            string funcName = advancedDropdownAttribute.FuncName;
            object parentObj = GetParentTarget(property);
            Debug.Assert(parentObj != null);
            Type parentType = parentObj.GetType();
            (ReflectUtils.GetPropType getPropType, object fieldOrMethodInfo) =
                ReflectUtils.GetProp(parentType, funcName);

            #region Get List Items
            IAdvancedDropdownList dropdownListValue;

            switch (getPropType)
            {
                case ReflectUtils.GetPropType.NotFound:
                {
                    _error = $"not found `{funcName}` on target `{parentObj}`";
                    DefaultDrawer(position, property, label);
                }
                    return;
                case ReflectUtils.GetPropType.Property:
                {
                    PropertyInfo foundPropertyInfo = (PropertyInfo)fieldOrMethodInfo;
                    dropdownListValue = foundPropertyInfo.GetValue(parentObj) as IAdvancedDropdownList;
                    if (dropdownListValue == null)
                    {
                        _error = $"dropdownListValue is null from `{funcName}` on target `{parentObj}`";
                        DefaultDrawer(position, property, label);
                        return;
                    }
                }
                    break;
                case ReflectUtils.GetPropType.Field:
                {
                    FieldInfo foundFieldInfo = (FieldInfo)fieldOrMethodInfo;
                    dropdownListValue = foundFieldInfo.GetValue(parentObj) as IAdvancedDropdownList;
                    if (dropdownListValue == null)
                    {
                        _error = $"dropdownListValue is null from `{funcName}` on target `{parentObj}`";
                        DefaultDrawer(position, property, label);
                        return;
                    }
                }
                    break;
                case ReflectUtils.GetPropType.Method:
                {
                    MethodInfo methodInfo = (MethodInfo)fieldOrMethodInfo;
                    ParameterInfo[] methodParams = methodInfo.GetParameters();
                    Debug.Assert(methodParams.All(p => p.IsOptional));

                    _error = "";
                    // IEnumerable<AdvancedDropdownItem<object>> result;
                    try
                    {
                        dropdownListValue =
                            methodInfo.Invoke(parentObj, methodParams.Select(p => p.DefaultValue).ToArray()) as IAdvancedDropdownList;
                        // Debug.Log(rawResult);
                        // Debug.Log(rawResult as IDropdownList);
                        // // Debug.Log(rawResult.GetType());
                        // // Debug.Log(rawResult.GetType().Name);
                        // // Debug.Log(typeof(rawResult));
                        //

                        // Debug.Log($"result: {dropdownListValue}");
                    }
                    catch (TargetInvocationException e)
                    {
                        Debug.Assert(e.InnerException != null);
                        _error = e.InnerException.Message;
                        Debug.LogException(e);
                        return;
                    }
                    catch (Exception e)
                    {
                        _error = e.Message;
                        Debug.LogException(e);
                        return;
                    }

                    if (dropdownListValue == null)
                    {
                        _error = $"dropdownListValue is null from `{funcName}()` on target `{parentObj}`";
                        DefaultDrawer(position, property, label);
                        return;
                    }
                }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(getPropType), getPropType, null);
            }

            #endregion

            #region Get Cur Value
            const BindingFlags bindAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
                                          BindingFlags.Public | BindingFlags.DeclaredOnly;
            // Object target = property.serializedObject.targetObject;
            FieldInfo field = parentType.GetField(property.name, bindAttr);
            Debug.Assert(field != null, $"{property.name}/{parentObj}");
            object curValue = field.GetValue(parentObj);
            // Debug.Log($"get cur value {curValue}, {parentObj}->{field}");
            string curDisplay = "";
            Debug.Assert(dropdownListValue != null);
            foreach ((string, object) itemInfos in Flatten("", dropdownListValue))
            {
                string name = itemInfos.Item1;
                object itemValue = itemInfos.Item2;

                if (curValue == null && itemValue == null)
                {
                    curDisplay = name;
                    break;
                }
                if (curValue is UnityEngine.Object curValueObj
                    && curValueObj == itemValue as UnityEngine.Object)
                {
                    curDisplay = name;
                    break;
                }
                if (itemValue == null)
                {
                    // nothing
                }
                else if (itemValue.Equals(curValue))
                {
                    curDisplay = name;
                    break;
                }
            }
            #endregion

            #region Dropdown

            Rect leftRect = EditorGUI.PrefixLabel(position, label);

            GUI.SetNextControlName(FieldControlName);
            // ReSharper disable once InvertIf
            if (EditorGUI.DropdownButton(leftRect, new GUIContent(curDisplay), FocusType.Keyboard))
            {
                float minHeight = advancedDropdownAttribute.MinHeight;
                float itemHeight = advancedDropdownAttribute.ItemHeight > 0
                    ? advancedDropdownAttribute.ItemHeight
                    : EditorGUIUtility.singleLineHeight;
                float titleHeight = advancedDropdownAttribute.TitleHeight;
                Vector2 size;
                if (minHeight < 0)
                {
                    if(advancedDropdownAttribute.UseTotalItemCount)
                    {
                        float totalItemCount = GetValueItemCounts(dropdownListValue);
                        // Debug.Log(totalItemCount);
                        size = new Vector2(position.width, totalItemCount * itemHeight + titleHeight);
                    }
                    else
                    {
                        float maxChildCount = GetDropdownPageHeight(dropdownListValue, itemHeight, advancedDropdownAttribute.SepHeight).Max();
                        size = new Vector2(position.width, maxChildCount + titleHeight);
                    }
                }
                else
                {
                    size = new Vector2(position.width, minHeight);
                }

                // Vector2 size = new Vector2(position.width, maxChildCount * EditorGUIUtility.singleLineHeight + 31f);
                SaintsAdvancedDropdown dropdown = new SaintsAdvancedDropdown(
                    dropdownListValue,
                    size,
                    position,
                    new AdvancedDropdownState(),
                    curItem =>
                    {
                        Util.SetValue(property, curItem, parentObj, parentType, field);
                        SetValueChanged(property);
                    },
                    GetIcon);
                dropdown.Show(position);
                dropdown.BindWindowPosition();
            }

            #endregion
        }

        private static IEnumerable<float> GetDropdownPageHeight(IAdvancedDropdownList dropdownList, float itemHeight, float sepHeight)
        {
            if (dropdownList.ChildCount() == 0)
            {
                // Debug.Log($"yield 0");
                yield return 0;
                yield break;
            }

            // Debug.Log($"yield {dropdownList.children.Count}");
            yield return dropdownList.ChildCount() * itemHeight + dropdownList.SepCount() * sepHeight;
            foreach (IEnumerable<float> eachChildHeight in dropdownList.children.Select(child => GetDropdownPageHeight(child, itemHeight, sepHeight)))
            {
                foreach (int i in eachChildHeight)
                {
                    yield return i;
                }
            }
        }

        private static int GetValueItemCounts(IAdvancedDropdownList dropdownList)
        {
            if (dropdownList.isSeparator)
            {
                return 0;
            }

            if(dropdownList.ChildCount() == 0)
            {
                return 1;
            }

            int count = 0;
            foreach (IAdvancedDropdownList child in dropdownList.children)
            {
                count += GetValueItemCounts(child);
            }

            return count;

            // if(dropdownList.ChildCount() == 0)
            // {
            //     Debug.Log(1);
            //     yield return 1;
            //     yield break;
            // }
            //
            // // Debug.Log(dropdownList.ChildCount());
            // // yield return dropdownList.children.Count(each => each.ChildCount() == 0);
            // foreach (IAdvancedDropdownList eachChild in dropdownList.children)
            // {
            //     foreach (int subChildCount in GetChildCounts(eachChild))
            //     {
            //         if(subChildCount > 0)
            //         {
            //             Debug.Log(subChildCount);
            //             yield return subChildCount;
            //         }
            //     }
            // }
        }

        private Texture2D GetIcon(string icon)
        {
            if (_iconCache.TryGetValue(icon, out Texture2D result))
            {
                return result;
            }

            result = RichTextDrawer.LoadTexture(icon);
            if (result == null)
            {
                return null;
            }
            if (result.width == 1 && result.height == 1)
            {
                return null;
            }
            _iconCache[icon] = result;
            return result;
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute) => _error == "" ? 0 : ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => _error == "" ? position : ImGuiHelpBox.Draw(position, _error, MessageType.Error);

        #endregion

#if UNITY_2021_3_OR_NEWER

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            VisualElement container,
            Label fakeLabel,
            object parent)
        {
            VisualElement root = new VisualElement();

            VisualElement popContainer = new VisualElement
            {
                name = "PopContainer",
                style =
                {
                    borderLeftColor = Color.green,
                    borderRightColor = Color.green,
                    borderTopColor = Color.green,
                    borderBottomColor = Color.green,

                    borderLeftWidth = 1,
                    borderRightWidth = 1,
                    borderTopWidth = 1,
                    borderBottomWidth = 1,
                },
            };

            MetaInfo metaInfo = GetMetaInfo(property, (AdvancedDropdownAttribute)saintsAttribute, parent);
            popContainer.Add(SaintsAdvancedDropdownUiToolkit.CloneTree(metaInfo, curItem => Util.SetValue(property, curItem, parent, parent.GetType(), metaInfo.FieldInfo)));

            // root.Add(new Button(() =>
            // {
            //     popContainer.Clear();
            //     Debug.Log("Done");
            // })
            // {
            //     text = "Remove",
            // });

            root.Add(new Button(() =>
            {
                popContainer.Clear();
                MetaInfo metaInfo = GetMetaInfo(property, (AdvancedDropdownAttribute)saintsAttribute, parent);
                popContainer.Add(SaintsAdvancedDropdownUiToolkit.CloneTree(metaInfo, curItem => Util.SetValue(property, curItem, parent, parent.GetType(), metaInfo.FieldInfo)));
                
                Debug.Log("Done");
            })
            {
                text = "Reload",
            });

            // root.Add(new Button(() =>
            // {
            //     // child.AddToClassList("saintsfield-advanced-dropdown-in-from-right");
            //     popContainer.Query<VisualElement>(className: "saintsfield-advanced-dropdown-item").ForEach(each =>
            //     {
            //         Debug.Log(each);
            //         each.RemoveFromClassList("saintsfield-advanced-dropdown-item-right");
            //     });
            //
            //     Debug.Log("Done");
            // })
            // {
            //     text = "Translate",
            // });

            // root.Add(new Button(() =>
            // {
            //     popContainer.Query<VisualElement>(className: "saintsfield-advanced-dropdown-item").ForEach(each =>
            //     {
            //         each.RemoveFromClassList("saintsfield-advanced-dropdown-item-right");
            //         // each.style.translate = new Translate(Length.Percent(-100), 0);
            //     });
            //
            //     Debug.Log("Done");
            // })
            // {
            //     text = "Add Class",
            // });
            root.Add(popContainer);

            return root;

            // Button button = new Button
            // {
            //     text = "Open",
            //     style =
            //     {
            //         flexGrow = 1,
            //     },
            // };
            //
            // button.clicked += () => UnityEditor.PopupWindow.Show(button.worldBound, new SaintsAdvancedDropdownUiToolkit(button.worldBound.width));

            // return button;
        }

        // protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
        //     ISaintsAttribute saintsAttribute, int index, VisualElement container, object parent) => new HelpBox("Not supported for UI Toolkit", HelpBoxMessageType.Error);
#endif
    }
}

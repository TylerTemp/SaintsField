using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Drawers.SaintsHashSetTypeDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using SaintsField.Utils;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using SearchParamType = SaintsField.Editor.Drawers.SaintsHashSetTypeDrawer.SaintsHashSetDrawer.SearchParamType;

namespace SaintsField.Editor.Utils.IMGUIEditDrawer
{
    internal static class IMGUIEditHashSet
    {
        private const float VerticalPadding = 1f;
        private const float SizeWidth = 48f;
        private const float MenuWidth = 18f;
        private const float ControlGap = 4f;
        private const float FooterButtonsWidth = 58f;
        private const float PagerInputWidth = 30f;
        private const float PagerItemsLabelWidth = 65f;
        private const float PagerButtonWidth = 19f;
        private const float PagerPageLabelWidth = 30f;
        private const float PagerSeparatorWidth = 8f;

        private static readonly SaintsHashSetAttribute DefaultAttribute =
            new SaintsHashSetAttribute(searchable: false);

        private sealed class SearchState
        {
            public bool Started = true;
            public bool Finished = true;
            public IEnumerator<int> SourceGenerator;
            public string SearchText = "";
            public double DebounceSearchTime;
            public readonly List<int> HitIndexes = new List<int>();
            public readonly List<int> CachedHitIndexes = new List<int>();
            public readonly List<int> VisibleIndexes = new List<int>();
            public int PageIndex;
            public int Size;
            public int TotalPage = 1;
            public int NumberOfItemsPerPage;
        }

        private sealed class HashSetContext
        {
            public string Key;
            public string Label;
            public Type ValueType;
            public Type ElementType;
            public Type SetInterface;
            public object RawValue;
            public readonly List<object> Values = new List<object>();
            public MethodInfo AddMethod;
            public MethodInfo RemoveMethod;
            public MethodInfo ContainsMethod;
            public Action<object> BeforeSet;
            public Action<object> SetterOrNull;
            public bool LabelGrayColor;
            public bool InHorizontalLayout;
            public IReadOnlyList<Attribute> AllAttributes;
            public IReadOnlyList<Attribute> ElementAttributes;
            public IReadOnlyList<object> Targets;
            public IRichTextTagProvider RichTextTagProvider;
            public SaintsHashSetAttribute Attribute;
            public ReorderableList ReorderableList;
            public bool StateInitialized;
            public bool SearchEnabled;
            public bool PagingEnabled;
            public bool DefaultSearch = true;
            public bool ObjectSearch = true;
            public bool ExtraSearch;
            public (MethodInfo methodInfo, SearchParamType paramType) ExtraSearchMethod;
            public object ExtraSearchTarget;
            public readonly SearchState Search = new SearchState();
            public readonly IMGUILoading Loading = new IMGUILoading();
            public Texture2D IconLeft;
            public Texture2D IconRight;
            public bool AddPanelOpen;
            public object AddValue;
            public string AddError = "";
        }

        private static readonly Dictionary<string, HashSetContext> Contexts =
            new Dictionary<string, HashSetContext>();

        public static (bool ok, float height) GetPropertyHeight(
            string label, Type valueType, object value, Action<object> beforeSet, Action<object> setterOrNull,
            bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes,
            IReadOnlyList<object> targets, IRichTextTagProvider richTextTagProvider, string foldoutViewKey)
        {
            (bool isHashSet, Type elementType, Type setInterface) = GetHashSetType(valueType, value);
            if (!isHashSet)
            {
                return (false, 0f);
            }

            HashSetContext context = EnsureContext(label, valueType, value, beforeSet, setterOrNull,
                labelGrayColor, inHorizontalLayout, allAttributes, targets, richTextTagProvider, foldoutViewKey,
                elementType, setInterface);
            return (true, GetHeight(context));
        }

        public static bool TryOnGUI(Rect position, string label, Type valueType, object value,
            Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout,
            IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets,
            IRichTextTagProvider richTextTagProvider, string foldoutViewKey)
        {
            (bool isHashSet, Type elementType, Type setInterface) = GetHashSetType(valueType, value);
            if (!isHashSet)
            {
                return false;
            }

            HashSetContext context = EnsureContext(label, valueType, value, beforeSet, setterOrNull,
                labelGrayColor, inHorizontalLayout, allAttributes, targets, richTextTagProvider, foldoutViewKey,
                elementType, setInterface);
            Draw(position, context);
            return true;
        }

        private static (bool ok, Type elementType, Type setInterface) GetHashSetType(Type valueType, object value)
        {
            Type type = value?.GetType() ?? valueType;
            if (type == null || RuntimeUtil.IsNull(value))
            {
                return (false, null, null);
            }

            IEnumerable<Type> candidates = type.GetInterfaces().Where(each => each.IsGenericType);
            if (type.IsGenericType)
            {
                candidates = candidates.Prepend(type);
            }
            foreach (Type candidate in candidates)
            {
                if (candidate.GetGenericTypeDefinition() == typeof(ISet<>))
                {
                    return (true, candidate.GetGenericArguments()[0], candidate);
                }
            }
            return (false, null, null);
        }

        private static HashSetContext EnsureContext(string label, Type valueType, object value,
            Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout,
            IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets,
            IRichTextTagProvider richTextTagProvider, string foldoutViewKey, Type elementType, Type setInterface)
        {
            string key = $"{foldoutViewKey}.hashSet";
            if (!Contexts.TryGetValue(key, out HashSetContext context) || context.ElementType != elementType)
            {
                context = new HashSetContext
                {
                    Key = key,
                    ElementType = elementType,
                    SetInterface = setInterface,
                };
                Type collectionInterface = typeof(ICollection<>).MakeGenericType(elementType);
                context.AddMethod = setInterface.GetMethod("Add", new[] { elementType });
                context.RemoveMethod = collectionInterface.GetMethod("Remove", new[] { elementType });
                context.ContainsMethod = collectionInterface.GetMethod("Contains", new[] { elementType });
                Contexts[key] = context;
            }

            context.Label = label;
            context.ValueType = valueType;
            context.RawValue = value;
            context.BeforeSet = beforeSet;
            context.SetterOrNull = setterOrNull;
            context.LabelGrayColor = labelGrayColor;
            context.InHorizontalLayout = inHorizontalLayout;
            context.AllAttributes = allAttributes ?? Array.Empty<Attribute>();
            context.ElementAttributes = context.AllAttributes.Where(each => each is not SaintsHashSetAttribute)
                .ToArray();
            context.Targets = targets ?? Array.Empty<object>();
            context.RichTextTagProvider = richTextTagProvider;
            context.Attribute = context.AllAttributes.OfType<SaintsHashSetAttribute>().FirstOrDefault() ??
                                DefaultAttribute;
            context.ExtraSearchTarget = context.Targets.FirstOrDefault(each => each != null);
            context.ExtraSearchMethod = !string.IsNullOrEmpty(context.Attribute.ExtraSearch) &&
                                        context.ExtraSearchTarget != null
                ? SaintsHashSetDrawer.GetSearchMethodInfo(context.Attribute.ExtraSearch,
                    context.ExtraSearchTarget.GetType(), elementType)
                : default;

            List<object> previousValues = new List<object>(context.Values);
            ReloadValues(context);
            bool valuesChanged = previousValues.Count != context.Values.Count ||
                                 previousValues.Where((valueAtIndex, index) =>
                                         !Util.GetIsEqual(valueAtIndex, context.Values[index]))
                                     .Any();

            if (!context.StateInitialized)
            {
                context.SearchEnabled = context.Attribute.Searchable;
                context.PagingEnabled = context.Attribute.NumberOfItemsPerPage > 0;
                context.DefaultSearch = true;
                context.ObjectSearch = context.Attribute.ObjectSearch;
                context.ExtraSearch = context.ExtraSearchMethod.methodInfo != null;
                context.Search.NumberOfItemsPerPage = context.Attribute.NumberOfItemsPerPage;
                SetFullResults(context);
                context.StateInitialized = true;
                SetExpanded(context.Key, true);
            }
            else if (valuesChanged)
            {
                RestartSearch(context, context.Search.SearchText, false, true);
            }

            EnsureReorderableList(context);
            context.ReorderableList.list = context.Values;
            context.ReorderableList.displayAdd = CanEdit(context);
            context.ReorderableList.displayRemove = CanEdit(context);
            context.ReorderableList.footerHeight = context.PagingEnabled || CanEdit(context)
                ? EditorGUIUtility.singleLineHeight
                : 0f;
            UpdateVisibleIndexes(context);
            return context;
        }

        private static void EnsureReorderableList(HashSetContext context)
        {
            if (context.ReorderableList != null)
            {
                return;
            }

            context.ReorderableList = new ReorderableList(context.Values, typeof(object), false, false, true, true)
            {
                headerHeight = 0f,
                footerHeight = EditorGUIUtility.singleLineHeight,
            };
            context.ReorderableList.elementHeightCallback = index => GetElementHeight(context, index);
            context.ReorderableList.drawElementCallback = (rect, index, _, _) => DrawElement(rect, context, index);
            context.ReorderableList.drawFooterCallback = rect =>
            {
                ReorderableList.defaultBehaviours.DrawFooter(rect, context.ReorderableList);
                DrawPagingFooter(rect, context);
            };
            context.ReorderableList.onAddCallback = _ => OpenAddPanel(context);
            context.ReorderableList.onRemoveCallback = list =>
            {
                int index = list.index >= 0 ? list.index : context.Values.Count - 1;
                if (index >= 0 && index < context.Values.Count)
                {
                    RemoveValues(context, new[] { context.Values[index] });
                }
            };
        }

        private static bool IsExpanded(string key) =>
            IMGUIEdit.ViewKey.ContainsKey(key) && IMGUIEdit.ViewKey[key];

        private static void SetExpanded(string key, bool expanded) => IMGUIEdit.ViewKey[key] = expanded;

        private static float GetHeight(HashSetContext context)
        {
            TickSearch(context);
            UpdateVisibleIndexes(context);
            float height = EditorGUIUtility.singleLineHeight + VerticalPadding * 2f;
            if (!IsExpanded(context.Key))
            {
                return height;
            }

            if (context.SearchEnabled)
            {
                height += EditorGUIUtility.singleLineHeight;
            }
            height += context.ReorderableList.GetHeight();
            if (context.AddPanelOpen && CanEdit(context))
            {
                height += GetAddPanelHeight(context, EditorGUIUtility.currentViewWidth) + 2f;
            }
            return height;
        }

        private static void Draw(Rect position, HashSetContext context)
        {
            Rect contentRect = new Rect(position)
            {
                y = position.y + VerticalPadding,
                height = Mathf.Max(0f, position.height - VerticalPadding * 2f),
            };
            (Rect headerRect, Rect bodyRect) = RectUtils.SplitHeightRect(contentRect,
                EditorGUIUtility.singleLineHeight);
            DrawHeader(headerRect, context);
            if (!IsExpanded(context.Key))
            {
                return;
            }

            TickSearch(context);
            UpdateVisibleIndexes(context);
            if (context.SearchEnabled)
            {
                (Rect searchRect, Rect afterSearchRect) = RectUtils.SplitHeightRect(bodyRect,
                    EditorGUIUtility.singleLineHeight);
                DrawSearchField(searchRect, context);
                bodyRect = afterSearchRect;
            }

            if (context.AddPanelOpen && CanEdit(context))
            {
                float panelHeight = GetAddPanelHeight(context, bodyRect.width);
                (Rect listRect, Rect panelRect) = RectUtils.SplitHeightRect(bodyRect,
                    Mathf.Max(0f, bodyRect.height - panelHeight - 2f));
                context.ReorderableList.DoList(listRect);
                panelRect.y += 2f;
                panelRect.height = panelHeight;
                DrawAddPanel(panelRect, context);
            }
            else
            {
                context.ReorderableList.DoList(bodyRect);
            }
        }

        private static void DrawHeader(Rect rect, HashSetContext context)
        {
            Rect sizeRect = new Rect(rect)
            {
                x = rect.xMax - SizeWidth,
                width = SizeWidth,
            };
            Rect menuRect = new Rect(rect)
            {
                x = sizeRect.x - MenuWidth - ControlGap,
                width = MenuWidth,
            };
            Rect foldoutRect = new Rect(rect)
            {
                width = Mathf.Max(0f, menuRect.x - rect.x - ControlGap),
            };

            SetExpanded(context.Key, EditorGUI.Foldout(foldoutRect, IsExpanded(context.Key),
                new GUIContent(context.Label), true));
            if (GUI.Button(menuRect, "...", EditorStyles.miniButton))
            {
                ShowMenu(menuRect, context);
            }

            using (new EditorGUI.DisabledScope(!CanEdit(context)))
            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                int newSize = EditorGUI.DelayedIntField(sizeRect, context.Values.Count);
                if (changed.changed)
                {
                    ChangeSize(context, Mathf.Max(0, newSize));
                }
            }
        }

        private static void ShowMenu(Rect rect, HashSetContext context)
        {
            GenericMenu menu = new GenericMenu();
            if (context.SetterOrNull == null)
            {
                menu.AddDisabledItem(new GUIContent("Set To Null"));
            }
            else
            {
                menu.AddItem(new GUIContent("Set To Null"), false, () =>
                {
                    context.BeforeSet?.Invoke(context.RawValue);
                    context.SetterOrNull(null);
                });
            }
            menu.AddSeparator("");

            bool pagingEnabled = context.PagingEnabled;
            menu.AddItem(new GUIContent("Paging"), pagingEnabled, () =>
            {
                context.PagingEnabled = !pagingEnabled;
                context.Search.NumberOfItemsPerPage = pagingEnabled
                    ? 0
                    : context.Attribute.NumberOfItemsPerPage > 0
                        ? context.Attribute.NumberOfItemsPerPage
                        : Mathf.Max(5, context.Values.Count / 2);
                context.Search.PageIndex = 0;
                context.ReorderableList.footerHeight = context.PagingEnabled || CanEdit(context)
                    ? EditorGUIUtility.singleLineHeight
                    : 0f;
                UpdateVisibleIndexes(context);
                Repaint();
            });

            bool searchEnabled = context.SearchEnabled;
            menu.AddItem(new GUIContent("Search"), searchEnabled, () =>
            {
                if (!searchEnabled && !context.DefaultSearch && !context.ExtraSearch)
                {
                    context.DefaultSearch = true;
                }
                context.SearchEnabled = !searchEnabled;
                if (searchEnabled)
                {
                    context.Search.SearchText = "";
                }
                RestartSearch(context, context.Search.SearchText, true, true);
                Repaint();
            });

            bool hasExtraSearch = context.ExtraSearchMethod.methodInfo != null;
            if (searchEnabled && hasExtraSearch)
            {
                menu.AddItem(new GUIContent("Default Search"), context.DefaultSearch, () =>
                {
                    context.DefaultSearch = !context.DefaultSearch;
                    if (!context.DefaultSearch && !context.ExtraSearch)
                    {
                        context.SearchEnabled = false;
                        context.Search.SearchText = "";
                    }
                    RestartSearch(context, context.Search.SearchText, false, true);
                    Repaint();
                });
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("Default Search"), context.DefaultSearch);
            }

            if (searchEnabled && context.DefaultSearch)
            {
                menu.AddItem(new GUIContent("Object Search"), context.ObjectSearch, () =>
                {
                    context.ObjectSearch = !context.ObjectSearch;
                    RestartSearch(context, context.Search.SearchText, false, true);
                    Repaint();
                });
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("Object Search"), context.ObjectSearch);
            }

            if (hasExtraSearch)
            {
                if (searchEnabled)
                {
                    menu.AddItem(new GUIContent("Extra Search"), context.ExtraSearch, () =>
                    {
                        context.ExtraSearch = !context.ExtraSearch;
                        if (!context.DefaultSearch && !context.ExtraSearch)
                        {
                            context.SearchEnabled = false;
                            context.Search.SearchText = "";
                        }
                        RestartSearch(context, context.Search.SearchText, false, true);
                        Repaint();
                    });
                }
                else
                {
                    menu.AddDisabledItem(new GUIContent("Extra Search"), context.ExtraSearch);
                }
            }
            menu.DropDown(rect);
        }

        private static void DrawSearchField(Rect rect, HashSetContext context)
        {
            string controlName = $"IMGUIEditHashSetSearch_{context.Key}";
            string oldSearchText = context.Search.SearchText;
            Rect fieldRect = new Rect(rect);
            if (context.Search.Started && !context.Search.Finished)
            {
                Rect loadingRect = new Rect(fieldRect)
                {
                    x = fieldRect.xMax - 14f,
                    width = 12f,
                };
                context.Loading.Draw(loadingRect);
                fieldRect.xMax -= 16f;
            }

            GUI.SetNextControlName(controlName);
            context.Search.SearchText = EditorGUI.TextField(fieldRect, GUIContent.none, context.Search.SearchText);
            if (oldSearchText != context.Search.SearchText)
            {
                RestartSearch(context, context.Search.SearchText, true);
            }

            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return &&
                GUI.GetNameOfFocusedControl() == controlName && !context.Search.Started &&
                context.Search.SourceGenerator != null)
            {
                context.Search.DebounceSearchTime = EditorApplication.timeSinceStartup - 1d;
            }

            if (string.IsNullOrEmpty(context.Search.SearchText))
            {
                EditorGUI.LabelField(new Rect(rect) { width = Mathf.Max(0f, rect.width - 6f) }, "Search",
                    new GUIStyle("label")
                    {
                        alignment = TextAnchor.MiddleRight,
                        normal = { textColor = Color.gray },
                        fontStyle = FontStyle.Italic,
                    });
            }
        }

        private static float GetElementHeight(HashSetContext context, int index)
        {
            if (index < 0 || index >= context.Values.Count || !context.Search.VisibleIndexes.Contains(index))
            {
                return 0f;
            }
            return IMGUIEdit.GetPropertyHeight($"Element {index}", context.ElementType, context.Values[index],
                null, CanEdit(context) ? newValue => ReplaceValue(context, context.Values[index], newValue) : null,
                context.LabelGrayColor, context.InHorizontalLayout, context.ElementAttributes, context.Targets,
                context.RichTextTagProvider, $"{context.Key}.[{index}]") + 2f;
        }

        private static void DrawElement(Rect rect, HashSetContext context, int index)
        {
            if (rect.height <= 0f || index < 0 || index >= context.Values.Count ||
                !context.Search.VisibleIndexes.Contains(index))
            {
                return;
            }

            Rect useRect = new Rect(rect)
            {
                y = rect.y + 1f,
                height = Mathf.Max(0f, rect.height - 2f),
            };
            object oldValue = context.Values[index];
            IMGUIEdit.OnGUI(useRect, $"Element {index}", context.ElementType, oldValue, null,
                CanEdit(context) ? newValue => ReplaceValue(context, oldValue, newValue) : null,
                context.LabelGrayColor, context.InHorizontalLayout, context.ElementAttributes, context.Targets,
                context.RichTextTagProvider, $"{context.Key}.[{index}]");
        }

        private static void DrawPagingFooter(Rect rect, HashSetContext context)
        {
            if (!context.PagingEnabled)
            {
                return;
            }

            Rect pagingRect = new Rect(rect)
            {
                width = Mathf.Max(0f, rect.width - FooterButtonsWidth),
            };
            Rect perPageRect = new Rect(pagingRect) { width = PagerInputWidth };
            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                int newValue = EditorGUI.DelayedIntField(perPageRect, GUIContent.none,
                    context.Search.NumberOfItemsPerPage);
                if (changed.changed)
                {
                    context.Search.NumberOfItemsPerPage = Mathf.Max(newValue, 0);
                    context.Search.PageIndex = 0;
                    UpdateVisibleIndexes(context);
                }
            }

            Rect separatorRect = new Rect(perPageRect)
            {
                x = perPageRect.xMax,
                width = PagerSeparatorWidth,
            };
            EditorGUI.LabelField(separatorRect, "/");
            Rect countRect = new Rect(separatorRect)
            {
                x = separatorRect.xMax,
                width = PagerItemsLabelWidth,
            };
            using (new EditorGUI.DisabledScope(!CanEdit(context)))
            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                int newCount = EditorGUI.DelayedIntField(countRect, GUIContent.none, context.Values.Count);
                if (changed.changed)
                {
                    ChangeSize(context, Mathf.Max(0, newCount));
                    return;
                }
            }
            EditorGUI.LabelField(countRect, "Items", new GUIStyle("label")
            {
                alignment = TextAnchor.MiddleRight,
                normal = { textColor = Color.gray },
                fontStyle = FontStyle.Italic,
            });

            Rect previousRect = new Rect(countRect)
            {
                x = countRect.xMax,
                width = PagerButtonWidth,
            };
            using (new EditorGUI.DisabledScope(context.Search.PageIndex <= 0))
            {
                context.IconLeft ??= Util.LoadResource<Texture2D>("classic-dropdown-left.png");
                if (GUI.Button(previousRect, context.IconLeft, EditorStyles.miniButtonLeft))
                {
                    context.Search.PageIndex = Mathf.Max(0, context.Search.PageIndex - 1);
                    UpdateVisibleIndexes(context);
                }
            }

            Rect pageRect = new Rect(previousRect)
            {
                x = previousRect.xMax,
                width = PagerInputWidth,
            };
            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                int pageIndex = EditorGUI.DelayedIntField(pageRect, GUIContent.none,
                    context.Search.PageIndex + 1) - 1;
                if (changed.changed)
                {
                    context.Search.PageIndex = Mathf.Max(0, pageIndex);
                    UpdateVisibleIndexes(context);
                }
            }

            Rect pageCountRect = new Rect(pageRect)
            {
                x = pageRect.xMax,
                width = PagerPageLabelWidth,
            };
            EditorGUI.LabelField(pageCountRect, $"/ {context.Search.TotalPage}");
            Rect nextRect = new Rect(pageCountRect)
            {
                x = pageCountRect.xMax,
                width = PagerButtonWidth,
            };
            using (new EditorGUI.DisabledScope(context.Search.PageIndex >= context.Search.TotalPage - 1))
            {
                context.IconRight ??= Util.LoadResource<Texture2D>("classic-dropdown-right.png");
                if (GUI.Button(nextRect, context.IconRight, EditorStyles.miniButtonRight))
                {
                    context.Search.PageIndex = Mathf.Min(context.Search.PageIndex + 1,
                        context.Search.TotalPage - 1);
                    UpdateVisibleIndexes(context);
                }
            }
        }

        private static float GetAddPanelHeight(HashSetContext context, float width)
        {
            float height = IMGUIEdit.GetPropertyHeight("Value", context.ElementType, context.AddValue, null,
                newValue =>
                {
                    context.AddValue = newValue;
                    RefreshAddError(context);
                }, context.LabelGrayColor, context.InHorizontalLayout, Array.Empty<Attribute>(), context.Targets,
                context.RichTextTagProvider, $"{context.Key}.[add]");
            if (!string.IsNullOrEmpty(context.AddError))
            {
                height += ImGuiHelpBox.GetHeight(context.AddError, width, MessageType.Error) + 2f;
            }
            return height + EditorGUIUtility.singleLineHeight + 8f;
        }

        private static void DrawAddPanel(Rect rect, HashSetContext context)
        {
            GUI.Box(rect, GUIContent.none, EditorStyles.helpBox);
            Rect workRect = new Rect(rect)
            {
                x = rect.x + 4f,
                y = rect.y + 4f,
                width = Mathf.Max(0f, rect.width - 8f),
                height = Mathf.Max(0f, rect.height - 8f),
            };
            float valueHeight = IMGUIEdit.GetPropertyHeight("Value", context.ElementType, context.AddValue, null,
                _ => { }, context.LabelGrayColor, context.InHorizontalLayout, Array.Empty<Attribute>(),
                context.Targets, context.RichTextTagProvider, $"{context.Key}.[add]");
            (Rect valueRect, Rect afterValueRect) = RectUtils.SplitHeightRect(workRect, valueHeight);
            IMGUIEdit.OnGUI(valueRect, "Value", context.ElementType, context.AddValue, null, newValue =>
                {
                    context.AddValue = newValue;
                    RefreshAddError(context);
                }, context.LabelGrayColor, context.InHorizontalLayout, Array.Empty<Attribute>(), context.Targets,
                context.RichTextTagProvider, $"{context.Key}.[add]");

            Rect actionArea = afterValueRect;
            if (!string.IsNullOrEmpty(context.AddError))
            {
                float errorHeight = ImGuiHelpBox.GetHeight(context.AddError, workRect.width, MessageType.Error);
                (Rect errorRect, Rect afterErrorRect) = RectUtils.SplitHeightRect(afterValueRect, errorHeight + 2f);
                errorRect.height = errorHeight;
                ImGuiHelpBox.Draw(errorRect, context.AddError, MessageType.Error);
                actionArea = afterErrorRect;
            }

            Rect actionRect = new Rect(actionArea)
            {
                y = actionArea.yMax - EditorGUIUtility.singleLineHeight,
                height = EditorGUIUtility.singleLineHeight,
            };
            (Rect okRect, Rect cancelRect) = RectUtils.SplitWidthRect(actionRect, actionRect.width * 0.5f);
            using (new EditorGUI.DisabledScope(!string.IsNullOrEmpty(context.AddError)))
            {
                if (GUI.Button(okRect, "OK"))
                {
                    AddValue(context, context.AddValue);
                }
            }
            if (GUI.Button(cancelRect, "Cancel"))
            {
                context.AddPanelOpen = false;
                Repaint();
            }
        }

        private static void OpenAddPanel(HashSetContext context)
        {
            if (!CanEdit(context))
            {
                return;
            }
            context.AddValue = CreateDefaultValue(context.ElementType);
            context.AddPanelOpen = true;
            RefreshAddError(context);
            Repaint();
        }

        private static void RefreshAddError(HashSetContext context)
        {
            context.AddError = Contains(context, context.AddValue) ? "Value already exists in hash set." : "";
        }

        private static void AddValue(HashSetContext context, object value)
        {
            if (!CanEdit(context) || Contains(context, value))
            {
                RefreshAddError(context);
                return;
            }
            context.BeforeSet?.Invoke(context.RawValue);
            if ((bool)context.AddMethod.Invoke(context.RawValue, new[] { value }))
            {
                context.SetterOrNull(context.RawValue);
            }
            context.AddPanelOpen = false;
            ReloadAndRefresh(context);
        }

        private static void ReplaceValue(HashSetContext context, object oldValue, object newValue)
        {
            if (!CanEdit(context) || Util.GetIsEqual(oldValue, newValue))
            {
                return;
            }
            if (Contains(context, newValue))
            {
                Debug.LogWarning($"Setting hash set value {oldValue} to existing value {newValue} is ignored");
                context.ReorderableList = null;
                Repaint();
                return;
            }

            context.BeforeSet?.Invoke(context.RawValue);
            if (!(bool)context.RemoveMethod.Invoke(context.RawValue, new[] { oldValue }))
            {
                return;
            }
            if (!(bool)context.AddMethod.Invoke(context.RawValue, new[] { newValue }))
            {
                context.AddMethod.Invoke(context.RawValue, new[] { oldValue });
                return;
            }
            context.SetterOrNull(context.RawValue);
            ReloadAndRefresh(context);
        }

        private static void RemoveValues(HashSetContext context, IEnumerable<object> values)
        {
            if (!CanEdit(context))
            {
                return;
            }
            object[] removeValues = values.ToArray();
            if (removeValues.Length == 0)
            {
                return;
            }

            context.BeforeSet?.Invoke(context.RawValue);
            bool changed = false;
            foreach (object value in removeValues)
            {
                changed |= (bool)context.RemoveMethod.Invoke(context.RawValue, new[] { value });
            }
            if (changed)
            {
                context.SetterOrNull(context.RawValue);
            }
            ReloadAndRefresh(context);
        }

        private static void ChangeSize(HashSetContext context, int newSize)
        {
            if (!CanEdit(context) || newSize == context.Values.Count)
            {
                return;
            }
            if (newSize > context.Values.Count)
            {
                OpenAddPanel(context);
                return;
            }
            RemoveValues(context, context.Values.Skip(newSize));
        }

        private static void ReloadAndRefresh(HashSetContext context)
        {
            ReloadValues(context);
            RestartSearch(context, context.Search.SearchText, false, true);
            context.ReorderableList = null;
            EnsureReorderableList(context);
            Repaint();
        }

        private static void ReloadValues(HashSetContext context)
        {
            context.Values.Clear();
            if (!RuntimeUtil.IsNull(context.RawValue))
            {
                context.Values.AddRange(((IEnumerable)context.RawValue).Cast<object>());
            }
        }

        private static bool Contains(HashSetContext context, object value) =>
            !RuntimeUtil.IsNull(context.RawValue) &&
            (bool)context.ContainsMethod.Invoke(context.RawValue, new[] { value });

        private static bool CanEdit(HashSetContext context) =>
            context.SetterOrNull != null && !RuntimeUtil.IsNull(context.RawValue);

        private static void RestartSearch(HashSetContext context, string searchText, bool resetPage,
            bool immediate = false)
        {
            SearchState search = context.Search;
            string safeSearchText = searchText ?? "";
            if (resetPage)
            {
                search.PageIndex = 0;
            }
            search.Size = context.Values.Count;
            search.SourceGenerator?.Dispose();
            search.SourceGenerator = null;
            if (string.IsNullOrEmpty(safeSearchText))
            {
                search.SearchText = "";
                SetFullResults(context);
                return;
            }

            IReadOnlyList<int> current = search.Started ? search.HitIndexes : search.CachedHitIndexes;
            search.CachedHitIndexes.Clear();
            search.CachedHitIndexes.AddRange(current);
            search.HitIndexes.Clear();
            search.SearchText = safeSearchText;
            search.Started = false;
            search.Finished = false;
            search.DebounceSearchTime = immediate
                ? 0d
                : EditorApplication.timeSinceStartup + 0.6d;
            search.SourceGenerator = SearchValues(context, safeSearchText).GetEnumerator();
            UpdateVisibleIndexes(context);
        }

        private static void SetFullResults(HashSetContext context)
        {
            SearchState search = context.Search;
            search.Started = true;
            search.Finished = true;
            search.Size = context.Values.Count;
            search.SourceGenerator?.Dispose();
            search.SourceGenerator = null;
            search.HitIndexes.Clear();
            search.HitIndexes.AddRange(Enumerable.Range(0, context.Values.Count));
            search.CachedHitIndexes.Clear();
            search.CachedHitIndexes.AddRange(search.HitIndexes);
            UpdateVisibleIndexes(context);
        }

        private static IEnumerable<int> SearchValues(HashSetContext context, string searchText)
        {
            IReadOnlyList<ListSearchToken> tokens = SerializedUtils.ParseSearch(searchText).ToArray();
            for (int index = 0; index < context.Values.Count; index++)
            {
                object value = context.Values[index];
                bool matched = context.DefaultSearch &&
                               Util.SearchObjectWithTokens(value, tokens, context.ObjectSearch);
                if (!matched && context.ExtraSearch && context.ExtraSearchMethod.methodInfo != null)
                {
                    object[] methodParams = context.ExtraSearchMethod.paramType switch
                    {
                        SearchParamType.Index => new object[] { index, tokens },
                        SearchParamType.Target => new[] { value, tokens },
                        _ => new[] { value, index, tokens },
                    };
                    matched = (bool)context.ExtraSearchMethod.methodInfo.Invoke(context.ExtraSearchTarget,
                        methodParams);
                }
                yield return matched ? index : -1;
            }
        }

        private static void TickSearch(HashSetContext context)
        {
            if (Event.current == null || Event.current.type != EventType.Repaint)
            {
                return;
            }
            SearchState search = context.Search;
            if (!search.Started && search.SourceGenerator != null &&
                EditorApplication.timeSinceStartup > search.DebounceSearchTime)
            {
                search.Started = true;
                UpdateVisibleIndexes(context);
            }
            if (!search.Started || search.Finished || search.SourceGenerator == null)
            {
                return;
            }

            bool changed = false;
            for (int tick = 0; tick < 50; tick++)
            {
                if (search.SourceGenerator.MoveNext())
                {
                    int index = search.SourceGenerator.Current;
                    if (index != -1)
                    {
                        search.HitIndexes.Add(index);
                        changed = true;
                    }
                }
                else
                {
                    search.Finished = true;
                    search.CachedHitIndexes.Clear();
                    search.CachedHitIndexes.AddRange(search.HitIndexes);
                    search.SourceGenerator.Dispose();
                    search.SourceGenerator = null;
                    changed = true;
                    break;
                }
            }
            if (changed)
            {
                UpdateVisibleIndexes(context);
                Repaint();
            }
        }

        private static void UpdateVisibleIndexes(HashSetContext context)
        {
            SearchState search = context.Search;
            IReadOnlyList<int> results = search.Started ? search.HitIndexes : search.CachedHitIndexes;
            int pageCount;
            int pageIndex;
            int skip;
            int take;
            if (!context.PagingEnabled || search.NumberOfItemsPerPage <= 0)
            {
                pageCount = 1;
                pageIndex = 0;
                skip = 0;
                take = int.MaxValue;
            }
            else
            {
                pageCount = Mathf.Max(1, Mathf.CeilToInt(results.Count / (float)search.NumberOfItemsPerPage));
                pageIndex = Mathf.Clamp(search.PageIndex, 0, pageCount - 1);
                skip = pageIndex * search.NumberOfItemsPerPage;
                take = search.NumberOfItemsPerPage;
            }
            search.PageIndex = pageIndex;
            search.TotalPage = pageCount;
            search.VisibleIndexes.Clear();
            search.VisibleIndexes.AddRange(results.Where(each => each >= 0 && each < context.Values.Count)
                .Skip(skip).Take(take));
        }

        private static void Repaint()
        {
            GUI.changed = true;
            EditorWindow.focusedWindow?.Repaint();
        }

        private static object CreateDefaultValue(Type type)
        {
            if (type == typeof(string))
            {
                return "";
            }
            if (type == typeof(Guid))
            {
                return Guid.NewGuid();
            }
            if (type?.IsEnum == true)
            {
                Array values = Enum.GetValues(type);
                return values.Length == 0 ? Activator.CreateInstance(type) : values.GetValue(0);
            }
            return type?.IsValueType == true ? Activator.CreateInstance(type) : null;
        }
    }

}

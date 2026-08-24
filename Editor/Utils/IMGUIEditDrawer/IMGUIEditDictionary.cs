using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Utils.IMGUIEditDrawer
{
    internal static class IMGUIEditDictionary
    {
        private const float VerticalPadding = 1f;
        private const float SizeWidth = 50f;
        private const float ButtonWidth = 18f;
        private const float ControlGap = 4f;
        private const float TablePadding = 2f;
        private const float CellPadding = 2f;
        private const float AddPanelPadding = 4f;
        private const double DebounceTime = 0.6d;
        private const float PagerInputWidth = 30f;
        private const float PagerItemsLabelWidth = 65f;
        private const float PagerButtonWidth = 19f;
        private const float PagerPageLabelWidth = 30f;
        private const float PagerSeparatorWidth = 8f;
        private const float SearchGap = 5f;

        private static readonly SaintsDictionaryAttribute DefaultSaintsDictionaryAttribute =
            new SaintsDictionaryAttribute(searchable: false, numberOfItemsPerPage: 0);

        private sealed class AsyncSearchItems
        {
            public bool Started = true;
            public bool Finished = true;
            public IEnumerator<object> SourceGenerator;
            public string KeySearchText = "";
            public string ValueSearchText = "";
            public double DebounceSearchTime;
            public readonly List<object> HitKeys = new List<object>();
            public readonly List<object> CachedHitKeys = new List<object>();
            public readonly List<object> VisibleKeys = new List<object>();
            public int PageIndex;
            public int Size;
            public int TotalPage = 1;
            public int NumberOfItemsPerPage;
        }

        private sealed class DictionaryContext
        {
            public string Key;
            public string Label;
            public Type ValueType;
            public Type KeyType;
            public Type ValueValueType;
            public object RawValue;
            public Action<object> BeforeSet;
            public Action<object> SetterOrNull;
            public bool LabelGrayColor;
            public bool InHorizontalLayout;
            public IReadOnlyList<Attribute> AllAttributes;
            public IReadOnlyList<object> Targets;
            public IRichTextTagProvider RichTextTagProvider;
            public bool IsReadOnly;
            public PropertyInfo KeysProperty;
            public PropertyInfo IndexerProperty;
            public MethodInfo RemoveMethod;
            public MethodInfo ContainsKeyMethod;
            public HashSet<int> SelectedIndexes = new HashSet<int>();
            public bool AddPanelOpen;
            public object AddKey;
            public object AddValue;
            public string AddError = "";
            public string Error = "";
            public SaintsDictionaryAttribute Attribute;
            public bool ConfigurationInitialized;
            public bool SearchEnabled;
            public bool ObjectSearch;
            public readonly AsyncSearchItems SearchItems = new AsyncSearchItems();
            public readonly IMGUILoading KeyLoading = new IMGUILoading();
            public readonly IMGUILoading ValueLoading = new IMGUILoading();
            public Texture2D LeftIcon;
            public Texture2D RightIcon;
        }

        private static readonly Dictionary<string, DictionaryContext> DictionaryContexts =
            new Dictionary<string, DictionaryContext>();

        public static (bool ok, float height) GetPropertyHeight(
            string label, Type valueType, object value, Action<object> beforeSet, Action<object> setterOrNull,
            bool labelGrayColor, bool inHorizontalLayout,
            IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets,
            IRichTextTagProvider richTextTagProvider, string foldoutViewKey)
        {
            (bool isDictionary, Type keyType, Type dictValueType, bool isReadOnly) =
                GetDictionaryTypes(valueType, value);
            if (!isDictionary)
            {
                return (false, 0f);
            }

            DictionaryContext context = EnsureDictionaryContext(label, valueType, value, beforeSet, setterOrNull,
                labelGrayColor, inHorizontalLayout, allAttributes, targets, richTextTagProvider, foldoutViewKey,
                keyType, dictValueType, isReadOnly);
            return (true, GetDictionaryHeight(context));
        }

        public static bool TryOnGUI(
            Rect position,
            string label, Type valueType, object value, Action<object> beforeSet, Action<object> setterOrNull,
            bool labelGrayColor, bool inHorizontalLayout,
            IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets,
            IRichTextTagProvider richTextTagProvider, string foldoutViewKey)
        {
            (bool isDictionary, Type keyType, Type dictValueType, bool isReadOnly) =
                GetDictionaryTypes(valueType, value);
            if (!isDictionary)
            {
                return false;
            }

            DictionaryContext context = EnsureDictionaryContext(label, valueType, value, beforeSet, setterOrNull,
                labelGrayColor, inHorizontalLayout, allAttributes, targets, richTextTagProvider, foldoutViewKey,
                keyType, dictValueType, isReadOnly);
            DrawDictionary(position, context);
            return true;
        }

        private static (bool ok, Type keyType, Type valueType, bool isReadOnly) GetDictionaryTypes(Type valueType,
            object value)
        {
            Type type = value?.GetType() ?? valueType;
            if (type == null)
            {
                return (false, null, null, false);
            }

            Type keyType = null;
            Type valueValueType = null;
            bool isReadOnly = false;
            IEnumerable<Type> candidates = type.GetInterfaces().Where(each => each.IsGenericType);
            if (type.IsGenericType)
            {
                candidates = candidates.Prepend(type);
            }

            foreach (Type candidate in candidates)
            {
                Type definition = candidate.GetGenericTypeDefinition();
                if (definition == typeof(IDictionary<,>))
                {
                    Type[] args = candidate.GetGenericArguments();
                    keyType = args[0];
                    valueValueType = args[1];
                    return (true, keyType, valueValueType, false);
                }

                if (definition == typeof(IReadOnlyDictionary<,>))
                {
                    Type[] args = candidate.GetGenericArguments();
                    keyType = args[0];
                    valueValueType = args[1];
                    isReadOnly = true;
                }
            }

            return (keyType != null, keyType, valueValueType, isReadOnly);
        }

        private static bool IsExpanded(string key) =>
            IMGUIEdit.ViewKey.ContainsKey(key) && IMGUIEdit.ViewKey[key];

        private static void SetExpanded(string key, bool expanded) => IMGUIEdit.ViewKey[key] = expanded;

        private static DictionaryContext EnsureDictionaryContext(string label, Type valueType, object value,
            Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout,
            IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets,
            IRichTextTagProvider richTextTagProvider, string foldoutViewKey, Type keyType, Type valueValueType,
            bool isReadOnly)
        {
            string key = $"{foldoutViewKey}.dictionary";
            if (!DictionaryContexts.ContainsKey(key))
            {
                DictionaryContexts[key] = new DictionaryContext
                {
                    Key = key,
                };
            }

            DictionaryContext context = DictionaryContexts[key];
            context.Label = label;
            context.ValueType = valueType;
            context.KeyType = keyType;
            context.ValueValueType = valueValueType;
            context.RawValue = value;
            context.BeforeSet = beforeSet;
            context.SetterOrNull = setterOrNull;
            context.LabelGrayColor = labelGrayColor;
            context.InHorizontalLayout = inHorizontalLayout;
            context.AllAttributes = allAttributes;
            context.Targets = targets;
            context.RichTextTagProvider = richTextTagProvider;
            context.Attribute = allAttributes?.OfType<SaintsDictionaryAttribute>().FirstOrDefault()
                                ?? DefaultSaintsDictionaryAttribute;
            (string accessError, PropertyInfo keysProperty, PropertyInfo indexerProperty, MethodInfo removeMethod,
                    MethodInfo containsKeyMethod) =
                GetDictionaryAccess(valueType, value, keyType);
            context.KeysProperty = keysProperty;
            context.IndexerProperty = indexerProperty;
            context.RemoveMethod = removeMethod;
            context.ContainsKeyMethod = containsKeyMethod;
            context.IsReadOnly = isReadOnly || GetDictionaryReadOnly(value);
            context.Error = RuntimeUtil.IsNull(value) ? "" : accessError;
            if (!CanEditDictionary(context))
            {
                context.AddPanelOpen = false;
            }

            if (context.AddPanelOpen)
            {
                context.AddError = GetAddKeyError(context);
            }

            object[] keys = GetDictionaryKeys(context).ToArray();
            EnsureSearchState(context, keys);
            ClampDictionarySelection(context, keys.Length);
            return context;
        }

        private static (string error, PropertyInfo keysProperty, PropertyInfo indexerProperty,
            MethodInfo removeMethod, MethodInfo containsKeyMethod) GetDictionaryAccess(Type valueType, object value,
            Type keyType)
        {
            Type type = value?.GetType() ?? valueType;
            if (type == null)
            {
                return ("Dictionary value edit requires a dictionary type.", null, null, null, null);
            }

            PropertyInfo keysProperty = type.GetProperty("Keys");
            PropertyInfo indexerProperty = keyType == null ? null : type.GetProperty("Item", new[] { keyType });
            MethodInfo removeMethod = keyType == null ? null : type.GetMethod("Remove", new[] { keyType });
            MethodInfo containsKeyMethod = keyType == null ? null : type.GetMethod("ContainsKey", new[] { keyType });

            if (typeof(IDictionary).IsAssignableFrom(type))
            {
                return ("", keysProperty, indexerProperty, removeMethod, containsKeyMethod);
            }

            if (keysProperty == null)
            {
                return ("Dictionary value edit requires a Keys property.", null, indexerProperty, removeMethod,
                    containsKeyMethod);
            }

            if (indexerProperty == null)
            {
                return ("Dictionary value edit requires an indexer property.", keysProperty, null, removeMethod,
                    containsKeyMethod);
            }

            return ("", keysProperty, indexerProperty, removeMethod, containsKeyMethod);
        }

        private static bool GetDictionaryReadOnly(object rawValue)
        {
            if (RuntimeUtil.IsNull(rawValue))
            {
                return false;
            }

            if (rawValue is IDictionary dictionary && dictionary.IsReadOnly)
            {
                return true;
            }

            PropertyInfo isReadOnlyProperty = rawValue.GetType().GetProperty("IsReadOnly");
            if (isReadOnlyProperty?.PropertyType == typeof(bool))
            {
                return (bool)isReadOnlyProperty.GetValue(rawValue);
            }

            return false;
        }

        private static float GetDictionaryHeight(DictionaryContext context)
        {
            TickAsyncSearch(context);

            float singleLineHeight = EditorGUIUtility.singleLineHeight;
            float height = singleLineHeight + VerticalPadding * 2;
            if (context.Error != "")
            {
                return height + ImGuiHelpBox.GetHeight(context.Error, EditorGUIUtility.currentViewWidth,
                    MessageType.Error);
            }

            if (!IsExpanded(context.Key) || RuntimeUtil.IsNull(context.RawValue))
            {
                return height;
            }

            height += TablePadding * 2 + singleLineHeight + singleLineHeight;
            if (context.SearchEnabled)
            {
                height += singleLineHeight;
            }

            foreach (object key in context.SearchItems.VisibleKeys)
            {
                object dictValue = GetDictionaryValue(context, key);
                height += GetDictionaryRowHeight(context, key, dictValue) + 1f;
            }

            if (context.AddPanelOpen && CanEditDictionary(context))
            {
                height += GetDictionaryAddPanelHeight(context, EditorGUIUtility.currentViewWidth) + 2f;
            }

            return height;
        }

        private static void DrawDictionary(Rect position, DictionaryContext context)
        {
            TickAsyncSearch(context);

            Rect contentRect = new Rect(position)
            {
                y = position.y + VerticalPadding,
                height = Mathf.Max(0f, position.height - VerticalPadding * 2),
            };

            Rect headerRect = new Rect(contentRect)
            {
                height = EditorGUIUtility.singleLineHeight,
            };
            DrawDictionaryHeader(headerRect, context);

            Rect leftRect = new Rect(contentRect)
            {
                y = headerRect.yMax,
                height = Mathf.Max(0f, contentRect.yMax - headerRect.yMax),
            };

            if (context.Error != "")
            {
                ImGuiHelpBox.Draw(leftRect, context.Error, MessageType.Error);
                return;
            }

            if (!IsExpanded(context.Key) || RuntimeUtil.IsNull(context.RawValue))
            {
                return;
            }

            GUI.Box(leftRect, GUIContent.none, EditorStyles.helpBox);
            Rect workRect = ShrinkRect(leftRect, TablePadding);

            (Rect tableHeaderRect, Rect afterHeaderRect) =
                RectUtils.SplitHeightRect(workRect, EditorGUIUtility.singleLineHeight);
            DrawDictionaryTableHeader(tableHeaderRect);
            leftRect = afterHeaderRect;

            if (context.SearchEnabled)
            {
                (Rect searchRect, Rect afterSearchRect) =
                    RectUtils.SplitHeightRect(leftRect, EditorGUIUtility.singleLineHeight);
                (Rect keySearchRect, Rect valueSearchRect) = GetDictionaryCellRects(searchRect);
                DrawSearchField(ShrinkRect(keySearchRect, CellPadding), true, context);
                DrawSearchField(ShrinkRect(valueSearchRect, CellPadding), false, context);
                leftRect = afterSearchRect;
            }

            object[] allKeys = GetDictionaryKeys(context).ToArray();
            object[] visibleKeys = context.SearchItems.VisibleKeys
                .Where(allKeys.Contains)
                .ToArray();
            for (int displayIndex = 0; displayIndex < visibleKeys.Length; displayIndex++)
            {
                object key = visibleKeys[displayIndex];
                int sourceIndex = Array.IndexOf(allKeys, key);
                object dictValue = GetDictionaryValue(context, key);
                float rowHeight = GetDictionaryRowHeight(context, key, dictValue);

                (Rect rowRect, Rect afterRowRect) = RectUtils.SplitHeightRect(leftRect, rowHeight);
                DrawDictionaryRow(rowRect, context, sourceIndex, key, dictValue);
                leftRect = afterRowRect;
            }

            (Rect footerRect, Rect afterFooterRect) =
                RectUtils.SplitHeightRect(leftRect, EditorGUIUtility.singleLineHeight);
            DrawDictionaryFooter(footerRect, context);
            leftRect = afterFooterRect;

            if (context.AddPanelOpen && CanEditDictionary(context))
            {
                Rect addPanelRect = new Rect(leftRect)
                {
                    y = leftRect.y + 2f,
                    height = Mathf.Max(0f, GetDictionaryAddPanelHeight(context, leftRect.width)),
                };
                DrawDictionaryAddPanel(addPanelRect, context);
            }
        }

        private static void DrawDictionaryHeader(Rect rect, DictionaryContext context)
        {
            int count = RuntimeUtil.IsNull(context.RawValue) ? 0 : GetDictionaryKeys(context).Count();
            Rect sizeRect = new Rect(rect)
            {
                x = rect.xMax - SizeWidth,
                width = SizeWidth,
            };
            Rect menuRect = new Rect(rect)
            {
                x = sizeRect.x - ButtonWidth - ControlGap,
                width = ButtonWidth,
            };
            Rect foldoutRect = new Rect(rect)
            {
                width = Mathf.Max(0f, menuRect.x - rect.x - ControlGap),
            };

            bool expanded = EditorGUI.Foldout(foldoutRect, IsExpanded(context.Key), new GUIContent(context.Label),
                true);
            SetExpanded(context.Key, expanded);

            if (GUI.Button(menuRect, "...", EditorStyles.miniButton))
            {
                ShowMenu(menuRect, context);
            }

            using (new EditorGUI.DisabledScope(!CanEditDictionary(context)))
            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                int newSize = EditorGUI.DelayedIntField(sizeRect, count);
                if (changed.changed)
                {
                    ChangeDictionarySize(context, Mathf.Max(0, newSize));
                }
            }
        }

        private static void ShowMenu(Rect rect, DictionaryContext context)
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

            AsyncSearchItems searchItems = context.SearchItems;
            bool pagingEnabled = searchItems.NumberOfItemsPerPage > 0;
            menu.AddItem(new GUIContent("Paging"), pagingEnabled, () =>
            {
                int configuredItemsPerPage = context.Attribute.NumberOfItemsPerPage;
                searchItems.NumberOfItemsPerPage = pagingEnabled
                    ? 0
                    : configuredItemsPerPage > 0
                        ? configuredItemsPerPage
                        : Mathf.Max(5, GetDictionaryKeys(context).Count() / 2);
                searchItems.PageIndex = 0;
                RefreshView(context);
            });

            bool searchEnabled = context.SearchEnabled;
            menu.AddItem(new GUIContent("Search"), searchEnabled, () =>
            {
                context.SearchEnabled = !searchEnabled;
                if (searchEnabled)
                {
                    RestartSearch(context, "", "", true);
                }
                RefreshView(context);
            });

            if (searchEnabled)
            {
                menu.AddItem(new GUIContent("Object Search"), context.ObjectSearch, () =>
                {
                    context.ObjectSearch = !context.ObjectSearch;
                    if (!string.IsNullOrEmpty(searchItems.KeySearchText)
                        || !string.IsNullOrEmpty(searchItems.ValueSearchText))
                    {
                        RestartSearch(context, searchItems.KeySearchText, searchItems.ValueSearchText, false);
                    }
                    RefreshView(context);
                });
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("Object Search"), context.ObjectSearch);
            }

            menu.DropDown(rect);
        }

        private static void RefreshView(DictionaryContext context)
        {
            UpdateVisibleKeys(context.SearchItems);
            GUI.changed = true;
            EditorWindow.focusedWindow?.Repaint();
        }

        private static void DrawSearchField(Rect rect, bool isKeySearch, DictionaryContext context)
        {
            AsyncSearchItems searchItems = context.SearchItems;
            bool searching = searchItems.Started && !searchItems.Finished;
            IMGUILoading loading = isKeySearch ? context.KeyLoading : context.ValueLoading;
            string controlName = $"{(isKeySearch ? "IMGUIEditDictionaryKeySearch" : "IMGUIEditDictionaryValueSearch")}_{context.Key}";
            string placeholder = isKeySearch ? "Key Search" : "Value Search";
            string currentText = isKeySearch ? searchItems.KeySearchText : searchItems.ValueSearchText;

            Rect fieldRect = new Rect(rect)
            {
                width = Mathf.Max(0f, rect.width - (searching ? 16f : 0f) - SearchGap),
            };
            if (searching)
            {
                Rect loadingRect = new Rect(rect)
                {
                    x = rect.xMax - 14f,
                    width = 12f,
                };
                loading.Draw(loadingRect);
            }

            GUI.SetNextControlName(controlName);
            string newText = EditorGUI.TextField(fieldRect, GUIContent.none, currentText);
            if (newText != currentText)
            {
                RestartSearch(context,
                    isKeySearch ? newText : searchItems.KeySearchText,
                    isKeySearch ? searchItems.ValueSearchText : newText,
                    true);
            }

            if (Event.current.type == EventType.KeyDown
                && Event.current.keyCode == KeyCode.Return
                && GUI.GetNameOfFocusedControl() == controlName
                && !searchItems.Started
                && searchItems.SourceGenerator != null
                && searchItems.DebounceSearchTime > EditorApplication.timeSinceStartup)
            {
                searchItems.DebounceSearchTime = EditorApplication.timeSinceStartup - 1d;
                CompleteSearchImmediately(context);
                GUI.changed = true;
            }

            string activeText = isKeySearch ? searchItems.KeySearchText : searchItems.ValueSearchText;
            if (string.IsNullOrEmpty(activeText))
            {
                EditorGUI.LabelField(new Rect(rect)
                {
                    width = Mathf.Max(0f, rect.width - 6f),
                }, placeholder, PlaceholderStyle);
            }
        }

        private static void EnsureSearchState(DictionaryContext context, IReadOnlyList<object> keys)
        {
            AsyncSearchItems searchItems = context.SearchItems;
            if (!context.ConfigurationInitialized)
            {
                context.ConfigurationInitialized = true;
                context.SearchEnabled = context.Attribute.Searchable;
                context.ObjectSearch = context.Attribute.ObjectSearch;
                searchItems.NumberOfItemsPerPage = context.Attribute.NumberOfItemsPerPage;
                SetFullResults(searchItems, keys);
                return;
            }

            if (searchItems.Size != keys.Count)
            {
                RestartSearch(context, searchItems.KeySearchText, searchItems.ValueSearchText, false);
            }
        }

        private static void RestartSearch(DictionaryContext context, string keySearchText, string valueSearchText,
            bool resetPage)
        {
            AsyncSearchItems searchItems = context.SearchItems;
            string safeKeySearch = keySearchText ?? "";
            string safeValueSearch = valueSearchText ?? "";
            object[] keys = GetDictionaryKeys(context).ToArray();

            if (resetPage)
            {
                searchItems.PageIndex = 0;
            }

            searchItems.Size = keys.Length;
            searchItems.SourceGenerator?.Dispose();
            searchItems.SourceGenerator = null;

            if (string.IsNullOrEmpty(safeKeySearch) && string.IsNullOrEmpty(safeValueSearch))
            {
                searchItems.KeySearchText = "";
                searchItems.ValueSearchText = "";
                SetFullResults(searchItems, keys);
                return;
            }

            IReadOnlyList<object> currentResults = GetCurrentResults(searchItems);
            searchItems.CachedHitKeys.Clear();
            searchItems.CachedHitKeys.AddRange(currentResults);
            searchItems.HitKeys.Clear();
            searchItems.KeySearchText = safeKeySearch;
            searchItems.ValueSearchText = safeValueSearch;
            searchItems.Started = false;
            searchItems.Finished = false;
            searchItems.DebounceSearchTime = EditorApplication.timeSinceStartup + DebounceTime;
            searchItems.SourceGenerator = Search(context, keys, safeKeySearch, safeValueSearch).GetEnumerator();
            UpdateVisibleKeys(searchItems);
        }

        private static void SetFullResults(AsyncSearchItems searchItems, IReadOnlyList<object> keys)
        {
            searchItems.Started = true;
            searchItems.Finished = true;
            searchItems.Size = keys.Count;
            searchItems.SourceGenerator?.Dispose();
            searchItems.SourceGenerator = null;
            searchItems.HitKeys.Clear();
            searchItems.HitKeys.AddRange(keys);
            searchItems.CachedHitKeys.Clear();
            searchItems.CachedHitKeys.AddRange(keys);
            UpdateVisibleKeys(searchItems);
        }

        private static void TickAsyncSearch(DictionaryContext context)
        {
            if (Event.current == null || Event.current.type != EventType.Repaint)
            {
                return;
            }

            AsyncSearchItems searchItems = context.SearchItems;
            if (!searchItems.Started
                && searchItems.SourceGenerator != null
                && EditorApplication.timeSinceStartup > searchItems.DebounceSearchTime)
            {
                searchItems.Started = true;
                UpdateVisibleKeys(searchItems);
            }

            if (!searchItems.Started || searchItems.Finished || searchItems.SourceGenerator == null)
            {
                return;
            }

            bool emptySearch = string.IsNullOrEmpty(searchItems.KeySearchText)
                               && string.IsNullOrEmpty(searchItems.ValueSearchText);
            int searchBatch = emptySearch ? int.MaxValue : 50;
            bool needRefresh = false;
            for (int searchTick = 0; searchTick < searchBatch; searchTick++)
            {
                if (searchItems.SourceGenerator.MoveNext())
                {
                    object current = searchItems.SourceGenerator.Current;
                    if (current != null)
                    {
                        searchItems.HitKeys.Add(current);
                        needRefresh = true;
                    }
                }
                else
                {
                    searchItems.Finished = true;
                    searchItems.CachedHitKeys.Clear();
                    searchItems.CachedHitKeys.AddRange(searchItems.HitKeys);
                    searchItems.SourceGenerator.Dispose();
                    searchItems.SourceGenerator = null;
                    needRefresh = true;
                    break;
                }
            }

            if (needRefresh)
            {
                searchItems.Size = GetDictionaryKeys(context).Count();
                UpdateVisibleKeys(searchItems);
                EditorWindow.focusedWindow?.Repaint();
            }
        }

        private static void CompleteSearchImmediately(DictionaryContext context)
        {
            AsyncSearchItems searchItems = context.SearchItems;
            if (searchItems.SourceGenerator == null || searchItems.Finished)
            {
                return;
            }

            searchItems.Started = true;
            while (searchItems.SourceGenerator.MoveNext())
            {
                object current = searchItems.SourceGenerator.Current;
                if (current != null)
                {
                    searchItems.HitKeys.Add(current);
                }
            }

            searchItems.Finished = true;
            searchItems.CachedHitKeys.Clear();
            searchItems.CachedHitKeys.AddRange(searchItems.HitKeys);
            searchItems.SourceGenerator.Dispose();
            searchItems.SourceGenerator = null;
            searchItems.Size = GetDictionaryKeys(context).Count();
            UpdateVisibleKeys(searchItems);
        }

        private static IReadOnlyList<object> GetCurrentResults(AsyncSearchItems searchItems) =>
            searchItems.Started ? searchItems.HitKeys : searchItems.CachedHitKeys;

        private static void UpdateVisibleKeys(AsyncSearchItems searchItems)
        {
            IReadOnlyList<object> source = GetCurrentResults(searchItems);
            int numberOfItemsPerPage = searchItems.NumberOfItemsPerPage;

            int pageCount;
            int pageIndex;
            int skipStart;
            int itemCount;
            if (numberOfItemsPerPage <= 0)
            {
                pageCount = 1;
                pageIndex = 0;
                skipStart = 0;
                itemCount = int.MaxValue;
            }
            else
            {
                pageCount = Mathf.Max(1, Mathf.CeilToInt(source.Count / (float)numberOfItemsPerPage));
                pageIndex = Mathf.Clamp(searchItems.PageIndex, 0, pageCount - 1);
                skipStart = pageIndex * numberOfItemsPerPage;
                itemCount = numberOfItemsPerPage;
            }

            searchItems.TotalPage = pageCount;
            searchItems.PageIndex = pageIndex;
            searchItems.VisibleKeys.Clear();
            searchItems.VisibleKeys.AddRange(source.Skip(skipStart).Take(itemCount));
        }

        private static IEnumerable<object> Search(DictionaryContext context, IReadOnlyList<object> keys,
            string keySearch, string valueSearch)
        {
            bool keySearchEmpty = string.IsNullOrEmpty(keySearch);
            bool valueSearchEmpty = string.IsNullOrEmpty(valueSearch);
            if (keySearchEmpty && valueSearchEmpty)
            {
                foreach (object key in keys)
                {
                    yield return key;
                }
                yield break;
            }

            IReadOnlyList<ListSearchToken> valueSearchTokens = SerializedUtils.ParseSearch(valueSearch).ToArray();
            if (keySearchEmpty)
            {
                foreach (object key in keys)
                {
                    object value = GetDictionaryValue(context, key);
                    yield return Util.SearchObjectWithTokens(value, valueSearchTokens, context.ObjectSearch)
                        ? key
                        : null;
                }
                yield break;
            }

            foreach (int index in Util.SearchArrayObjects(keys, keySearch, context.ObjectSearch))
            {
                if (index == -1)
                {
                    yield return null;
                    continue;
                }

                object key = keys[index];
                object value = GetDictionaryValue(context, key);
                yield return Util.SearchObjectWithTokens(value, valueSearchTokens, context.ObjectSearch)
                    ? key
                    : null;
            }
        }

        private static float GetDictionaryRowHeight(DictionaryContext context, object key, object dictValue)
        {
            float keyHeight = IMGUIEdit.GetPropertyHeight("", context.KeyType, key, null,
                CanEditDictionary(context) ? _ => { } : null, context.LabelGrayColor, false,
                Array.Empty<Attribute>(), context.Targets, context.RichTextTagProvider,
                GetDictionaryElementKey(context, "key", key));
            float valueHeight = IMGUIEdit.GetPropertyHeight("", context.ValueValueType, dictValue, null,
                CanEditDictionary(context) ? _ => { } : null, context.LabelGrayColor, false,
                context.AllAttributes, context.Targets, context.RichTextTagProvider,
                GetDictionaryElementKey(context, "value", key));
            return Mathf.Max(keyHeight, valueHeight, EditorGUIUtility.singleLineHeight) + 2f;
        }

        private static void DrawDictionaryTableHeader(Rect rect)
        {
            GUI.Box(rect, GUIContent.none, EditorStyles.toolbar);
            (Rect keyRect, Rect valueRect) = GetDictionaryCellRects(rect);
            EditorGUI.LabelField(ShrinkRect(keyRect, CellPadding), "Keys", EditorStyles.miniBoldLabel);
            EditorGUI.LabelField(ShrinkRect(valueRect, CellPadding), "Values", EditorStyles.miniBoldLabel);
        }

        private static void DrawDictionaryRow(Rect rect, DictionaryContext context, int index, object key,
            object dictValue)
        {
            HandleDictionaryRowSelection(rect, context, index);

            bool selected = context.SelectedIndexes.Contains(index);
            if (selected)
            {
                EditorGUI.DrawRect(rect, new Color(0.24f, 0.48f, 0.90f, 0.35f));
            }
            else if (index % 2 == 1)
            {
                EditorGUI.DrawRect(rect, EditorGUIUtility.isProSkin
                    ? new Color(1f, 1f, 1f, 0.03f)
                    : new Color(0f, 0f, 0f, 0.04f));
            }

            (Rect keyRect, Rect valueRect) = GetDictionaryCellRects(rect);
            Rect separatorRect = new Rect(keyRect)
            {
                x = keyRect.xMax,
                width = 1f,
            };
            EditorGUI.DrawRect(separatorRect, EditorGUIUtility.isProSkin
                ? new Color(1f, 1f, 1f, 0.12f)
                : new Color(0f, 0f, 0f, 0.12f));

            using (new ZeroLabelWidthScope())
            {
                IMGUIEdit.OnGUI(ShrinkRect(keyRect, CellPadding), "", context.KeyType, key, null,
                    CanEditDictionary(context) ? newKey => ChangeDictionaryKey(context, key, newKey) : null,
                    context.LabelGrayColor, false, Array.Empty<Attribute>(), context.Targets,
                    context.RichTextTagProvider, GetDictionaryElementKey(context, "key", key));

                IMGUIEdit.OnGUI(ShrinkRect(valueRect, CellPadding), "", context.ValueValueType, dictValue, null,
                    CanEditDictionary(context) ? newValue => SetDictionaryValue(context, key, newValue) : null,
                    context.LabelGrayColor, false, context.AllAttributes, context.Targets,
                    context.RichTextTagProvider, GetDictionaryElementKey(context, "value", key));
            }
        }

        private static bool CanEditDictionary(DictionaryContext context) =>
            context.SetterOrNull != null
            && context.Error == ""
            && !context.IsReadOnly
            && !RuntimeUtil.IsNull(context.RawValue)
            && (context.RawValue is IDictionary dictionary
                ? !dictionary.IsReadOnly && !dictionary.IsFixedSize
                : context.IndexerProperty?.CanWrite == true
                  && context.RemoveMethod != null
                  && context.ContainsKeyMethod != null);

        private static IEnumerable<object> GetDictionaryKeys(DictionaryContext context)
        {
            if (RuntimeUtil.IsNull(context.RawValue))
            {
                return Array.Empty<object>();
            }

            if (context.RawValue is IDictionary dictionary)
            {
                return dictionary.Keys.Cast<object>();
            }

            if (context.KeysProperty?.GetValue(context.RawValue) is IEnumerable keys)
            {
                return keys.Cast<object>();
            }

            return Array.Empty<object>();
        }

        private static object GetDictionaryValue(DictionaryContext context, object key)
        {
            if (RuntimeUtil.IsNull(context.RawValue))
            {
                return null;
            }

            if (context.RawValue is IDictionary dictionary)
            {
                return dictionary[key];
            }

            return context.IndexerProperty?.GetValue(context.RawValue, new[] { key });
        }

        private static void SetDictionaryValue(DictionaryContext context, object key, object value)
        {
            if (!CanEditDictionary(context))
            {
                return;
            }

            string error = SetDictionaryValueRaw(context, key, value);
            if (error != "")
            {
                context.Error = error;
                return;
            }

            ApplyDictionaryValue(context);
        }

        private static void ChangeDictionaryKey(DictionaryContext context, object oldKey, object newKey)
        {
            if (!CanEditDictionary(context) || RuntimeUtil.IsNull(newKey) || Util.GetIsEqual(oldKey, newKey))
            {
                return;
            }

            (string containsError, bool contains) = DictionaryContainsKey(context, newKey);
            if (containsError != "" || contains)
            {
                context.Error = containsError;
                return;
            }

            object oldValue = GetDictionaryValue(context, oldKey);
            string setError = SetDictionaryValueRaw(context, newKey, oldValue);
            if (setError != "")
            {
                context.Error = setError;
                return;
            }

            string removeError = RemoveDictionaryEntryRaw(context, oldKey);
            if (removeError != "")
            {
                context.Error = removeError;
                return;
            }

            ApplyDictionaryValue(context);
        }

        private static void DrawDictionaryFooter(Rect rect, DictionaryContext context)
        {
            int count = RuntimeUtil.IsNull(context.RawValue) ? 0 : GetDictionaryKeys(context).Count();
            AsyncSearchItems searchItems = context.SearchItems;
            bool pagingEnabled = searchItems.NumberOfItemsPerPage > 0;
            Rect addButtonRect = new Rect(rect)
            {
                x = rect.xMax - ButtonWidth,
                width = ButtonWidth,
            };
            Rect removeButtonRect = new Rect(addButtonRect)
            {
                x = addButtonRect.x - ControlGap - ButtonWidth,
            };
            Rect controlsRect = new Rect(rect)
            {
                width = Mathf.Max(0f, removeButtonRect.x - ControlGap - rect.x),
            };

            if (pagingEnabled)
            {
                Rect numberOfItemsPerPageRect = new Rect(controlsRect)
                {
                    width = PagerInputWidth,
                };
                using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                {
                    int newNumberOfItemsPerPage = EditorGUI.DelayedIntField(numberOfItemsPerPageRect,
                        GUIContent.none, searchItems.NumberOfItemsPerPage);
                    if (changed.changed)
                    {
                        searchItems.NumberOfItemsPerPage = Mathf.Max(newNumberOfItemsPerPage, 0);
                        searchItems.PageIndex = 0;
                        RefreshView(context);
                    }
                }

                Rect numberOfItemsSeparatorRect = new Rect(numberOfItemsPerPageRect)
                {
                    x = numberOfItemsPerPageRect.xMax,
                    width = PagerSeparatorWidth,
                };
                EditorGUI.LabelField(numberOfItemsSeparatorRect, "/");

                Rect totalItemsRect = new Rect(numberOfItemsSeparatorRect)
                {
                    x = numberOfItemsSeparatorRect.xMax,
                    width = PagerItemsLabelWidth,
                };
                using (new EditorGUI.DisabledScope(!CanEditDictionary(context)))
                using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                {
                    int newCount = EditorGUI.DelayedIntField(totalItemsRect, GUIContent.none, count);
                    if (changed.changed)
                    {
                        ChangeDictionarySize(context, Mathf.Max(0, newCount));
                        return;
                    }
                }
                EditorGUI.LabelField(totalItemsRect, "Items", PlaceholderStyle);

                Rect previousPageRect = new Rect(totalItemsRect)
                {
                    x = totalItemsRect.xMax,
                    width = PagerButtonWidth,
                };
                using (new EditorGUI.DisabledScope(searchItems.PageIndex <= 0))
                {
                    context.LeftIcon ??= Util.LoadResource<Texture2D>("classic-dropdown-left.png");
                    if (GUI.Button(previousPageRect, context.LeftIcon, EditorStyles.miniButtonLeft))
                    {
                        searchItems.PageIndex = Mathf.Max(0, searchItems.PageIndex - 1);
                        RefreshView(context);
                    }
                }

                Rect pageRect = new Rect(previousPageRect)
                {
                    x = previousPageRect.xMax,
                    width = PagerInputWidth,
                };
                using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                {
                    int newPageIndex = EditorGUI.DelayedIntField(pageRect, GUIContent.none,
                        searchItems.PageIndex + 1) - 1;
                    if (changed.changed)
                    {
                        searchItems.PageIndex = Mathf.Max(newPageIndex, 0);
                        RefreshView(context);
                    }
                }

                Rect totalPageRect = new Rect(pageRect)
                {
                    x = pageRect.xMax,
                    width = PagerPageLabelWidth,
                };
                EditorGUI.LabelField(totalPageRect, $"/ {searchItems.TotalPage}");

                Rect nextPageRect = new Rect(totalPageRect)
                {
                    x = totalPageRect.xMax,
                    width = PagerButtonWidth,
                };
                using (new EditorGUI.DisabledScope(searchItems.PageIndex >= searchItems.TotalPage - 1))
                {
                    context.RightIcon ??= Util.LoadResource<Texture2D>("classic-dropdown-right.png");
                    if (GUI.Button(nextPageRect, context.RightIcon, EditorStyles.miniButtonRight))
                    {
                        searchItems.PageIndex = Mathf.Min(searchItems.PageIndex + 1, searchItems.TotalPage - 1);
                        RefreshView(context);
                    }
                }
            }
            else
            {
                Rect sizeRect = new Rect(controlsRect)
                {
                    width = SizeWidth,
                };
                Rect labelRect = new Rect(controlsRect)
                {
                    x = sizeRect.xMax + ControlGap,
                    width = Mathf.Max(0f, controlsRect.xMax - sizeRect.xMax - ControlGap),
                };
                using (new EditorGUI.DisabledScope(!CanEditDictionary(context)))
                using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                {
                    int newSize = EditorGUI.DelayedIntField(sizeRect, count);
                    if (changed.changed)
                    {
                        ChangeDictionarySize(context, Mathf.Max(0, newSize));
                    }
                }
                EditorGUI.LabelField(labelRect, "Items", EditorStyles.miniLabel);
            }

            using (new EditorGUI.DisabledScope(!CanEditDictionary(context) || count == 0))
            {
                if (GUI.Button(removeButtonRect, "-", EditorStyles.miniButtonLeft))
                {
                    RemoveSelectedDictionaryEntries(context);
                }
            }

            using (new EditorGUI.DisabledScope(!CanEditDictionary(context) || context.AddPanelOpen))
            {
                if (GUI.Button(addButtonRect, "+", EditorStyles.miniButtonRight))
                {
                    OpenAddDictionaryPanel(context);
                }
            }
        }

        private static float GetDictionaryAddPanelHeight(DictionaryContext context, float width)
        {
            float contentWidth = Mathf.Max(0f, width - AddPanelPadding * 2);
            float height = AddPanelPadding * 2;
            height += IMGUIEdit.GetPropertyHeight("Key", context.KeyType, context.AddKey, null, _ => { },
                context.LabelGrayColor, context.InHorizontalLayout, Array.Empty<Attribute>(), context.Targets,
                context.RichTextTagProvider, $"{context.Key}.add.key");
            height += IMGUIEdit.GetPropertyHeight("Value", context.ValueValueType, context.AddValue, null, _ => { },
                context.LabelGrayColor, context.InHorizontalLayout, context.AllAttributes, context.Targets,
                context.RichTextTagProvider, $"{context.Key}.add.value");
            if (context.AddError != "")
            {
                height += ImGuiHelpBox.GetHeight(context.AddError, contentWidth, MessageType.Error);
            }

            height += EditorGUIUtility.singleLineHeight;
            return height;
        }

        private static void DrawDictionaryAddPanel(Rect rect, DictionaryContext context)
        {
            GUI.Box(rect, GUIContent.none, EditorStyles.helpBox);
            Rect leftRect = ShrinkRect(rect, AddPanelPadding);

            float keyHeight = IMGUIEdit.GetPropertyHeight("Key", context.KeyType, context.AddKey, null, _ => { },
                context.LabelGrayColor, context.InHorizontalLayout, Array.Empty<Attribute>(), context.Targets,
                context.RichTextTagProvider, $"{context.Key}.add.key");
            (Rect keyRect, Rect afterKeyRect) = RectUtils.SplitHeightRect(leftRect, keyHeight);
            IMGUIEdit.OnGUI(keyRect, "Key", context.KeyType, context.AddKey, null, newKey =>
            {
                context.AddKey = newKey;
                context.AddError = GetAddKeyError(context);
            }, context.LabelGrayColor, context.InHorizontalLayout, Array.Empty<Attribute>(), context.Targets,
                context.RichTextTagProvider, $"{context.Key}.add.key");

            float valueHeight = IMGUIEdit.GetPropertyHeight("Value", context.ValueValueType, context.AddValue, null,
                _ => { }, context.LabelGrayColor, context.InHorizontalLayout, context.AllAttributes, context.Targets,
                context.RichTextTagProvider, $"{context.Key}.add.value");
            (Rect valueRect, Rect afterValueRect) = RectUtils.SplitHeightRect(afterKeyRect, valueHeight);
            IMGUIEdit.OnGUI(valueRect, "Value", context.ValueValueType, context.AddValue, null,
                newValue => context.AddValue = newValue, context.LabelGrayColor, context.InHorizontalLayout,
                context.AllAttributes, context.Targets, context.RichTextTagProvider, $"{context.Key}.add.value");

            leftRect = afterValueRect;
            if (context.AddError != "")
            {
                (Rect errorRect, Rect afterErrorRect) = RectUtils.SplitHeightRect(leftRect,
                    ImGuiHelpBox.GetHeight(context.AddError, leftRect.width, MessageType.Error));
                ImGuiHelpBox.Draw(errorRect, context.AddError, MessageType.Error);
                leftRect = afterErrorRect;
            }

            (Rect buttonRect, _) = RectUtils.SplitHeightRect(leftRect, EditorGUIUtility.singleLineHeight);
            (Rect okRect, Rect cancelRect) = RectUtils.SplitWidthRect(buttonRect, buttonRect.width * 0.5f);
            okRect.width = Mathf.Max(0f, okRect.width - 1f);
            cancelRect.x += 1f;
            cancelRect.width = Mathf.Max(0f, cancelRect.width - 1f);

            using (new EditorGUI.DisabledScope(context.AddError != ""))
            {
                if (GUI.Button(okRect, "OK"))
                {
                    ConfirmAddDictionaryEntry(context);
                }
            }

            if (GUI.Button(cancelRect, "Cancel"))
            {
                context.AddPanelOpen = false;
            }
        }

        private static void OpenAddDictionaryPanel(DictionaryContext context)
        {
            if (!CanEditDictionary(context))
            {
                return;
            }

            context.AddKey = CreateDictionaryPanelDefaultValue(context.KeyType);
            context.AddValue = CreateDictionaryPanelDefaultValue(context.ValueValueType);
            context.AddPanelOpen = true;
            context.AddError = GetAddKeyError(context);
        }

        private static void ConfirmAddDictionaryEntry(DictionaryContext context)
        {
            context.AddError = GetAddKeyError(context);
            if (context.AddError != "")
            {
                return;
            }

            string error = SetDictionaryValueRaw(context, context.AddKey, context.AddValue);
            if (error != "")
            {
                context.AddError = error;
                return;
            }

            context.AddPanelOpen = false;
            context.SelectedIndexes.Clear();
            ApplyDictionaryValue(context);
            GUI.changed = true;
        }

        private static string GetAddKeyError(DictionaryContext context)
        {
            if (RuntimeUtil.IsNull(context.AddKey))
            {
                return "Key can not be null.";
            }

            (string error, bool contains) = DictionaryContainsKey(context, context.AddKey);
            if (error != "")
            {
                return error;
            }

            return contains ? "Key already exists." : "";
        }

        private static object CreateDictionaryPanelDefaultValue(Type type) =>
            type?.IsValueType == true ? Activator.CreateInstance(type) : null;

        private static void ChangeDictionarySize(DictionaryContext context, int newSize)
        {
            if (!CanEditDictionary(context))
            {
                return;
            }

            object[] keys = GetDictionaryKeys(context).ToArray();
            if (newSize == keys.Length)
            {
                return;
            }

            if (newSize > keys.Length)
            {
                OpenAddDictionaryPanel(context);
                return;
            }

            RemoveDictionaryEntries(context, keys.Skip(newSize));
        }

        private static void RemoveSelectedDictionaryEntries(DictionaryContext context)
        {
            object[] keys = GetDictionaryKeys(context).ToArray();
            if (keys.Length == 0)
            {
                return;
            }

            List<int> removeIndexes = context.SelectedIndexes
                .Where(each => each >= 0 && each < keys.Length)
                .OrderByDescending(each => each)
                .ToList();
            if (removeIndexes.Count == 0)
            {
                removeIndexes.Add(keys.Length - 1);
            }

            RemoveDictionaryEntries(context, removeIndexes.Select(index => keys[index]));
        }

        private static void RemoveDictionaryEntries(DictionaryContext context, IEnumerable<object> keys)
        {
            if (!CanEditDictionary(context))
            {
                return;
            }

            string error = "";
            foreach (object key in keys.ToArray())
            {
                error = RemoveDictionaryEntryRaw(context, key);
                if (error != "")
                {
                    break;
                }
            }

            if (error != "")
            {
                context.Error = error;
                return;
            }

            context.SelectedIndexes.Clear();
            ApplyDictionaryValue(context);
            GUI.changed = true;
        }

        private static (string error, bool contains) DictionaryContainsKey(DictionaryContext context, object key)
        {
            try
            {
                if (context.RawValue is IDictionary dictionary)
                {
                    return ("", dictionary.Contains(key));
                }

                if (context.ContainsKeyMethod == null)
                {
                    return ("Dictionary value edit requires a ContainsKey method.", false);
                }

                return ("", (bool)context.ContainsKeyMethod.Invoke(context.RawValue, new[] { key }));
            }
            catch (Exception e)
            {
                return (GetExceptionMessage(e), false);
            }
        }

        private static string SetDictionaryValueRaw(DictionaryContext context, object key, object value)
        {
            try
            {
                if (context.RawValue is IDictionary dictionary)
                {
                    dictionary[key] = value;
                    return "";
                }

                if (context.IndexerProperty == null)
                {
                    return "Dictionary value edit requires an indexer property.";
                }

                context.IndexerProperty.SetValue(context.RawValue, value, new[] { key });
                return "";
            }
            catch (Exception e)
            {
                return GetExceptionMessage(e);
            }
        }

        private static string RemoveDictionaryEntryRaw(DictionaryContext context, object key)
        {
            try
            {
                if (context.RawValue is IDictionary dictionary)
                {
                    dictionary.Remove(key);
                    return "";
                }

                if (context.RemoveMethod == null)
                {
                    return "Dictionary value edit requires a Remove method.";
                }

                context.RemoveMethod.Invoke(context.RawValue, new[] { key });
                return "";
            }
            catch (Exception e)
            {
                return GetExceptionMessage(e);
            }
        }

        private static void HandleDictionaryRowSelection(Rect rect, DictionaryContext context, int index)
        {
            Event current = Event.current;
            if (current.type != EventType.MouseDown || current.button != 0 || !rect.Contains(current.mousePosition))
            {
                return;
            }

            if (current.control || current.command)
            {
                if (context.SelectedIndexes.Contains(index))
                {
                    context.SelectedIndexes.Remove(index);
                }
                else
                {
                    context.SelectedIndexes.Add(index);
                }
            }
            else
            {
                context.SelectedIndexes.Clear();
                context.SelectedIndexes.Add(index);
            }

            GUI.changed = true;
        }

        private static void ClampDictionarySelection(DictionaryContext context, int count)
        {
            context.SelectedIndexes.RemoveWhere(each => each < 0 || each >= count);
        }

        private static (Rect keyRect, Rect valueRect) GetDictionaryCellRects(Rect rect)
        {
            float keyWidth = Mathf.Max(40f, rect.width * 0.5f);
            Rect keyRect = new Rect(rect)
            {
                width = Mathf.Min(keyWidth, rect.width),
            };
            Rect valueRect = new Rect(rect)
            {
                x = keyRect.xMax + 1f,
                width = Mathf.Max(0f, rect.width - keyRect.width - 1f),
            };
            return (keyRect, valueRect);
        }

        private static Rect ShrinkRect(Rect rect, float padding) => new Rect(rect)
        {
            x = rect.x + padding,
            y = rect.y + padding,
            width = Mathf.Max(0f, rect.width - padding * 2),
            height = Mathf.Max(0f, rect.height - padding * 2),
        };

        private static string GetDictionaryElementKey(DictionaryContext context, string part, object key) =>
            $"{context.Key}.{part}.{key?.GetHashCode() ?? 0}";

        private static string GetExceptionMessage(Exception exception) =>
            exception is TargetInvocationException { InnerException: { } innerException }
                ? innerException.Message
                : exception.Message;

        private static GUIStyle PlaceholderStyle => new GUIStyle("label")
        {
            alignment = TextAnchor.MiddleRight,
            normal = { textColor = Color.gray },
            fontStyle = FontStyle.Italic,
        };

        private sealed class ZeroLabelWidthScope : IDisposable
        {
            private readonly float _oldLabelWidth;

            public ZeroLabelWidthScope()
            {
                _oldLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 1f;
            }

            public void Dispose()
            {
                EditorGUIUtility.labelWidth = _oldLabelWidth;
            }
        }

        private static void ApplyDictionaryValue(DictionaryContext context)
        {
            context.BeforeSet?.Invoke(context.RawValue);
            context.SetterOrNull?.Invoke(context.RawValue);
            RestartSearch(context, context.SearchItems.KeySearchText, context.SearchItems.ValueSearchText, false);
            GUI.changed = true;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.ArraySizeDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using SaintsField.Utils;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace SaintsField.Editor.Playa.Renderer.ListDrawerSettings
{
    public partial class ListDrawerSettingsRenderer
    {
        private IMGUILoading _imguiLoading = new IMGUILoading();
        private RichTextDrawer _richTextDrawer;

        ~ListDrawerSettingsRenderer()
        {
            _richTextDrawer = null;
        }


        public override void OnDestroyIMGUI()
        {
            if (_asyncSearchItems?.SourceGenerator != null)
            {
                _asyncSearchItems.SourceGenerator.Dispose();
                _asyncSearchItems.SourceGenerator = null;
            }
        }

        private class UnsetGuiStyleFixedHeight : IDisposable
        {
            private readonly GUIStyle _guiStyle;
            private readonly float _oldValue;

            public UnsetGuiStyleFixedHeight(GUIStyle guiStyle)
            {
                _guiStyle = guiStyle;
                _oldValue = guiStyle.fixedHeight;
                _guiStyle.fixedHeight = 0;
            }

            public void Dispose()
            {
                _guiStyle.fixedHeight = _oldValue;
            }
        }

        private struct PagingInfo
        {
            public IReadOnlyList<int> IndexesAfterSearch;
            public List<int> IndexesCurPage;
            public int CurPageIndex;
            public int PageCount;
        }

        private class ImGuiListInfo
        {
            public SerializedProperty Property;
            public PreCheckResult PreCheckResult;

            public bool HasSearch;
            public bool HasPaging;
            public PagingInfo PagingInfo;
            public string SearchText = string.Empty;
            public int PageIndex;
            public int NumberOfItemsPrePage;
        }

        private ReorderableList _imGuiReorderableList;
        private ImGuiListInfo _imGuiListInfo;

        private string _curXml;
        private RichTextDrawer.RichTextChunk[] _curXmlChunks;

        private IEnumerable<IReadOnlyList<int>> SearchCallback(SerializedProperty arrayProperty, string search,
            ListDrawerSettingsAttribute listDrawerSettingsAttribute)
        {
            const int batchLimit = 10;

            Type elementType = ReflectUtils.GetElementType(FieldWithInfo.FieldInfo?.FieldType ?? FieldWithInfo.PropertyInfo.PropertyType);
            string extraSearchCallback = listDrawerSettingsAttribute.ExtraSearch;
            string overrideSearchCallback = listDrawerSettingsAttribute.OverrideSearch;

            (MethodInfo methodInfo, ParamType paramType) extraSearchMethod = default;
            (MethodInfo methodInfo, ParamType paramType) overrideSearchMethod = default;

            if (!string.IsNullOrEmpty(extraSearchCallback))
            {
                extraSearchMethod = GetSearchMethodInfo(FieldWithInfo.Targets[0].GetType(), elementType, extraSearchCallback);
            }

            if (!string.IsNullOrEmpty(overrideSearchCallback))
            {
                overrideSearchMethod = GetSearchMethodInfo(FieldWithInfo.Targets[0].GetType(), elementType, overrideSearchCallback);
            }

            IReadOnlyList<ListSearchToken> searchTokens = SerializedUtils.ParseSearch(search).ToList();
            IEnumerable rawValueList = (IEnumerable)(FieldWithInfo.FieldInfo != null
                ? FieldWithInfo.FieldInfo.GetValue(FieldWithInfo.Targets[0])
                : FieldWithInfo.PropertyInfo.GetValue(FieldWithInfo.Targets[0]));

            if (overrideSearchMethod.methodInfo != null)
            {
                if (overrideSearchMethod.paramType == ParamType.Index)
                {
                    List<int> batchResults = new List<int>();
                    int batchCount = 0;
                    foreach (int fullIndex in Enumerable.Range(0, arrayProperty.arraySize))
                    {
                        if ((bool)overrideSearchMethod.methodInfo.Invoke(FieldWithInfo.Targets[0],
                                new object[] { fullIndex, searchTokens }))
                        {
                            batchResults.Add(fullIndex);
                        }

                        batchCount++;
                        if (batchCount / batchLimit >= 1)
                        {
                            yield return batchResults.ToArray();
                            batchCount = 0;
                            batchResults.Clear();
                        }
                    }

                    if (batchResults.Count > 0)
                    {
                        yield return batchResults;
                    }

                    yield break;
                }

                {
                    int curIndex = 0;
                    List<int> batchResults = new List<int>();
                    int batchCount = 0;

                    foreach (object rawValue in rawValueList)
                    {
                        object[] methodParams = overrideSearchMethod.paramType == ParamType.Target
                            ? new[] { rawValue, searchTokens }
                            : new[] { rawValue, curIndex, searchTokens };

                        if ((bool)overrideSearchMethod.methodInfo.Invoke(FieldWithInfo.Targets[0], methodParams))
                        {
                            batchResults.Add(curIndex);
                        }

                        curIndex++;
                        batchCount++;
                        if (batchCount / batchLimit >= 1)
                        {
                            yield return batchResults.ToArray();
                            batchCount = 0;
                            batchResults.Clear();
                        }
                    }

                    if (batchResults.Count > 0)
                    {
                        yield return batchResults;
                    }

                    yield break;
                }
            }

            if (extraSearchMethod.methodInfo != null)
            {
                if (extraSearchMethod.paramType == ParamType.Index)
                {
                    List<int> batchResults = new List<int>();
                    int batchCount = 0;

                    foreach (int fullIndex in Enumerable.Range(0, arrayProperty.arraySize))
                    {
                        if ((bool)extraSearchMethod.methodInfo.Invoke(FieldWithInfo.Targets[0],
                                new object[] { fullIndex, searchTokens }))
                        {
                            batchResults.Add(fullIndex);
                        }
                        else
                        {
                            SerializedProperty itemProp = arrayProperty.GetArrayElementAtIndex(fullIndex);
                            HashSet<object>[] searchedObjectsArray = Enumerable.Range(0, searchTokens.Count)
                                .Select(_ => new HashSet<object>())
                                .ToArray();
                            bool all = true;
                            for (int index = 0; index < searchTokens.Count; index++)
                            {
                                ListSearchToken token = searchTokens[index];
                                HashSet<object> searchedObject = searchedObjectsArray[index];
                                if (!SerializedUtils.SearchProp(itemProp, token.Token, searchedObject))
                                {
                                    all = false;
                                    break;
                                }
                            }

                            if (all)
                            {
                                batchResults.Add(fullIndex);
                            }
                        }

                        batchCount++;
                        if (batchCount / batchLimit >= 1)
                        {
                            yield return batchResults.ToArray();
                            batchCount = 0;
                            batchResults.Clear();
                        }
                    }

                    if (batchResults.Count > 0)
                    {
                        yield return batchResults;
                    }

                    yield break;
                }

                {
                    int curIndex = 0;
                    List<int> batchResults = new List<int>();
                    int batchCount = 0;
                    foreach (object rawValue in rawValueList)
                    {
                        object[] methodParams = extraSearchMethod.paramType == ParamType.Target
                            ? new[] { rawValue, searchTokens }
                            : new[] { rawValue, curIndex, searchTokens };

                        if ((bool)extraSearchMethod.methodInfo.Invoke(FieldWithInfo.Targets[0], methodParams))
                        {
                            batchResults.Add(curIndex);
                        }
                        else
                        {
                            SerializedProperty itemProp = arrayProperty.GetArrayElementAtIndex(curIndex);
                            HashSet<object>[] searchedObjectsArray = Enumerable.Range(0, searchTokens.Count)
                                .Select(_ => new HashSet<object>())
                                .ToArray();

                            bool all = true;
                            for (int index = 0; index < searchTokens.Count; index++)
                            {
                                ListSearchToken token = searchTokens[index];
                                HashSet<object> searchedObjects = searchedObjectsArray[index];
                                if (!SerializedUtils.SearchProp(itemProp, token.Token, searchedObjects))
                                {
                                    all = false;
                                    break;
                                }
                            }

                            if (all)
                            {
                                batchResults.Add(curIndex);
                            }
                        }

                        curIndex++;
                        batchCount++;
                        if (batchCount / batchLimit >= 1)
                        {
                            yield return batchResults.ToArray();
                            batchCount = 0;
                            batchResults.Clear();
                        }
                    }

                    if (batchResults.Count > 0)
                    {
                        yield return batchResults;
                    }

                    yield break;
                }
            }

            List<int> defaultBatchResults = new List<int>();
            int defaultBatchCount = 0;
            foreach (int i in SerializedUtils.SearchArrayProperty(arrayProperty, search))
            {
                if (i != -1)
                {
                    defaultBatchResults.Add(i);
                }

                defaultBatchCount++;
                if (defaultBatchCount / batchLimit >= 1)
                {
                    yield return defaultBatchResults.ToArray();
                    defaultBatchCount = 0;
                    defaultBatchResults.Clear();
                }
            }

            if (defaultBatchResults.Count > 0)
            {
                yield return defaultBatchResults;
            }
        }

        private void UpdatePage(SerializedProperty property, int newPageIndex, int numberOfItemsPerPage,
            ListDrawerSettingsAttribute listDrawerSettingsAttribute)
        {
            string searchText = _imGuiListInfo.SearchText;
            List<int> resultIndexes;
            if (string.IsNullOrWhiteSpace(searchText))
            {
                resultIndexes = Enumerable.Range(0, property.arraySize).ToList();
                _asyncSearchItems.Started = true;
                _asyncSearchItems.Finished = true;
                _asyncSearchItems.CachedFullSources = new List<int>(resultIndexes);
                _asyncSearchItems.FullSources = new List<int>(resultIndexes);
                _asyncSearchItems.SearchText = "";
                if (_asyncSearchItems.SourceGenerator != null)
                {
                    _asyncSearchItems.SourceGenerator.Dispose();
                    _asyncSearchItems.SourceGenerator = null;
                }
            }
            else if (_asyncSearchItems.SearchText == searchText)
            {
                resultIndexes = !_asyncSearchItems.Started && !_asyncSearchItems.Finished
                    ? _asyncSearchItems.CachedFullSources
                    : _asyncSearchItems.FullSources;
            }
            else
            {
                _asyncSearchItems.SearchText = searchText;
                _asyncSearchItems.DebounceSearchTime = EditorApplication.timeSinceStartup + 0.6f;
                _asyncSearchItems.Started = false;
                _asyncSearchItems.Finished = false;
                _asyncSearchItems.FullSources.Clear();
                if (_asyncSearchItems.SourceGenerator != null)
                {
                    _asyncSearchItems.SourceGenerator.Dispose();
                    _asyncSearchItems.SourceGenerator = null;
                }

                _asyncSearchItems.SourceGenerator = SearchCallback(property, searchText, listDrawerSettingsAttribute).GetEnumerator();
                resultIndexes = _asyncSearchItems.CachedFullSources;
            }

            PagingInfo newPagingInfo = GetPagingInfo(newPageIndex, resultIndexes, numberOfItemsPerPage);
            if (_imGuiReorderableList != null && !newPagingInfo.IndexesCurPage.SequenceEqual(_imGuiListInfo.PagingInfo.IndexesCurPage))
            {
                _imGuiReorderableList = null;
            }

            _imGuiListInfo.PagingInfo = newPagingInfo;
            _imGuiListInfo.PageIndex = newPagingInfo.CurPageIndex;
            _imGuiListInfo.NumberOfItemsPrePage = Mathf.Max(numberOfItemsPerPage, 0);
            _asyncSearchItems.CurPageIndex = newPagingInfo.CurPageIndex;
            _asyncSearchItems.ItemIndexToPropertyIndex.Clear();
            _asyncSearchItems.ItemIndexToPropertyIndex.AddRange(newPagingInfo.IndexesCurPage);
        }

        private void TickAsyncSearch(SerializedProperty property, ListDrawerSettingsAttribute listDrawerSettingsAttribute)
        {
            if (_asyncSearchItems == null)
            {
                return;
            }

            if (Event.current == null || Event.current.type != EventType.Repaint)
            {
                return;
            }

            if (!_asyncSearchItems.Started && _asyncSearchItems.SourceGenerator != null &&
                EditorApplication.timeSinceStartup > _asyncSearchItems.DebounceSearchTime)
            {
                _asyncSearchItems.Started = true;
                UpdatePage(property, _imGuiListInfo.PageIndex, _imGuiListInfo.NumberOfItemsPrePage, listDrawerSettingsAttribute);
            }

            if (_asyncSearchItems.Started && !_asyncSearchItems.Finished && _asyncSearchItems.SourceGenerator != null)
            {
                if (_asyncSearchItems.SourceGenerator.MoveNext())
                {
                    IReadOnlyList<int> currentValue = _asyncSearchItems.SourceGenerator.Current;
                    if (currentValue != null && currentValue.Count > 0)
                    {
                        _asyncSearchItems.FullSources.AddRange(currentValue);
                        UpdatePage(property, _imGuiListInfo.PageIndex, _imGuiListInfo.NumberOfItemsPrePage, listDrawerSettingsAttribute);
                    }
                }
                else
                {
                    _asyncSearchItems.Finished = true;
                    _asyncSearchItems.SourceGenerator.Dispose();
                    _asyncSearchItems.SourceGenerator = null;
                }
            }
        }

        private void SetArraySize(SerializedProperty property, int newSize, ArraySizeAttribute arraySizeAttribute,
            ListDrawerSettingsAttribute listDrawerSettingsAttribute)
        {
            int min = -1;
            int max = -1;
            if (arraySizeAttribute != null)
            {
                (string error, bool _, int minValue, int maxValue) = ArraySizeAttributeDrawer.GetMinMax(arraySizeAttribute,
                    FieldWithInfo.SerializedProperty,
                    FieldWithInfo.FieldInfo, FieldWithInfo.Targets[0]);

                if (error == "")
                {
                    min = minValue;
                    max = maxValue;
                }
            }

            if (min > 0 && newSize < min)
            {
                newSize = min;
            }
            else if (max > 0 && newSize > max)
            {
                newSize = max;
            }

            if (property.arraySize != newSize)
            {
                property.arraySize = newSize;
                property.serializedObject.ApplyModifiedProperties();
            }

            UpdatePage(property, _imGuiListInfo.PageIndex, _imGuiListInfo.NumberOfItemsPrePage, listDrawerSettingsAttribute);
        }

        private void DrawListDrawerSettingsField(SerializedProperty property, Rect position, ArraySizeAttribute arraySizeAttribute,
            ListDrawerSettingsAttribute listDrawerSettingsAttribute)
        {
            int min = -1;
            int max = -1;
            bool dynamic = true;
            if (arraySizeAttribute != null)
            {
                (string error, bool dynamicValue, int minValue, int maxValue) = ArraySizeAttributeDrawer.GetMinMax(arraySizeAttribute,
                    FieldWithInfo.SerializedProperty,
                    FieldWithInfo.FieldInfo, FieldWithInfo.Targets[0]);

                if (error == "")
                {
                    min = minValue;
                    max = maxValue;
                    dynamic = dynamicValue;
                }
            }

            Rect usePosition = new Rect(position)
            {
                y = position.y + 1,
                height = position.height - 2,
            };

            if (!property.isExpanded)
            {
                if (Event.current.type == EventType.Repaint)
                {
                    GUIStyle headerBackground = "RL Header";
                    using (new UnsetGuiStyleFixedHeight(headerBackground))
                    {
                        headerBackground.Draw(usePosition, false, false, false, false);
                    }
                }

                Rect paddingTitle = new Rect(usePosition)
                {
                    x = usePosition.x + 6,
                    y = usePosition.y + 1,
                    height = usePosition.height - 2,
                    width = usePosition.width - 12,
                };

                Type elementType = ReflectUtils.GetElementType(FieldWithInfo.FieldInfo?.FieldType ?? FieldWithInfo.PropertyInfo.PropertyType);

                DrawListDrawerHeader(paddingTitle, elementType, property, arraySizeAttribute, listDrawerSettingsAttribute);
                return;
            }

            TickAsyncSearch(property, listDrawerSettingsAttribute);
            UpdatePage(property, _imGuiListInfo.PageIndex, _imGuiListInfo.NumberOfItemsPrePage, listDrawerSettingsAttribute);

            if (_imGuiReorderableList == null)
            {
                _imGuiReorderableList = new ReorderableList(property.serializedObject, property, true, true, true, true)
                    {
                        headerHeight = SaintsPropertyDrawer.SingleLineHeight * ((_imGuiListInfo.HasPaging || _imGuiListInfo.HasSearch)? 2: 1),
                    };

                Type elementType = ReflectUtils.GetElementType(FieldWithInfo.FieldInfo?.FieldType ?? FieldWithInfo.PropertyInfo.PropertyType);
                _imGuiReorderableList.drawHeaderCallback += v => DrawListDrawerHeader(v, elementType, property,
                    arraySizeAttribute, listDrawerSettingsAttribute);
                _imGuiReorderableList.elementHeightCallback += DrawListDrawerItemHeight;
                _imGuiReorderableList.drawElementCallback += DrawListDrawerItem;

                if(arraySizeAttribute != null)
                {
                    if (dynamic)
                    {
                        _imGuiReorderableList.onCanRemoveCallback += r =>
                        {
                            (string removeError, bool _, int removeMin, int _) = ArraySizeAttributeDrawer.GetMinMax(arraySizeAttribute, FieldWithInfo.SerializedProperty,
                                FieldWithInfo.FieldInfo, FieldWithInfo.Targets[0]);
                            if (removeError != "")
                            {
                                return true;
                            }
                            bool canRemove = r.count > removeMin;
                            // Debug.Log($"canRemove={canRemove}, count={r.count}, min={arraySizeAttribute.Min}");
                            return canRemove;
                        };
                        _imGuiReorderableList.onCanAddCallback += r =>
                        {
                            (string addError, bool _, int _, int addMax) = ArraySizeAttributeDrawer.GetMinMax(arraySizeAttribute, FieldWithInfo.SerializedProperty,
                                FieldWithInfo.FieldInfo, FieldWithInfo.Targets[0]);
                            if (addError != "")
                            {
                                return true;
                            }
                            bool canAdd = r.count < addMax;
                            // Debug.Log($"canAdd={canAdd}, count={r.count}, max={arraySizeAttribute.Max}");
                            return canAdd;
                        };
                    }
                    else
                    {
                        if(min > 0)
                        {
                            _imGuiReorderableList.onCanRemoveCallback += r =>
                            {
                                bool canRemove = r.count > min;
                                // Debug.Log($"canRemove={canRemove}, count={r.count}, min={arraySizeAttribute.Min}");
                                return canRemove;
                            };
                        }
                        if (max > 0)
                        {
                            _imGuiReorderableList.onCanAddCallback += r =>
                            {
                                bool canAdd = r.count < max;
                                // Debug.Log($"canAdd={canAdd}, count={r.count}, max={arraySizeAttribute.Max}");
                                return canAdd;
                            };
                        }
                    }
                    // _imGuiReorderableList.onCanAddCallback += _ => !(arraySizeAttribute.Min >= 0 && property.arraySize <= arraySizeAttribute.Min);
                }
            }

            // Debug.Log(ReorderableList.defaultBehaviours);
            // Debug.Log(ReorderableList.defaultBehaviours.headerBackground);

            using(new UnsetGuiStyleFixedHeight("RL Header"))
            {
                try
                {
                    _imGuiReorderableList.DoList(usePosition);
                }
                catch (ObjectDisposedException)
                {
                    _imGuiReorderableList = null;
                }
                catch (NullReferenceException)
                {
                    _imGuiReorderableList = null;
                }
            }
        }

        private Texture2D _iconDown;
        private Texture2D _iconLeft;
        private Texture2D _iconRight;

        private void DrawListDrawerHeader(Rect rect, Type elementType, SerializedProperty property,
            ArraySizeAttribute arraySizeAttribute, ListDrawerSettingsAttribute listDrawerSettingsAttribute)
        {
            int min = -1;
            int max = -1;
            // bool dynamic = true;
            if (arraySizeAttribute != null)
            {
                (string error, bool _, int minValue, int maxValue) = ArraySizeAttributeDrawer.GetMinMax(arraySizeAttribute,
                    FieldWithInfo.SerializedProperty,
                    FieldWithInfo.FieldInfo, FieldWithInfo.Targets[0]);

                if (error == "")
                {
                    min = minValue;
                    max = maxValue;
                    // dynamic = dynamicValue;
                }
            }

            DragAndDropImGui(rect, elementType, property);

            // const float twoNumberInputWidth = 20;
            const float inputWidth = 30;
            // const float itemsLabelWidth = 75;
            const float itemsLabelWidth = 65;
            const float buttonWidth = 19;
            // const float pagingLabelWidth = 35;
            const float pagingLabelWidth = 30;
            const float pagingSepWidth = 8;

            const float gap = 5;

            (Rect titleRect, Rect controlRect) = RectUtils.SplitHeightRect(rect, EditorGUIUtility.singleLineHeight);
            controlRect.height -= 1;

            (Rect titleFoldRect, Rect titleButtonRect) = RectUtils.SplitWidthRect(titleRect, 16);

            if (!_imGuiListInfo.HasPaging && !_imGuiListInfo.HasSearch)  // draw element count container
            {
                (Rect titleButtonNewRect, Rect titleItemTotalRect) =
                    RectUtils.SplitWidthRect(titleButtonRect, titleButtonRect.width - 50);
                titleItemTotalRect.y += 1;
                titleItemTotalRect.height -= 2;

                using(new EditorGUI.DisabledScope(min > 0 && min == max))
                using(EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                    {
                    int newCount = EditorGUI.DelayedIntField(titleItemTotalRect, GUIContent.none, property.arraySize);
                    if (changed.changed)
                    {
                        SetArraySize(property, newCount, arraySizeAttribute, listDrawerSettingsAttribute);
                        return;
                    }
                }

                titleButtonRect = titleButtonNewRect;
            }

            if(GUI.Button(titleButtonRect, "", GUIStyle.none))
            {
                _imGuiListInfo.Property.isExpanded = !_imGuiListInfo.Property.isExpanded;
                return;
            }

            PreCheckResult preCheckResult = _imGuiListInfo.PreCheckResult;
            if (preCheckResult.HasRichLabel)
            {
                // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
                if(_richTextDrawer == null)
                {
                    _richTextDrawer = new RichTextDrawer();
                }

                // Debug.Log(preCheckResult.RichLabelXml);
                if (_curXml != preCheckResult.RichLabelXml)
                {
                    _curXmlChunks =
                        RichTextDrawer
                            .ParseRichXmlWithProvider(preCheckResult.RichLabelXml, this)
                            .ToArray();
                }

                _curXml = preCheckResult.RichLabelXml;

                _richTextDrawer.DrawChunks(titleButtonRect, _curXmlChunks);
            }
            else
            {
                EditorGUI.LabelField(titleButtonRect, _imGuiListInfo.Property.displayName);
            }

            if (_imGuiListInfo.Property.isExpanded)
            {
                if (!_iconDown)
                {
                    _iconDown = Util.LoadResource<Texture2D>("classic-dropdown-gray.png");
                }
                GUI.DrawTexture(titleFoldRect, _iconDown);
            }
            else
            {
                if (!_iconRight)
                {
                    _iconRight = Util.LoadResource<Texture2D>("classic-dropdown-right-gray.png");
                }
                GUI.DrawTexture(titleFoldRect, _iconRight);
                return;
            }

            float searchInputWidth = rect.width - inputWidth * 2 - itemsLabelWidth - pagingSepWidth - buttonWidth * 2 - pagingLabelWidth;

            (Rect searchRect, Rect pagingRect) = RectUtils.SplitWidthRect(controlRect, _imGuiListInfo.HasPaging? searchInputWidth: controlRect.width);

            if(_imGuiListInfo.HasSearch)
            {
                string searchControlName = $"ListDrawerSettingsSearch_{property.propertyPath}";
                string oldSearchText = _imGuiListInfo.SearchText;
                Rect searchFieldRect = new Rect(searchRect)
                {
                    width = searchRect.width - gap,
                };
                if (_asyncSearchItems.Started && !_asyncSearchItems.Finished)
                {
                    Rect loadingRect = new Rect(searchFieldRect)
                    {
                        x = searchFieldRect.xMax - 14f,
                        width = 12f,
                    };
                    _imguiLoading?.Draw(loadingRect);
                    searchFieldRect.xMax -= 16f;
                }

                GUI.SetNextControlName(searchControlName);
                _imGuiListInfo.SearchText = EditorGUI.TextField(searchFieldRect, GUIContent.none, _imGuiListInfo.SearchText);
                if (oldSearchText != _imGuiListInfo.SearchText)
                {
                    UpdatePage(property, 0, _imGuiListInfo.NumberOfItemsPrePage, listDrawerSettingsAttribute);
                }

                if (Event.current.type == EventType.KeyDown
                    && Event.current.keyCode == KeyCode.Return
                    && GUI.GetNameOfFocusedControl() == searchControlName
                    && !_asyncSearchItems.Started
                    && _asyncSearchItems.SourceGenerator != null
                    && _asyncSearchItems.DebounceSearchTime > EditorApplication.timeSinceStartup)
                {
                    _asyncSearchItems.DebounceSearchTime = EditorApplication.timeSinceStartup - 1;
                }

                if (string.IsNullOrEmpty(_imGuiListInfo.SearchText))
                {
                    EditorGUI.LabelField(new Rect(searchRect)
                    {
                        width = searchRect.width - 6,
                    }, "Search", new GUIStyle("label") { alignment = TextAnchor.MiddleRight, normal =
                    {
                        textColor = Color.gray,
                    }, fontStyle = FontStyle.Italic});
                }
            }

            if(_imGuiListInfo.HasPaging)
            {
                Rect numberOfItemsPerPageRect = new Rect(pagingRect)
                {
                    width = inputWidth,
                };
                using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                {
                    int newNumberOfItemsPerPage = EditorGUI.DelayedIntField(numberOfItemsPerPageRect, GUIContent.none,
                        _imGuiListInfo.NumberOfItemsPrePage);
                    if (changed.changed)
                    {
                        UpdatePage(property, _imGuiListInfo.PageIndex, Mathf.Max(newNumberOfItemsPerPage, 0), listDrawerSettingsAttribute);
                    }
                }

                Rect numberOfItemsSepRect = new Rect(numberOfItemsPerPageRect)
                {
                    x = numberOfItemsPerPageRect.x + numberOfItemsPerPageRect.width,
                    width = pagingSepWidth,
                };
                EditorGUI.LabelField(numberOfItemsSepRect, $"/");

                Rect numberOfItemsTotalRect = new Rect(numberOfItemsSepRect)
                {
                    x = numberOfItemsSepRect.x + numberOfItemsSepRect.width,
                    width = itemsLabelWidth,
                };
                using(EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                {
                    int newCount = EditorGUI.DelayedIntField(numberOfItemsTotalRect, GUIContent.none, property.arraySize);
                    if (changed.changed)
                    {
                        SetArraySize(property, newCount, arraySizeAttribute, listDrawerSettingsAttribute);
                        return;
                    }
                }
                // EditorGUI.LabelField(totalItemRect, $"/ 8888 Items");

                EditorGUI.LabelField(numberOfItemsTotalRect, "Items", new GUIStyle("label") { alignment = TextAnchor.MiddleRight, normal =
                {
                    textColor = Color.gray,
                }, fontStyle = FontStyle.Italic});

                Rect prePageRect = new Rect(numberOfItemsTotalRect)
                {
                    x = numberOfItemsTotalRect.x + numberOfItemsTotalRect.width,
                    width = buttonWidth,
                };
                using (new EditorGUI.DisabledScope(_imGuiListInfo.PagingInfo.CurPageIndex <= 0))
                {
                    if (!_iconLeft)
                    {
                        _iconLeft = Util.LoadResource<Texture2D>("classic-dropdown-left.png");
                    }
                    if (GUI.Button(prePageRect, _iconLeft, EditorStyles.miniButtonLeft))
                    {
                        if (_imGuiListInfo.PagingInfo.CurPageIndex > 0)
                        {
                            _imGuiListInfo.PageIndex -= 1;
                        }
                    }
                }

                Rect pageRect = new Rect(prePageRect)
                {
                    x = prePageRect.x + prePageRect.width,
                    width = inputWidth,
                };
                using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                {
                    int newPageIndex = EditorGUI.DelayedIntField(pageRect, GUIContent.none, _imGuiListInfo.PageIndex + 1) - 1;
                    if (changed.changed)
                    {
                        UpdatePage(property, newPageIndex, _imGuiListInfo.NumberOfItemsPrePage, listDrawerSettingsAttribute);
                    }
                }
                Rect totalPageRect = new Rect(pageRect)
                {
                    x = pageRect.x + pageRect.width,
                    width = pagingLabelWidth,
                };
                EditorGUI.LabelField(totalPageRect, $"/ {_imGuiListInfo.PagingInfo.PageCount}");
                // EditorGUI.LabelField(totalPageRect, $"/ 888");

                Rect nextPageRect = new Rect(totalPageRect)
                {
                    x = totalPageRect.x + totalPageRect.width,
                    width = buttonWidth,
                };
                using (new EditorGUI.DisabledScope(_imGuiListInfo.PagingInfo.CurPageIndex >=
                                                   _imGuiListInfo.PagingInfo.PageCount - 1))
                {
                    if (!_iconRight)
                    {
                        _iconRight = Util.LoadResource<Texture2D>("classic-dropdown-right.png");
                    }
                    if (GUI.Button(nextPageRect, _iconRight, EditorStyles.miniButtonRight))
                    {
                        if (_imGuiListInfo.PagingInfo.CurPageIndex < _imGuiListInfo.PagingInfo.PageCount - 1)
                        {
                            _imGuiListInfo.PageIndex += 1;
                        }
                    }
                }
            }
        }

        private float DrawListDrawerItemHeight(int index)
        {
            if (_imGuiListInfo.PagingInfo.IndexesCurPage.Contains(index))
            {
                if(index >= _imGuiListInfo.Property.arraySize)
                {
                    return 0;
                }
                SerializedProperty element = FieldWithInfo.SerializedProperty.GetArrayElementAtIndex(index);
                return EditorGUI.GetPropertyHeight(element, true);
            }

            return 0;
        }

        private void DrawListDrawerItem(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (rect.height <= 0)
            {
                return;
            }

            SerializedProperty property = _imGuiListInfo.Property.GetArrayElementAtIndex(index);
            int displayIndex = _imGuiListInfo.PagingInfo.IndexesCurPage.IndexOf(index);

            Rect useRect = property.propertyType == SerializedPropertyType.Generic
                ? new Rect(rect)
                {
                    x = rect.x + 12,
                    width = rect.width - 12,
                }
                : rect;

            EditorGUI.PropertyField(useRect, property, new GUIContent($"Element {(displayIndex >= 0 ? displayIndex : index)}"), true);
        }



        private static PagingInfo GetPagingInfo(int newPageIndex, IReadOnlyList<int> fullIndexResults, int numberOfItemsPerPage)
        {
            // IReadOnlyList<int> fullIndexResults = string.IsNullOrEmpty(search)
            //     ? Enumerable.Range(0, property.arraySize).ToList()
            //     : SerializedUtils.SearchArrayProperty(property, search).ToList();

// #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_LIST_DRAWER_SETTINGS
//             Debug.Log($"index search={search} result: {string.Join(",", fullIndexResults)}; numberOfItemsPerPage={numberOfItemsPerPage}");
// #endif

            // List<int> itemIndexToPropertyIndex = fullIndexResults.ToList();
            int curPageIndex;

            int pageCount;
            int skipStart;
            int itemCount;
            if (numberOfItemsPerPage <= 0)
            {
                pageCount = 1;
                curPageIndex = 0;
                skipStart = 0;
                itemCount = int.MaxValue;
            }
            else
            {
                pageCount = Mathf.CeilToInt((float)fullIndexResults.Count / numberOfItemsPerPage);
                curPageIndex = Mathf.Clamp(newPageIndex, 0, pageCount - 1);
                skipStart = curPageIndex * numberOfItemsPerPage;
                itemCount = numberOfItemsPerPage;
            }

            List<int> curPageItemIndexes = fullIndexResults.Skip(skipStart).Take(itemCount).ToList();

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_LIST_DRAWER_SETTINGS
            Debug.Log($"set items: {string.Join(", ", curPageItemIndexes)}, itemIndexToPropertyIndex={string.Join(",", fullIndexResults)}");
#endif

            return new PagingInfo
            {
                IndexesAfterSearch = fullIndexResults,
                IndexesCurPage = curPageItemIndexes,
                CurPageIndex = curPageIndex,
                PageCount = pageCount,
            };
        }



        protected override float GetFieldHeightIMGUI(float width, PreCheckResult preCheckResult)
        {
            ListDrawerSettingsAttribute listDrawerSettingsAttribute = FieldWithInfo.PlayaAttributes.OfType<ListDrawerSettingsAttribute>().First();

            bool hasSearch = listDrawerSettingsAttribute.Searchable;
            bool hasPaging = listDrawerSettingsAttribute.NumberOfItemsPerPage > 0;

            if (_imGuiListInfo == null)
            {
                int numberOfItemsPrePage = listDrawerSettingsAttribute.NumberOfItemsPerPage;
                List<int> fullList = Enumerable.Range(0, FieldWithInfo.SerializedProperty.arraySize).ToList();
                _imGuiListInfo = new ImGuiListInfo
                {
                    Property = FieldWithInfo.SerializedProperty,
                    PreCheckResult = preCheckResult,
                    HasSearch = hasSearch,
                    HasPaging = hasPaging,
                    PagingInfo = GetPagingInfo(0, fullList, numberOfItemsPrePage),
                    NumberOfItemsPrePage = numberOfItemsPrePage,
                    PageIndex = 0,
                    SearchText = "",
                };
                _asyncSearchItems = new AsyncSearchItems
                {
                    Started = true,
                    Finished = true,
                    SourceGenerator = Enumerable.Empty<IReadOnlyList<int>>().GetEnumerator(),
                    FullSources = fullList,
                    CachedFullSources = new List<int>(fullList),
                    SearchText = "",
                    DebounceSearchTime = double.MaxValue,
                    ItemIndexToPropertyIndex = fullList.ToList(),
                    CurPageIndex = 0,
                };
                FieldWithInfo.SerializedProperty.isExpanded = true;
            }

            TickAsyncSearch(FieldWithInfo.SerializedProperty, listDrawerSettingsAttribute);
            UpdatePage(FieldWithInfo.SerializedProperty, _imGuiListInfo.PageIndex, _imGuiListInfo.NumberOfItemsPrePage,
                listDrawerSettingsAttribute);

            if (!FieldWithInfo.SerializedProperty.isExpanded)
            {
                // return SaintsPropertyDrawer.SingleLineHeight;
                return SaintsPropertyDrawer.SingleLineHeight;
            }

            int extraLineCount = (hasSearch || hasPaging) ? 4 : 3;

            float height = _imGuiListInfo.PagingInfo.IndexesCurPage
                               .Select(index => EditorGUI.GetPropertyHeight(FieldWithInfo.SerializedProperty.GetArrayElementAtIndex(index), true))
                               .Sum()
                           + (_imGuiListInfo.PagingInfo.IndexesCurPage.Count == 0? EditorGUIUtility.singleLineHeight: 0)
                           + SaintsPropertyDrawer.SingleLineHeight * extraLineCount;  // header with controller line, footer (plus, minus)
            return height;
        }

        // protected override void SerializedFieldRenderPositionTargetIMGUI(Rect position, PreCheckResult preCheckResult)
        protected override void RenderPositionTargetIMGUI(Rect position, PreCheckResult preCheckResult)
        {
            using (new EditorGUI.DisabledScope(preCheckResult.IsDisabled))
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_SERIALIZED_FIELD_RENDERER
                Debug.Log($"SerField: {FieldWithInfo.SerializedProperty.displayName}->{FieldWithInfo.SerializedProperty.propertyPath}; arraySize={preCheckResult.ArraySize}");
#endif

                ListDrawerSettingsAttribute listDrawerSettingsAttribute = FieldWithInfo.PlayaAttributes.OfType<ListDrawerSettingsAttribute>().First();
                if(_imGuiListInfo != null)
                {
                    _imGuiListInfo.PreCheckResult = preCheckResult;
                    ArraySizeAttribute arraySizeAttribute = FieldWithInfo.PlayaAttributes.OfType<ArraySizeAttribute>().FirstOrDefault();
                    DrawListDrawerSettingsField(FieldWithInfo.SerializedProperty, position, arraySizeAttribute, listDrawerSettingsAttribute);
                }
            }
        }
    }
}

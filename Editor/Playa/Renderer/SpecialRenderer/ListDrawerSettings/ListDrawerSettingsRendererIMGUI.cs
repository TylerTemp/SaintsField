using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.ArraySizeDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace SaintsField.Editor.Playa.Renderer.SpecialRenderer.ListDrawerSettings
{
    public partial class ListDrawerSettingsRenderer
    {
        private RichTextDrawer _richTextDrawer;

        ~ListDrawerSettingsRenderer()
        {
            _richTextDrawer = null;
        }

        public override void OnDestroy()
        {
            _richTextDrawer?.Dispose();
            _richTextDrawer = null;
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

        private void DrawListDrawerSettingsField(SerializedProperty property, Rect position, ArraySizeAttribute arraySizeAttribute, bool delayed)
        {
            int min = -1;
            int max = -1;
            bool dynamic = true;
            if (arraySizeAttribute != null)
            {
                (string error, bool dynamicValue, int minValue, int maxValue) = ArraySizeAttributeDrawer.GetMinMax(arraySizeAttribute,
                    FieldWithInfo.SerializedProperty,
                    FieldWithInfo.FieldInfo, FieldWithInfo.Target);

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

                DrawListDrawerHeader(paddingTitle, delayed, elementType, property, arraySizeAttribute);
                return;
            }

            PagingInfo newPagingInfo = GetPagingInfo(property, _imGuiListInfo.PageIndex, _imGuiListInfo.SearchText, _imGuiListInfo.NumberOfItemsPrePage);
            if (!newPagingInfo.IndexesCurPage.SequenceEqual(_imGuiListInfo.PagingInfo.IndexesCurPage))
            {
                _imGuiReorderableList = null;
            }

            _imGuiListInfo.PagingInfo = newPagingInfo;

            if (_imGuiReorderableList == null)
            {
                _imGuiReorderableList = new ReorderableList(property.serializedObject, property, true, true, true, true)
                    {
                        headerHeight = SaintsPropertyDrawer.SingleLineHeight * ((_imGuiListInfo.HasPaging || _imGuiListInfo.HasSearch)? 2: 1),
                    };

                Type elementType = ReflectUtils.GetElementType(FieldWithInfo.FieldInfo?.FieldType ?? FieldWithInfo.PropertyInfo.PropertyType);
                _imGuiReorderableList.drawHeaderCallback += v => DrawListDrawerHeader(v, delayed, elementType, property, arraySizeAttribute);
                _imGuiReorderableList.elementHeightCallback += DrawListDrawerItemHeight;
                _imGuiReorderableList.drawElementCallback += DrawListDrawerItem;

                if(arraySizeAttribute != null)
                {
                    if (dynamic)
                    {
                        _imGuiReorderableList.onCanRemoveCallback += r =>
                        {
                            (string removeError, bool _, int removeMin, int _) = ArraySizeAttributeDrawer.GetMinMax(arraySizeAttribute, FieldWithInfo.SerializedProperty,
                                FieldWithInfo.FieldInfo, FieldWithInfo.Target);
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
                                FieldWithInfo.FieldInfo, FieldWithInfo.Target);
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

        private void DrawListDrawerHeader(Rect rect, bool delayed, Type elementType, SerializedProperty property, ArraySizeAttribute arraySizeAttribute)
        {
            int min = -1;
            int max = -1;
            bool dynamic = true;
            if (arraySizeAttribute != null)
            {
                (string error, bool dynamicValue, int minValue, int maxValue) = ArraySizeAttributeDrawer.GetMinMax(arraySizeAttribute,
                    FieldWithInfo.SerializedProperty,
                    FieldWithInfo.FieldInfo, FieldWithInfo.Target);

                if (error == "")
                {
                    min = minValue;
                    max = maxValue;
                    dynamic = dynamicValue;
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
                    int newCount = EditorGUI.DelayedIntField(titleItemTotalRect, GUIContent.none,
                        _imGuiListInfo.PagingInfo.IndexesAfterSearch.Count);
                    if (changed.changed)
                    {
                        if(min > 0 && newCount < min)
                        {
                            newCount = min;
                        }
                        else if(max > 0 && newCount > max)
                        {
                            newCount = max;
                        }
                        if(_imGuiListInfo.Property.arraySize != newCount)
                        {
                            _imGuiListInfo.Property.arraySize = newCount;
                            _imGuiListInfo.Property.serializedObject.ApplyModifiedProperties();
                            _imGuiListInfo.PagingInfo = GetPagingInfo(_imGuiListInfo.Property, _imGuiListInfo.PageIndex,
                                _imGuiListInfo.SearchText, _imGuiListInfo.NumberOfItemsPrePage);
                            return;
                        }
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
                            .ParseRichXml(preCheckResult.RichLabelXml, FieldWithInfo.SerializedProperty.displayName, FieldWithInfo.SerializedProperty, FieldWithInfo.FieldInfo, FieldWithInfo.Target)
                            .ToArray();
                }

                _curXml = preCheckResult.RichLabelXml;

                _richTextDrawer.DrawChunks(titleButtonRect, new GUIContent(FieldWithInfo.SerializedProperty.displayName), _curXmlChunks);
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
                if(delayed)
                {
                    _imGuiListInfo.SearchText = EditorGUI.DelayedTextField(new Rect(searchRect)
                    {
                        width = searchRect.width - gap,
                    }, GUIContent.none, _imGuiListInfo.SearchText);
                }
                else
                {
                    _imGuiListInfo.SearchText = EditorGUI.TextField(new Rect(searchRect)
                    {
                        width = searchRect.width - gap,
                    }, GUIContent.none, _imGuiListInfo.SearchText);
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
                _imGuiListInfo.NumberOfItemsPrePage = EditorGUI.IntField(numberOfItemsPerPageRect, GUIContent.none,
                    _imGuiListInfo.NumberOfItemsPrePage);

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
                    int newCount = EditorGUI.DelayedIntField(numberOfItemsTotalRect, GUIContent.none,
                        _imGuiListInfo.PagingInfo.IndexesAfterSearch.Count);
                    if (changed.changed)
                    {
                        _imGuiListInfo.Property.arraySize = newCount;
                        _imGuiListInfo.Property.serializedObject.ApplyModifiedProperties();
                        _imGuiListInfo.PagingInfo = GetPagingInfo(_imGuiListInfo.Property, _imGuiListInfo.PageIndex,
                            _imGuiListInfo.SearchText, _imGuiListInfo.NumberOfItemsPrePage);
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
                _imGuiListInfo.PageIndex =
                    EditorGUI.IntField(pageRect, GUIContent.none, _imGuiListInfo.PageIndex + 1) - 1;
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

            Rect useRect = property.propertyType == SerializedPropertyType.Generic
                ? new Rect(rect)
                {
                    x = rect.x + 12,
                    width = rect.width - 12,
                }
                : rect;

            EditorGUI.PropertyField(useRect, property, new GUIContent($"Element {index}"), true);
        }

        private static PagingInfo GetPagingInfo(SerializedProperty property, int newPageIndex, string search, int numberOfItemsPerPage)
        {
            IReadOnlyList<int> fullIndexResults = string.IsNullOrEmpty(search)
                ? Enumerable.Range(0, property.arraySize).ToList()
                : SerializedUtils.SearchArrayProperty(property, search).ToList();

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_LIST_DRAWER_SETTINGS
            Debug.Log($"index search={search} result: {string.Join(",", fullIndexResults)}; numberOfItemsPerPage={numberOfItemsPerPage}");
#endif

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
                _imGuiListInfo = new ImGuiListInfo
                {
                    Property = FieldWithInfo.SerializedProperty,
                    PreCheckResult = preCheckResult,
                    HasSearch = hasSearch,
                    HasPaging = hasPaging,
                    PagingInfo = GetPagingInfo(FieldWithInfo.SerializedProperty, 0, "", numberOfItemsPrePage),
                    NumberOfItemsPrePage = numberOfItemsPrePage,
                    PageIndex = 0,
                    SearchText = "",
                };
                FieldWithInfo.SerializedProperty.isExpanded = true;
            }
            else
            {
                _imGuiListInfo.PagingInfo = GetPagingInfo(FieldWithInfo.SerializedProperty, _imGuiListInfo.PageIndex,
                    _imGuiListInfo.SearchText, _imGuiListInfo.NumberOfItemsPrePage);
            }

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

        protected override void SerializedFieldRenderPositionTargetIMGUI(Rect position, PreCheckResult preCheckResult)
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
                    DrawListDrawerSettingsField(FieldWithInfo.SerializedProperty, position, arraySizeAttribute, listDrawerSettingsAttribute.Delayed);
                }
            }
        }

        protected override void RenderTargetIMGUI(float width, PreCheckResult preCheckResult)
        {
            float height = GetFieldHeightIMGUI(width, preCheckResult);
            Rect position = EditorGUILayout.GetControlRect(true, height, GUILayout.ExpandWidth(true));
            RenderPositionTargetIMGUI(position, preCheckResult);
        }
    }
}

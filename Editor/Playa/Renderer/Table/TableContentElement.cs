using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Drawers.SaintsRowDrawer;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using SaintsField.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Playa.Renderer.Table
{
    public class TableContentElement: VisualElement
    {
        private readonly SaintsFieldWithInfo _fieldWithInfo;

        private readonly VisualElement _emptyNotice;
        private readonly VisualElement _tableContentContainer;

        private readonly HashSet<string> valueTableHeaders;
        private readonly bool headerIsHide;
        private readonly Type elementType;

        private int _preArraySize;
        private MultiColumnListView _multiColumnListView;

        public TableContentElement(SaintsFieldWithInfo fieldWithInfo)
        {
            _fieldWithInfo = fieldWithInfo;
            // Empty
            // _emptyNotice = new HelpBox("Table is empty", HelpBoxMessageType.None);
            Color borderColor = new Color(0.1254902f, 0.1254902f, 0.1254902f);
            _emptyNotice = new VisualElement
            {
                style =
                {
                    borderLeftColor = borderColor,
                    borderLeftWidth = 1,
                    borderRightColor = borderColor,
                    borderRightWidth = 1,
                    borderTopColor = borderColor,
                    borderTopWidth = 1,
                    borderBottomColor = borderColor,
                    borderBottomWidth = 1,
                    backgroundColor = new Color(0.2745098f, 0.2745098f, 0.2745098f),
                },
            };
            _emptyNotice.Add(new Label("Table is empty")
            {
                style =
                {
                    height = 22,
                    paddingLeft = 6,
                    paddingRight = 2,
                    unityTextAlign = TextAnchor.MiddleLeft,
                    whiteSpace = WhiteSpace.NoWrap,
                },
            });
            Add(_emptyNotice);

            // Filled
            _tableContentContainer = new VisualElement();
            Add(_tableContentContainer);

            this.TrackPropertyValue(fieldWithInfo.SerializedProperty, _ => OnArrayPropertyChanged());

            elementType =
                ReflectUtils.GetElementType(fieldWithInfo.FieldInfo?.FieldType ??
                                            fieldWithInfo.PropertyInfo.PropertyType);

            #region Headers

            MemberInfo info;
            if (fieldWithInfo.FieldInfo != null)
            {
                info = fieldWithInfo.FieldInfo;
            }
            else
            {
                info = fieldWithInfo.PropertyInfo;
            }
            TableHeadersAttribute tableHeadersAttribute = ReflectCache.GetCustomAttributes<TableHeadersAttribute>(info)
                .FirstOrDefault();
            valueTableHeaders = new HashSet<string>();
            headerIsHide = true;
            if (tableHeadersAttribute != null)
            {
                headerIsHide = tableHeadersAttribute.IsHide;
                foreach (TableHeadersAttribute.Header header in tableHeadersAttribute.Headers)
                {
                    // string rawName = header.Name;
                    List<string> rawNames = new List<string>();
                    if (header.IsCallback)
                    {
                        (string error, object value) = Util.GetOfNoParams<object>(fieldWithInfo.Targets[0], header.Name, null);
                        if (error != "")
                        {
#if SAINTSFIELD_DEBUG
                            Debug.LogError(error);
#endif
                            continue;
                        }

                        if (RuntimeUtil.IsNull(value))
                        {
                        }
                        // ReSharper disable once ConvertIfStatementToSwitchStatement
                        else if (value is string s)
                        {
                            rawNames.Add(s);
                        }
                        else if (value is IEnumerable<string> si)
                        {
                            rawNames.AddRange(si.Where(each => each != null));
                        }
                        else if (value is IEnumerable<object> oi)
                        {
                            rawNames.AddRange(oi.Where(each => !RuntimeUtil.IsNull(each))
                                .Select(each => each.ToString()));
                        }
                    }
                    else
                    {
                        rawNames.Add(header.Name);
                    }

                    valueTableHeaders.UnionWith(rawNames.SelectMany(each => new[]{each, ObjectNames.NicifyVariableName(each)}));
                }
            }
            #endregion

            OnArrayPropertyChanged();
        }

        public IEnumerable<int> SelectedIndices()
        {
            if (_preArraySize == 0)
            {
                return Array.Empty<int>();
            }

            return _multiColumnListView.selectedIndices;
        }

        private void OnArrayPropertyChanged()
        {
            var arrayProp = _fieldWithInfo.SerializedProperty;
            int newArraySize = arrayProp.arraySize;
            if (newArraySize == 0)
            {
                _preArraySize = newArraySize;
                _emptyNotice.style.display = DisplayStyle.Flex;
                _tableContentContainer.Clear();
                _multiColumnListView = null;
                return;
            }

            if (_preArraySize == 0)
            {
                _preArraySize = newArraySize;
                _emptyNotice.style.display = DisplayStyle.None;
                _tableContentContainer.Clear();

                MultiColumnListView multiColumnListView = _multiColumnListView = new MultiColumnListView
                {
                    showBoundCollectionSize = true,
                    virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                    itemsSource = MakeSource(arrayProp),
                    reorderable = true,
                    reorderMode = ListViewReorderMode.Animated,
                    showBorder = true,

                    viewDataKey = SerializedUtils.GetUniqueId(arrayProp),

                    // this has some issue because we bind order with renderer. Sort is not possible
// #if UNITY_6000_0_OR_NEWER
//                 sortingMode = ColumnSortingMode.Default,
// #else
//                 sortingEnabled = true,
// #endif
                };
                _tableContentContainer.Add(multiColumnListView);

                var firstProp = arrayProp.GetArrayElementAtIndex(0);
                bool itemIsObject = firstProp.propertyType == SerializedPropertyType.ObjectReference;

                if (itemIsObject)
                {
                    Object obj0 = multiColumnListView.itemsSource.Cast<SerializedProperty>()
                        .Select(each => each.objectReferenceValue)
                        .FirstOrDefault(each => each);

                    Dictionary<string, List<string>> columnToMemberIds = new Dictionary<string, List<string>>();
                    Dictionary<string, bool> columnToDefaultHide = new Dictionary<string, bool>();

                    using(SerializedObject serializedObject = new SerializedObject(obj0))
                    {
                        Dictionary<string, SerializedProperty> serializedPropertyDict = SerializedUtils
                            .GetAllField(serializedObject)
                            .Where(each => each != null)
                            .ToDictionary(each => each.name, each => each.Copy());
                        IEnumerable<SaintsFieldInfoName> saintsFieldWithInfos = SaintsEditor
                            .HelperGetSaintsFieldWithInfo(serializedPropertyDict,  new []{obj0})
                            .Where(SaintsEditor.SaintsFieldInfoShouldDraw)
                            .Select(each => new SaintsFieldInfoName(each, AbsRenderer.GetFriendlyName(each)));

                        foreach (SaintsFieldInfoName saintsFieldInfoName in saintsFieldWithInfos)
                        {
                            string columnName = saintsFieldInfoName.FriendlyName;
                            foreach (IPlayaAttribute playaAttribute in saintsFieldInfoName.SaintsFieldWithInfo.PlayaAttributes)
                            {
                                // ReSharper disable once InvertIf
                                if (playaAttribute is TableColumnAttribute tc)
                                {
                                    columnName = tc.Title;
                                    break;
                                }
                            }

                            bool headerHide = HeaderDefaultHide(columnName, valueTableHeaders, headerIsHide);
                            if (headerHide)
                            {
                                columnToDefaultHide[columnName] = true;
                            }
                            else
                            {
                                // ReSharper disable once LoopCanBeConvertedToQuery
                                foreach (IPlayaAttribute playaAttribute in saintsFieldInfoName.SaintsFieldWithInfo
                                             .PlayaAttributes)
                                {
                                    // ReSharper disable once InvertIf
                                    if (playaAttribute is TableHideAttribute)
                                    {
                                        columnToDefaultHide[columnName] = true;
                                        break;
                                    }
                                }
                            }

                            if(!columnToMemberIds.TryGetValue(columnName, out List<string> list))
                            {
                                columnToMemberIds[columnName] = list = new List<string>();
                            }
                            // Debug.Log($"{columnName}: {saintsFieldWithInfo}");
                            list.Add(saintsFieldInfoName.SaintsFieldWithInfo.MemberId);
                        }
                    }

                    // ReSharper disable once UseDeconstruction
                    foreach (KeyValuePair<string, List<string>> columnKv in columnToMemberIds)
                    {
                        string columnName = columnKv.Key;
                        List<string> memberIds = columnKv.Value;

                        string id = string.Join(";", memberIds);

                        bool visible = true;
                        if (columnToDefaultHide.TryGetValue(columnName, out bool hide))
                        {
                            visible = !hide;
                        }

                        multiColumnListView.columns.Add(new Column
                        {
                            name = id,
                            title = columnName,
                            stretchable = true,
                            visible = visible,
                            makeCell = () =>
                            {
                                VisualElement itemContainer = new VisualElement();

                                itemContainer.RegisterCallback<AttachToPanelEvent>(_ => UIToolkitUtils.LoopCheckOutOfScoopFoldout(itemContainer));

                                return itemContainer;
                            },
                            bindCell = (element, index) =>
                            {
                                SerializedProperty targetProp = ((SerializedProperty)multiColumnListView.itemsSource[index]).Copy();
                                targetProp.isExpanded = true;

                                Object targetPropValue = targetProp.objectReferenceValue;

                                if (RuntimeUtil.IsNull(targetPropValue))
                                {
                                    ObjectField arrayItemProp = new ObjectField("")
                                    {
                                        objectType = elementType,
                                    };

                                    element.Clear();
                                    element.Add(arrayItemProp);

                                    arrayItemProp.RegisterValueChangedCallback(evt =>
                                    {
                                        targetProp.objectReferenceValue = evt.newValue;
                                        targetProp.serializedObject.ApplyModifiedProperties();
                                        multiColumnListView.Rebuild();
                                    });
                                    return;
                                }

                                SerializedObject targetSerializedObject = new SerializedObject(targetPropValue);

                                Dictionary<string, SerializedProperty> targetPropertyDict = SerializedUtils
                                    .GetAllField(targetSerializedObject)
                                    .Where(each => each != null)
                                    .ToDictionary(each => each.name, each => each.Copy());

                                List<SaintsFieldWithInfo> allSaintsFieldWithInfos =
                                    new List<SaintsFieldWithInfo>(memberIds.Count);

                                int serCount = 0;
                                foreach (SaintsFieldWithInfo saintsFieldWithInfo in SaintsEditor
                                             .HelperGetSaintsFieldWithInfo(targetPropertyDict, new[]{targetPropValue})
                                             .Where(saintsFieldWithInfo => memberIds.Contains(saintsFieldWithInfo.MemberId)))
                                {
                                    allSaintsFieldWithInfos.Add(saintsFieldWithInfo);
                                    if (saintsFieldWithInfo.SerializedProperty != null)
                                    {
                                        serCount += 1;
                                    }
                                }

                                element.Clear();

                                bool saintsRowInline = memberIds.Count == 1;
                                bool noLabel = serCount <= 1;

                                using(new SaintsRowAttributeDrawer.ForceInlineScoop(saintsRowInline? 1: 0))
                                {
                                    // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
                                    foreach (SaintsFieldWithInfo saintsFieldWithInfo in allSaintsFieldWithInfos)
                                    {
                                        foreach (AbsRenderer renderer in SaintsEditor.HelperMakeRenderer(arrayProp.serializedObject, saintsFieldWithInfo))
                                        {
                                            // Debug.Log(renderer);
                                            // ReSharper disable once InvertIf
                                            if (renderer != null)
                                            {
                                                renderer.NoLabel = noLabel;
                                                renderer.InDirectHorizontalLayout = renderer.InAnyHorizontalLayout = true;
                                                VisualElement fieldElement = renderer.CreateVisualElement();
                                                if (fieldElement != null)
                                                {
                                                    element.Add(fieldElement);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        });
                    }
                }
                else  // item is general
                {
                    // Debug.Log($"rendering generic {firstProp.propertyPath}");
                    Dictionary<string, SerializedProperty> firstSerializedPropertyDict = SerializedUtils.GetPropertyChildren(firstProp)
                        .Where(each => each != null)
                        .ToDictionary(each => each.name);

                    MemberInfo info = (MemberInfo)_fieldWithInfo.FieldInfo ?? _fieldWithInfo.PropertyInfo;

                    (PropertyAttribute[] _, object parentRefreshed) = SerializedUtils.GetAttributesAndDirectParent<PropertyAttribute>(firstProp);

                    (string error, int index, object value) firstPropValue = Util.GetValue(firstProp, info, parentRefreshed);

                    IEnumerable<SaintsFieldWithInfo> firstSaintsFieldWithInfos = SaintsEditor
                        .HelperGetSaintsFieldWithInfo(firstSerializedPropertyDict, new[]{firstPropValue.value})
                        .Where(SaintsEditor.SaintsFieldInfoShouldDraw);

                    Dictionary<string, List<string>> columnToMemberIds = new Dictionary<string, List<string>>();
                    Dictionary<string, bool> columnToDefaultHide = new Dictionary<string, bool>();

                    foreach (SaintsFieldWithInfo saintsFieldWithInfo in firstSaintsFieldWithInfos)
                    {
                        string columnName = AbsRenderer.GetFriendlyName(saintsFieldWithInfo);
                        foreach (IPlayaAttribute playaAttribute in saintsFieldWithInfo.PlayaAttributes)
                        {
                            // ReSharper disable once InvertIf
                            if (playaAttribute is TableColumnAttribute tc)
                            {
                                columnName = tc.Title;
                                break;
                            }
                        }

                        bool headerHide = HeaderDefaultHide(columnName, valueTableHeaders, headerIsHide);
                        if (headerHide)
                        {
                            columnToDefaultHide[columnName] = true;
                        }
                        else
                        {
                            // ReSharper disable once LoopCanBeConvertedToQuery
                            foreach (IPlayaAttribute playaAttribute in saintsFieldWithInfo.PlayaAttributes)
                            {
                                // ReSharper disable once InvertIf
                                if (playaAttribute is TableHideAttribute)
                                {
                                    columnToDefaultHide[columnName] = true;
                                    break;
                                }
                            }
                        }

                        if(!columnToMemberIds.TryGetValue(columnName, out List<string> list))
                        {
                            columnToMemberIds[columnName] = list = new List<string>();
                        }
                        // Debug.Log($"{columnName}: {saintsFieldWithInfo}");
                        list.Add(saintsFieldWithInfo.MemberId);
                    }

                    // ReSharper disable once UseDeconstruction
                    foreach (KeyValuePair<string, List<string>> columnKv in columnToMemberIds)
                    {
                        string columnName = columnKv.Key;
                        List<string> memberIds = columnKv.Value;

                        string id = string.Join(";", memberIds);

                        bool visible = true;
                        if (columnToDefaultHide.TryGetValue(columnName, out bool hide))
                        {
                            visible = !hide;
                        }

                        Column curColumn = new Column
                        {
                            name = id,
                            title = columnName,
                            stretchable = true,
                            visible = visible,
                        };
                        multiColumnListView.columns.Add(curColumn);

                        curColumn.makeCell = () =>
                        {
                            VisualElement itemContainer = new VisualElement();

                            itemContainer.RegisterCallback<AttachToPanelEvent>(_ => UIToolkitUtils.LoopCheckOutOfScoopFoldout(itemContainer));

                            return itemContainer;
                        };

                        curColumn.bindCell = (element, index) =>
                        {
                            // Debug.Log($"id={id}/index={index}");
                            SerializedProperty targetProp = (SerializedProperty)multiColumnListView.itemsSource[index];
                            targetProp.isExpanded = true;

                            (PropertyAttribute[] _, object parentRefreshed) = SerializedUtils.GetAttributesAndDirectParent<PropertyAttribute>(targetProp);

                            (string error, int index, object value) targetPropValue = Util.GetValue(targetProp, info, parentRefreshed);
                            if (targetPropValue.error != "")
                            {
                                element.Clear();
                                element.Add(new HelpBox(targetPropValue.error, HelpBoxMessageType.Error));
                                return;
                            }
                            Dictionary<string, SerializedProperty> targetSerializedPropertyDict = SerializedUtils.GetPropertyChildren(targetProp)
                                .Where(each => each != null)
                                .ToDictionary(each => each.name);

                            List<SaintsFieldWithInfo>  allSaintsFieldWithInfos = new List<SaintsFieldWithInfo>(memberIds.Count);

                            // Debug.Log($"looking {targetPropValue.value}({targetPropValue.error})");

                            int serCount = 0;
                            foreach (SaintsFieldWithInfo saintsFieldWithInfo in SaintsEditor
                                         .HelperGetSaintsFieldWithInfo(targetSerializedPropertyDict, new[]{targetPropValue.value})
                                         .Where(saintsFieldWithInfo => memberIds.Contains(saintsFieldWithInfo.MemberId)))
                            {
                                // Debug.Log($"get {saintsFieldWithInfo}");
                                allSaintsFieldWithInfos.Add(saintsFieldWithInfo);
                                if (saintsFieldWithInfo.SerializedProperty != null)
                                {
                                    serCount += 1;
                                }
                            }

                            element.Clear();

                            bool saintsRowInline = memberIds.Count == 1;
                            bool noLabel = serCount <= 1;

                            using(new SaintsRowAttributeDrawer.ForceInlineScoop(saintsRowInline? 1: 0))
                            {
                                // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
                                foreach (SaintsFieldWithInfo saintsFieldWithInfo in allSaintsFieldWithInfos)
                                {
                                    foreach (AbsRenderer renderer in SaintsEditor.HelperMakeRenderer(arrayProp.serializedObject, saintsFieldWithInfo))
                                    {
                                        if(renderer != null)
                                        {
                                            renderer.NoLabel = noLabel;
                                            renderer.InDirectHorizontalLayout = renderer.InAnyHorizontalLayout = true;
                                            VisualElement fieldElement = renderer.CreateVisualElement();
                                            if (fieldElement != null)
                                            {
                                                element.Add(fieldElement);
                                            }
                                        }
                                    }
                                }
                            }
                        };
                    }
                }

                multiColumnListView.itemIndexChanged += (first, second) =>
                {
// #if SAINTSFIELD_DEBUG
//                 Debug.Log($"drag {first}({first}) -> {second}({second}) for {arrayProp.propertyPath}({arrayProp.arraySize})");
// #endif

                    arrayProp.MoveArrayElement(first, second);
                    arrayProp.serializedObject.ApplyModifiedProperties();
                    multiColumnListView.itemsSource = Enumerable
                        .Range(0, arrayProp.arraySize)
                        .Select(arrayProp.GetArrayElementAtIndex)
                        .ToList();
                };
                multiColumnListView.RegisterCallback<KeyDownEvent>(evt =>
            {
                // ReSharper disable once MergeIntoLogicalPattern
                bool ctrl = evt.modifiers == EventModifiers.Control ||
                            evt.modifiers == EventModifiers.Command;

                bool copyCommand = ctrl && evt.keyCode == KeyCode.C;
                if (copyCommand)
                {
                    SerializedProperty selected = multiColumnListView.selectedItems
                        .Cast<SerializedProperty>()
                        // .Select(each => SerializedUtils.PropertyPathIndex(each.propertyPath))
                        .FirstOrDefault();
                    // Debug.Log(string.Join(", ", selected));
                    if (selected == null)
                    {
                        return;
                    }

                    if (ClipboardHelper.CanCopySerializedProperty(selected.propertyType))
                    {
                        ClipboardHelper.DoCopySerializedProperty(selected);
                    }
                }

                bool pasteCommand = ctrl && evt.keyCode == KeyCode.V;
                if (pasteCommand)
                {
                    SerializedProperty selected = multiColumnListView.selectedItems
                        .Cast<SerializedProperty>()
                        // .Select(each => SerializedUtils.PropertyPathIndex(each.propertyPath))
                        .FirstOrDefault();
                    // Debug.Log(string.Join(", ", selected));
                    if (selected == null)
                    {
                        return;
                    }

                    (bool pasteHasReflection, bool pasteHasValue) = ClipboardHelper.CanPasteSerializedProperty(selected.propertyType);
                    // Debug.Log($"{pasteHasReflection}, {pasteHasValue}");
                    if (pasteHasReflection && pasteHasValue)
                    {
                        ClipboardHelper.DoPasteSerializedProperty(selected);
                        selected.serializedObject.ApplyModifiedProperties();
                    }
                }
            });

                return;
            }

            if (_preArraySize != newArraySize)
            {
                _preArraySize = newArraySize;
                // MultiColumnListView multiColumnListView = container.Q<MultiColumnListView>();
                var source = MakeSource(arrayProp);
                // Debug.Log($"Refresh set source to {string.Join(", ", source.Select(each => $"{each.propertyPath}/{each.propertyType}"))}");
                _multiColumnListView.itemsSource = source;
                // _multiColumnListView.Rebuild();
                // _multiColumnListView.schedule.Execute(() =>
                // {
                //     var source = MakeSource(arrayProp);
                //     Debug.Log(
                //         $"Refresh set source to {string.Join(", ", source.Select(each => $"{each.propertyPath}/{each.propertyType}"))}");
                //     _multiColumnListView.itemsSource = source;
                // }).StartingIn(500);
            }
        }

        private static List<SerializedProperty> MakeSource(SerializedProperty arrayProp)
        {
            return Enumerable.Range(0, arrayProp.arraySize)
                .Select(arrayProp.GetArrayElementAtIndex).ToList();
        }

        private readonly struct SaintsFieldInfoName
        {
            public readonly SaintsFieldWithInfo SaintsFieldWithInfo;
            public readonly string FriendlyName;

            public SaintsFieldInfoName(SaintsFieldWithInfo saintsFieldWithInfo, string friendlyName)
            {
                SaintsFieldWithInfo = saintsFieldWithInfo;
                FriendlyName = friendlyName;
            }
        }

        private static bool HeaderDefaultHide(string value, ICollection<string> valueTableHeaders, bool headerIsHide)
        {
            bool inHeader = valueTableHeaders.Contains(value);
            if (headerIsHide)
            {
                return inHeader;
            }
            return !inHeader;
        }
    }
}

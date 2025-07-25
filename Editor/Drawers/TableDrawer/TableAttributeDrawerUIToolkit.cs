#if UNITY_2022_2_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE && !SAINTSFIELD_DEBUG_UNITY_BROKEN_FALLBACK
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Drawers.ArraySizeDrawer;
using SaintsField.Editor.Drawers.SaintsRowDrawer;
using SaintsField.Editor.Playa;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Editor.Playa.RendererGroup;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using SaintsField.Playa;
using SaintsField.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;


namespace SaintsField.Editor.Drawers.TableDrawer
{
    public partial class TableAttributeDrawer
    {
        private static string NameArraySize(SerializedProperty property) => $"{property.propertyPath}__Table_ArraySize";
        private static string NameAddButton(SerializedProperty property) => $"{property.propertyPath}__Table_AddButton";
        private static string NameRemoveButton(SerializedProperty property) => $"{property.propertyPath}__Table_RemoveButton";

        private int _preArraySize;

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            int propertyIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);
            if (propertyIndex != 0)
            {
                return new VisualElement();
            }
            (string error, SerializedProperty arrayProp) = SerializedUtils.GetArrayProperty(property);

            if (error != "")
            {
                return new HelpBox(error, HelpBoxMessageType.Error);
            }

            TableAttribute tableAttribute = (TableAttribute) saintsAttribute;

            VisualElement root = new VisualElement();

            bool itemIsObject = property.propertyType == SerializedPropertyType.ObjectReference;
            if(itemIsObject)
            {
                Object obj0 = MakeSource(arrayProp).Select(each => each.objectReferenceValue)
                    .FirstOrDefault(each => each);
                if (!obj0)
                {
                    // PropertyField nullProp = new PropertyField(child0);
                    // nullProp.Bind(child0.serializedObject);
                    // return nullProp;
                    ObjectField arrayItemProp = new ObjectField("")
                    {
                        objectType = ReflectUtils.GetElementType(info.FieldType),
                    };

                    root.Add(arrayItemProp);

                    arrayItemProp.RegisterValueChangedCallback(evt =>
                    {
                        root.Clear();
                        property.objectReferenceValue = evt.newValue;
                        property.serializedObject.ApplyModifiedProperties();
                        BuildContent(arrayProp, root, tableAttribute, allAttributes, property, info, parent);
                    });
                    return root;
                }
            }

            BuildContent(arrayProp, root, tableAttribute, allAttributes, property, info, parent);
            return root;
        }

        private void BuildContent(SerializedProperty arrayProp, VisualElement root, TableAttribute tableAttribute, IReadOnlyList<PropertyAttribute> allAttributes, SerializedProperty property, FieldInfo info, object parent)
        {
            bool itemIsObject = property.propertyType == SerializedPropertyType.ObjectReference;

            int propertyIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);
            if (propertyIndex != 0)
            {
                return;
            }
            // (string error, SerializedProperty arrayProp) = SerializedUtils.GetArrayProperty(property);

            _preArraySize = arrayProp.arraySize;

            // if (error != "")
            // {
            //     return new HelpBox(error, HelpBoxMessageType.Error);
            // }

            // controls
            VisualElement controls = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.FlexEnd,
                    // marginBottom = 4,
                },
            };

            MultiColumnListView multiColumnListView = new MultiColumnListView
            {
                showBoundCollectionSize = true,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                itemsSource = MakeSource(arrayProp),
                reorderable = true,
                reorderMode = ListViewReorderMode.Animated,
                showBorder = true,

                // this has some issue because we bind order with renderer. Sort is not possible
// #if UNITY_6000_0_OR_NEWER
//                 sortingMode = ColumnSortingMode.Default,
// #else
//                 sortingEnabled = true,
// #endif
            };

            IntegerField arraySizeField = new IntegerField
            {
                value = arrayProp.arraySize,
                isDelayed = true,
                style =
                {
                    minWidth = 50,
                },
                name = NameArraySize(property),
            };
            arraySizeField.RegisterValueChangedCallback(evt =>
            {
                int newValue = evt.newValue;
                int oldValue = arrayProp.arraySize;
                int changedValue = ChangeArraySize(newValue, arrayProp);
                if (changedValue == oldValue)
                {
                    return;
                }

                _preArraySize = newValue;
                multiColumnListView.itemsSource = MakeSource(arrayProp);
                multiColumnListView.Rebuild();
            });
            controls.Add(arraySizeField);

            Toolbar toolbar = new Toolbar();
            ToolbarButton addButton = new ToolbarButton(() =>
            {
                int oldValue = arrayProp.arraySize;
                ChangeArraySize(oldValue + 1, arrayProp);
            })
            {
                text = "+",
                name = NameAddButton(property),
            };
            if (tableAttribute.HideAddButton)
            {
                addButton.style.display = DisplayStyle.None;
            }
            toolbar.Add(addButton);
            ToolbarButton removeButton = new ToolbarButton(() =>
            {
                DeleteArrayElement(arrayProp, multiColumnListView.selectedIndices);
            })
            {
                text = "-",
                name = NameRemoveButton(property),
            };
            if (tableAttribute.HideRemoveButton)
            {
                removeButton.style.display = DisplayStyle.None;
            }
            toolbar.Add(removeButton);

            if (tableAttribute.HideAddButton && tableAttribute.HideRemoveButton)
            {
                arraySizeField.SetEnabled(false);
            }

            controls.Add(toolbar);

            root.Add(controls);

            #region Headers

            TableHeadersAttribute tableHeadersAttribute = allAttributes
                .OfType<TableHeadersAttribute>()
                .FirstOrDefault();
            HashSet<string> valueTableHeaders = new HashSet<string>();
            bool headerIsHide = true;
            if (tableHeadersAttribute != null)
            {
                headerIsHide = tableHeadersAttribute.IsHide;
                foreach (TableHeadersAttribute.Header header in tableHeadersAttribute.Headers)
                {
                    // string rawName = header.Name;
                    List<string> rawNames = new List<string>();
                    if (header.IsCallback)
                    {
                        (string error, object value) = Util.GetOfNoParams<object>(parent, header.Name, null);
                        if (error != "")
                        {
#if UNITY_EDITOR
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
                        .HelperGetSaintsFieldWithInfo(serializedPropertyDict, new []{obj0})
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

                            HashSet<Toggle> toggles = new HashSet<Toggle>();

                            itemContainer.schedule
                                .Execute(() => SaintsRendererGroup.CheckOutOfScoopFoldout(itemContainer, toggles))
                                .Every(250);

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
                                    objectType = ReflectUtils.GetElementType(info.FieldType),
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

                            using(new SaintsRowAttributeDrawer.ForceInlineScoop(saintsRowInline))
                            {
                                // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
                                foreach (SaintsFieldWithInfo saintsFieldWithInfo in allSaintsFieldWithInfos)
                                {
                                    foreach (AbsRenderer renderer in SaintsEditor.HelperMakeRenderer(property.serializedObject, saintsFieldWithInfo))
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
                SerializedProperty firstProp = arrayProp.GetArrayElementAtIndex(0);
                // Debug.Log($"rendering generic {firstProp.propertyPath}");
                Dictionary<string, SerializedProperty> firstSerializedPropertyDict = SerializedUtils.GetPropertyChildren(firstProp)
                    .Where(each => each != null)
                    .ToDictionary(each => each.name);

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

                        HashSet<Toggle> toggles = new HashSet<Toggle>();

                        itemContainer.schedule
                            .Execute(() => SaintsRendererGroup.CheckOutOfScoopFoldout(itemContainer, toggles))
                            .Every(250);

                        return itemContainer;
                    };

                    curColumn.bindCell = (element, index) =>
                    {
                        // Debug.Log($"id={id}/index={index}");
                        SerializedProperty targetProp = (SerializedProperty)multiColumnListView.itemsSource[index];
                        targetProp.isExpanded = true;

                        (string error, int index, object value) targetPropValue = Util.GetValue(targetProp, info, parentRefreshed);
                        Dictionary<string, SerializedProperty> targetSerializedPropertyDict = SerializedUtils.GetPropertyChildren(targetProp)
                            .Where(each => each != null)
                            .ToDictionary(each => each.name);

                        List<SaintsFieldWithInfo>  allSaintsFieldWithInfos = new List<SaintsFieldWithInfo>(memberIds.Count);

                        int serCount = 0;
                        foreach (SaintsFieldWithInfo saintsFieldWithInfo in SaintsEditor
                                     .HelperGetSaintsFieldWithInfo(targetSerializedPropertyDict, new[]{targetPropValue.value})
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

                        using(new SaintsRowAttributeDrawer.ForceInlineScoop(saintsRowInline))
                        {
                            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
                            foreach (SaintsFieldWithInfo saintsFieldWithInfo in allSaintsFieldWithInfos)
                            {
                                foreach (AbsRenderer renderer in SaintsEditor.HelperMakeRenderer(property.serializedObject, saintsFieldWithInfo))
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
            root.Add(multiColumnListView);

            multiColumnListView.TrackPropertyValue(arrayProp, _ =>
            {
                // ReSharper disable once InvertIf
                if (_preArraySize != arrayProp.arraySize)
                {
                    _preArraySize = arrayProp.arraySize;
                    // MultiColumnListView multiColumnListView = container.Q<MultiColumnListView>();
                    multiColumnListView.itemsSource = MakeSource(arrayProp);
                    multiColumnListView.Rebuild();
                    arraySizeField.SetValueWithoutNotify(arrayProp.arraySize);
                }
            });

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

            // bool focused = false;
            // multiColumnListView.RegisterCallback<FocusOutEvent>(_ => focused = false);
            // multiColumnListView.RegisterCallback<FocusInEvent>(_ => focused = true);
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
// #endif

            // return root;
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

        private ArraySizeAttribute _arraySizeAttribute;
        private bool _dynamic;
        private int _min = -1;
        private int _max = -1;

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            _arraySizeAttribute = allAttributes.OfType<ArraySizeAttribute>().FirstOrDefault();
            if (_arraySizeAttribute == null)
            {
                return;
            }

            (string error, bool dynamic, int min, int max) = ArraySizeAttributeDrawer.GetMinMax(_arraySizeAttribute, property, info, parent);
            if (error != "")
            {
                return;
            }

            _dynamic = dynamic;
            _min = min;
            _max = max;
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, Action<object> onValueChanged, FieldInfo info)
        {
            if (_arraySizeAttribute is null)
            {
                return;
            }

            (string arrayError, SerializedProperty arrayProp) = SerializedUtils.GetArrayProperty(property);
            if (arrayError != "")
            {
                return;
            }

            if (_dynamic)
            {
                object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
                if (parent == null)
                {
                    return;
                }

                (string error, bool _, int min, int max) = ArraySizeAttributeDrawer.GetMinMax(_arraySizeAttribute, property, info, parent);
                if (error != "")
                {
                    return;
                }

                _min = min;
                _max = max;
            }

            Button addButton = container.Q<Button>(name: NameAddButton(property));
            Button removeButton = container.Q<Button>(name: NameRemoveButton(property));

            // Debug.Log($"{arrayProp.arraySize}: {_max}");

            if(_max >= 0)
            {
                addButton.SetEnabled(arrayProp.arraySize < _max);
            }
            if(_min >= 0)
            {
                removeButton.SetEnabled(arrayProp.arraySize > _min);
            }
        }

        // this does not work because Unity internally update the style using inline
        // protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
        //     IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        // {
        //     bool isRenderedBySaintsEditor = UIToolkitUtils.FindParentClass(container, AbsRenderer.ClassSaintsFieldPlayaContainer).Any();
        //     // Debug.Log($"isRenderedBySaintsEditor={isRenderedBySaintsEditor}");
        //     if (isRenderedBySaintsEditor)
        //     {
        //         return;
        //     }
        //
        //     ListView listView = UIToolkitUtils.IterUpWithSelf(container).OfType<ListView>().FirstOrDefault();
        //     if (listView == null)
        //     {
        //         return;
        //     }
        //
        //     VisualElement first = listView.Q<VisualElement>(name: "unity-list-view__reorderable-item");
        //     if (first != null && !first.ClassListContains("saints-field-first-item"))
        //     {
        //         first.AddToClassList("saints-field-first-item");
        //         StyleSheet uss = Util.LoadResource<StyleSheet>("UIToolkit/ListViewSkipRest.uss");
        //         Debug.Log($"uss={uss}");
        //         listView.styleSheets.Add(uss);
        //     }
        // }

        private static List<SerializedProperty> MakeSource(SerializedProperty arrayProp)
        {
            return Enumerable.Range(0, arrayProp.arraySize)
                .Select(arrayProp.GetArrayElementAtIndex).ToList();
        }
    }
}
#elif  !SAINTSFIELD_UI_TOOLKIT_DISABLE && (UNITY_2021_3_OR_NEWER || SAINTSFIELD_DEBUG_UNITY_BROKEN_FALLBACK)
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.TableDrawer
{
    public partial class TableAttributeDrawer
    {
        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            int propertyIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);
            if (propertyIndex != 0)
            {
                return new VisualElement();
            }

            // Action<object> onValueChangedCallback = null;
            // onValueChangedCallback = value =>
            // {
            //     object newFetchParent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
            //     if (newFetchParent == null)
            //     {
            //         Debug.LogWarning($"{property.propertyPath} parent disposed unexpectedly.");
            //         return;
            //     }
            //
            //     foreach (SaintsPropertyInfo saintsPropertyInfo in saintsPropertyDrawers)
            //     {
            //         saintsPropertyInfo.Drawer.OnValueChanged(
            //             property, saintsPropertyInfo.Attribute, saintsPropertyInfo.Index, containerElement,
            //             info, newFetchParent,
            //             onValueChangedCallback,
            //             value);
            //     }
            // };

            IMGUILabelHelper imguiLabelHelper = new IMGUILabelHelper(property.displayName);

            IMGUIContainer imGuiContainer = new IMGUIContainer(() =>
            {
                GUIContent label = imguiLabelHelper.NoLabel
                    ? GUIContent.none
                    : new GUIContent(imguiLabelHelper.RichLabel);

                property.serializedObject.Update();

                using(new ImGuiFoldoutStyleRichTextScoop())
                using(new ImGuiLabelStyleRichTextScoop())
                // using(EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                {
                    float height =
                        GetFieldHeight(property, label, Screen.width, saintsAttribute, info, true, parent);
                    Rect rect = EditorGUILayout.GetControlRect(true, height, GUILayout.ExpandWidth(true));

                    DrawField(rect, property, label, saintsAttribute, allAttributes, new OnGUIPayload(), info, parent);
                    // ReSharper disable once InvertIf
                    // if (changed.changed)
                    // {
                    //     object newFetchParent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
                    //     if (newFetchParent == null)
                    //     {
                    //         Debug.LogWarning($"{property.propertyPath} parent disposed unexpectedly.");
                    //         return;
                    //     }
                    //
                    //     (string error, int _, object value) = Util.GetValue(property, info, newFetchParent);
                    //     if (error == "")
                    //     {
                    //         onValueChangedCallback(value);
                    //     }
                    // }
                }

                property.serializedObject.ApplyModifiedProperties();
            })
            {
                style =
                {
                    flexGrow = 1,
                    flexShrink = 0,
                },
                userData = imguiLabelHelper,
            };
            imGuiContainer.AddToClassList(IMGUILabelHelper.ClassName);

            return imGuiContainer;
        }
    }
}

#endif

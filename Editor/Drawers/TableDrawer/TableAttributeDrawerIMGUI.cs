using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.ArraySizeDrawer;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.TableDrawer
{
    public partial class TableAttributeDrawer
    {
        private class SaintsTable : TreeView
        {
            public readonly SerializedProperty ArrayProp;
            private readonly IReadOnlyDictionary<int, IReadOnlyList<string>> _headerToPropNames;
            private readonly Type _elementType;

            public bool Changed;

            // public SaintsTable(TreeViewState state) : base(state)
            // {
            //     Reload();
            // }

            public SaintsTable(TreeViewState state, MultiColumnHeader multiColumnHeader, SerializedProperty arrayProp, IReadOnlyDictionary<int, IReadOnlyList<string>> headerToPropNames, Type elementType) : base(state, multiColumnHeader)
            {
                // Custom setup
                rowHeight = 20;
                // columnIndexForTreeFoldouts = 2;
                showAlternatingRowBackgrounds = true;
                showBorder = true;
                // customFoldoutYOffset = (kRowHeights - EditorGUIUtility.singleLineHeight) * 0.5f;
                // extraSpaceBeforeIconAndLabel = kToggleWidth;
                // multiColumnHeader.sortingChanged += OnSortingChanged;
                ArrayProp = arrayProp;
                _headerToPropNames = headerToPropNames;
                _elementType = elementType;

                Reload();
            }

            // private void OnSortingChanged(MultiColumnHeader header)
            // {
            //     Debug.Log(header);
            // }

            protected override TreeViewItem BuildRoot()
            {
                OnDestroy();

                TreeViewItem root = new TreeViewItem {id = -1, depth = -1, displayName = "Root"};

                List<TreeViewItem> allItems = Enumerable.Range(0, ArrayProp.arraySize).Select(index => new TreeViewItem { id = index, depth = 0, displayName = $"{index}" }).ToList();

                // Utility method that initializes the TreeViewItem.children and .parent for all items.
                SetupParentsAndChildrenFromDepths(root, allItems);

                // Return root of the tree
                return root;
            }

            protected override void RowGUI(RowGUIArgs args)
            {
                TreeViewItem item = args.item;
                for (int i = 0; i < args.GetNumVisibleColumns(); i++)
                {
                    CellGUI(args.GetCellRect(i), item, args.GetColumn(i));
                    // CellGUI(args.GetCellRect(i), item, (args.GetColumn(i) as MultiColumnHeaderState.Column).headerContent.text, ref args);
                }
            }

            private readonly Dictionary<int, SerializedObject> _serializedObjects = new Dictionary<int, SerializedObject>();

            public void OnDestroy()
            {
                foreach (SerializedObject so in _serializedObjects.Values)
                {
                    so.Dispose();
                }
                _serializedObjects.Clear();
            }

            protected override float GetCustomRowHeight(int row, TreeViewItem item)
            {
                // int elementIndex = item.id;
                // int arraySize = ArrayProp.arraySize;
                // if (elementIndex >= arraySize)
                // {
                //     return 0;
                // }

                SerializedProperty arrayItemProp = ArrayProp.GetArrayElementAtIndex(item.id);
                // Debug.Log(arrayItemProp.propertyPath);
                if (string.IsNullOrEmpty(arrayItemProp.propertyPath))
                {
                    return 0;
                }
                List<float> allHeight = new List<float>();

                foreach (IReadOnlyList<string> propNames in _headerToPropNames.Values)
                {
                    if(arrayItemProp.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        Object obj = arrayItemProp.objectReferenceValue;
                        if (obj == null)
                        {
                            return EditorGUIUtility.singleLineHeight;
                        }

                        if (!_serializedObjects.TryGetValue(item.id, out SerializedObject serObj))
                        {
                            serObj = new SerializedObject(obj);
                            _serializedObjects[item.id] = serObj;
                        }
                        // SerializedObject serObj = new SerializedObject(obj);
                        float height = propNames
                            .Select(propName => serObj.FindProperty(propName))
                            .Select(prop => EditorGUI.GetPropertyHeight(prop, prop.propertyType == SerializedPropertyType.Generic
                                ? new GUIContent(prop.displayName)
                                : GUIContent.none, true))
                            .Sum();
                        allHeight.Add(height);
                    }
                    else  // generic type
                    {
                        float height = propNames
                            .Select(propName => arrayItemProp.FindPropertyRelative(propName))
                            .Select(prop => EditorGUI.GetPropertyHeight(prop, prop.propertyType == SerializedPropertyType.Generic
                                ? new GUIContent(prop.displayName)
                                : GUIContent.none, true))
                            .Sum();
                        // SerializedProperty prop = arrayItemProp.FindPropertyRelative(propName);
                        allHeight.Add(height);
                    }
                }

                return allHeight.Max();
            }

            private void CellGUI(Rect getCellRect, TreeViewItem item, int getColumn)
            {
                if(item.id >= ArrayProp.arraySize)
                {
                    return;
                }

                SerializedProperty arrayItemProp = ArrayProp.GetArrayElementAtIndex(item.id);
                IReadOnlyList<string> propNames = _headerToPropNames[getColumn];

                if(arrayItemProp.propertyType == SerializedPropertyType.ObjectReference)
                {
                    Object obj = arrayItemProp.objectReferenceValue;
                    if (obj == null)
                    {
                        // ReSharper disable once ConvertToUsingDeclaration
                        using(EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                        {
                            Object newObj = EditorGUI.ObjectField(getCellRect, obj, _elementType, true);
                            // ReSharper disable once InvertIf
                            if (changed.changed)
                            {
                                arrayItemProp.objectReferenceValue = newObj;
                                arrayItemProp.serializedObject.ApplyModifiedProperties();
                                Reload();
                            }
                        }
                    }
                    else
                    {
                        if (!_serializedObjects.TryGetValue(item.id, out SerializedObject serObj))
                        {
                            serObj = new SerializedObject(obj);
                            _serializedObjects[item.id] = serObj;
                        }
                        // SerializedObject serObj = new SerializedObject(obj);
                        if (propNames.Count == 1)
                        {
                            SerializedProperty prop = serObj.FindProperty(propNames[0]);
                            GUIContent guiContent = prop.propertyType == SerializedPropertyType.Generic
                                ? new GUIContent(prop.displayName)
                                : GUIContent.none;
                            // ReSharper disable once ConvertToUsingDeclaration
                            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                            {
                                EditorGUI.PropertyField(getCellRect, prop,
                                    guiContent);
                                if (changed.changed)
                                {
                                    Changed = true;
                                }
                            }
                        }
                        else
                        {
                            Rect leftRect = getCellRect;
                            // ReSharper disable once ConvertToUsingDeclaration
                            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                            {
                                foreach (string propName in propNames)
                                {
                                    SerializedProperty prop = serObj.FindProperty(propName);

                                    GUIContent guiContent = prop.propertyType == SerializedPropertyType.Generic
                                        ? new GUIContent(prop.displayName)
                                        : GUIContent.none;

                                    float height = EditorGUI.GetPropertyHeight(prop, guiContent, true);
                                    (Rect useRect, Rect belowRect) = RectUtils.SplitHeightRect(leftRect, height);
                                    leftRect = belowRect;
                                    EditorGUI.PropertyField(useRect, prop,
                                        guiContent);

                                }

                                if (changed.changed)
                                {
                                    Changed = true;
                                }
                            }
                        }
                    }
                }
                else  // generic type
                {
                    if(propNames.Count == 1)
                    {
                        SerializedProperty prop = arrayItemProp.FindPropertyRelative(propNames[0]);
                        GUIContent guiContent = prop.propertyType == SerializedPropertyType.Generic
                            ? new GUIContent(prop.displayName)
                            : GUIContent.none;
                        // Debug.Log(getCellRect.height);
                        // ReSharper disable once ConvertToUsingDeclaration
                        using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                        {
                            EditorGUI.PropertyField(getCellRect, prop,
                                guiContent);
                            if(changed.changed)
                            {
                                Changed = true;
                            }
                        }
                    }
                    else
                    {
                        Rect leftRect = getCellRect;
                        // ReSharper disable once ConvertToUsingDeclaration
                        using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                        {
                            foreach (string propName in propNames)
                            {
                                SerializedProperty prop = arrayItemProp.FindPropertyRelative(propName);
                                GUIContent guiContent =
                                    prop.propertyType == SerializedPropertyType.Generic
                                        ? new GUIContent(prop.displayName)
                                        : GUIContent.none;
                                float height = EditorGUI.GetPropertyHeight(prop, guiContent, true);
                                (Rect useRect, Rect belowRect) = RectUtils.SplitHeightRect(leftRect, height);
                                leftRect = belowRect;
                                EditorGUI.PropertyField(useRect, prop, guiContent);
                            }

                            if (changed.changed)
                            {
                                Changed = true;
                            }
                        }
                    }
                }
            }

            private int[] _draggedPropertyIndexes = Array.Empty<int>();

            protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
            {
                // Debug.Log($"dragAndDropPosition={args.dragAndDropPosition}");
                // Debug.Log($"insertAtIndex={args.insertAtIndex}");
                // Debug.Log($"parentItem={args.parentItem}");
                // Debug.Log($"performDrop={args.performDrop}");
                // ReSharper disable once InvertIf
                if (args.performDrop && _draggedPropertyIndexes.Length > 0)
                {
                    // Debug.Log($"{string.Join(", ", _draggedPropertyIndexes)} -> {args.insertAtIndex}: {args.dragAndDropPosition}");
                    foreach (int index in _draggedPropertyIndexes)
                    {
                        // the last will give index+1, wtf...
                        ArrayProp.MoveArrayElement(index, Mathf.Min(args.insertAtIndex, ArrayProp.arraySize - 1));
                        ArrayProp.serializedObject.ApplyModifiedProperties();
                    }

                    _draggedPropertyIndexes = Array.Empty<int>();
                    EditorApplication.delayCall += OnDestroy;
                }

                // return DragAndDropVisualMode.None;
                return DragAndDropVisualMode.Move;
            }

            protected override bool CanStartDrag(CanStartDragArgs args)
            {
                _draggedPropertyIndexes = args.draggedItemIDs.OrderByDescending(each => each).ToArray();
                // Debug.Log(string.Join(", ", _draggedPropertyIndexes));
                // _draggedPropertyIndex = args.draggedItem.id;
                // Debug.Log(args.draggedItem.id);
                // Debug.Log(args.draggedItemIDs);
                return true;
            }

            protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
            {
                DragAndDrop.PrepareStartDrag();
                // DragAndDrop.paths = null;
                // DragAndDrop.objectReferences = new Object[] { };
                // DragAndDrop.SetGenericData("SaintsTable", this);
                DragAndDrop.StartDrag("SaintsTable");
            }
        }

        private SaintsTable _saintsTable;
        private string _error;
        private bool _isObjectReference;

        // private int _curSize;

        protected override void OnDisposeIMGUI()
        {
            // ReSharper disable once InvertIf
            if (_saintsTable != null)
            {
                _saintsTable.OnDestroy();
                _saintsTable = null;
            }
        }

        private int _preArraySizeImGui = -1;

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, FieldInfo info,
            bool hasLabelWidth, object parent)
        {
            int propertyIndex;
            try
            {
                propertyIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);
            }
            catch (NullReferenceException)
            {
                return 0;
            }
            catch (ObjectDisposedException)
            {
                return 0;
            }

            if (propertyIndex != 0)
            {
                return 0;
            }

            (string error, SerializedProperty arrayProp) = SerializedUtils.GetArrayProperty(property);

            if (_preArraySizeImGui == -1)
            {
                _preArraySizeImGui = arrayProp.arraySize;
            }

            _error = error;
            if (_error != "")
            {
                return SingleLineHeight;
            }

            if (_saintsTable == null)
            {
                float properFullWidth = Mathf.Max(20, width - IndentWidth * 2 - 20);

                Dictionary<int, IReadOnlyList<string>> headerToPropNames = new Dictionary<int, IReadOnlyList<string>>();

                _isObjectReference = arrayProp.GetArrayElementAtIndex(0).propertyType == SerializedPropertyType.ObjectReference;
                List<MultiColumnHeaderState.Column> columns = new List<MultiColumnHeaderState.Column>();
                if(_isObjectReference)
                {
                    Object obj0 = Enumerable.Range(0, arrayProp.arraySize).Select(arrayProp.GetArrayElementAtIndex)
                        .Select(each => each.objectReferenceValue).FirstOrDefault(each => each != null);
                    if(obj0 == null)
                    {
                        return SingleLineHeight;
                    }

                    // ReSharper disable once ConvertToUsingDeclaration
                    using (SerializedObject serializedObject = new SerializedObject(obj0))
                    {
                        Dictionary<string, List<SerializedPropertyInfo>> columnToProperties = new Dictionary<string, List<SerializedPropertyInfo>>();
                        foreach (SerializedProperty serializedProperty in SerializedUtils.GetAllField(serializedObject))
                        {
                            (TableColumnAttribute[] tableColumnAttributes, object _) = SerializedUtils.GetAttributesAndDirectParent<TableColumnAttribute>(serializedProperty);
                            string columnName = tableColumnAttributes.Length > 0? tableColumnAttributes[0].Title: serializedProperty.displayName;
                            if(!columnToProperties.TryGetValue(columnName, out List<SerializedPropertyInfo> list))
                            {
                                columnToProperties[columnName] = list = new List<SerializedPropertyInfo>();
                            }
                            list.Add(new SerializedPropertyInfo
                            {
                                Name = serializedProperty.name,
                                PropertyPath = serializedProperty.propertyPath,
                            });
                        }

                        float useWidth = properFullWidth / columnToProperties.Count;

                        // ReSharper disable once UseDeconstruction
                        foreach ((KeyValuePair<string, List<SerializedPropertyInfo>> columnKv, int index) in columnToProperties.WithIndex())
                        {
                            string columnName = columnKv.Key;
                            List<SerializedPropertyInfo> properties = columnKv.Value;

                            columns.Add(new MultiColumnHeaderState.Column
                            {
                                headerContent = new GUIContent(columnName),
                                headerTextAlignment = TextAlignment.Left,
                                sortedAscending = true,
                                sortingArrowAlignment = TextAlignment.Right,
                                width = useWidth,
                                minWidth = 60,
                                autoResize = true,
                                allowToggleVisibility = true,
                            });
                            headerToPropNames[index] = properties.Select(each => each.Name).ToArray();
                        }
                    }
                }
                else
                {
                    SerializedProperty firstProp = arrayProp.GetArrayElementAtIndex(0);

                    Dictionary<string, List<SerializedPropertyInfo>> columnToProperties = new Dictionary<string, List<SerializedPropertyInfo>>();

                    foreach (SerializedProperty serializedProperty in SerializedUtils.GetPropertyChildren(firstProp)
                                 .Where(each => each != null))
                    {
                        (TableColumnAttribute[] tableColumnAttributes, object _) = SerializedUtils.GetAttributesAndDirectParent<TableColumnAttribute>(serializedProperty);
                        string columnName = tableColumnAttributes.Length > 0? tableColumnAttributes[0].Title: serializedProperty.displayName;
                        if(!columnToProperties.TryGetValue(columnName, out List<SerializedPropertyInfo> list))
                        {
                            columnToProperties[columnName] = list = new List<SerializedPropertyInfo>();
                        }
                        list.Add(new SerializedPropertyInfo
                        {
                            Name = serializedProperty.name,
                            PropertyPath = serializedProperty.propertyPath,
                        });
                    }

                    float useWidth = properFullWidth / columnToProperties.Count;

                    // ReSharper disable once UseDeconstruction
                    foreach ((KeyValuePair<string, List<SerializedPropertyInfo>> columnKv, int index) in columnToProperties.WithIndex())
                    {
                        string columnName = columnKv.Key;
                        List<SerializedPropertyInfo> properties = columnKv.Value;
                        IReadOnlyList<string> propNames = properties.Select(each => each.Name).ToArray();

                        // string id = string.Join(";", properties.Select(each => each.PropertyPath));
                        columns.Add(new MultiColumnHeaderState.Column
                        {
                            headerContent = new GUIContent(columnName),
                            headerTextAlignment = TextAlignment.Left,
                            sortedAscending = true,
                            sortingArrowAlignment = TextAlignment.Right,
                            width = useWidth,
                            minWidth = 60,
                            autoResize = true,
                            allowToggleVisibility = true,
                        });
                        headerToPropNames[index] = propNames;
                    }

                }
                _saintsTable = new SaintsTable(
                    new TreeViewState(),
                    new MultiColumnHeader(new MultiColumnHeaderState(columns.ToArray())),
                    arrayProp,
                    headerToPropNames,
                    ReflectUtils.GetElementType(info.FieldType)
                );
            }

            // if (_curSize != arrayProp.arraySize)
            // {
            //     _curSize = arrayProp.arraySize;
            //     // _saintsTable.Reload();
            // }
            // _saintsTable.Reload();

            if (_preArraySizeImGui != arrayProp.arraySize)
            {
                _preArraySizeImGui = arrayProp.arraySize;
                _saintsTable.Reload();
            }

            // ReSharper disable once InvertIf
            if (_saintsTable.Changed)
            {
                _saintsTable.Changed = false;
                _saintsTable.Reload();
            }

            // Debug.Log($"tableHeight={_saintsTable.totalHeight}, minHeight={EditorGUIUtility.singleLineHeight * (arrayProp.arraySize + 1)}, SingleLineHeight={SingleLineHeight}");


            return Mathf.Max(_saintsTable.totalHeight, EditorGUIUtility.singleLineHeight * (arrayProp.arraySize + 1)) + SingleLineHeight;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes,
            OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            TableAttribute tableAttribute = (TableAttribute) saintsAttribute;

            int propertyIndex;
            try
            {
                propertyIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);
            }
            catch (NullReferenceException)
            {
                return;
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            if (propertyIndex != 0)
            {
                return;
            }

            if (_error != "" || _saintsTable == null)
            {
                DefaultDrawer(position, property, label, info);
                return;
            }

            (Rect controlRect, Rect tableRect) = RectUtils.SplitHeightRect(position, SingleLineHeight);
            const int arraySizeWidth = 50;
            float rightWidth = arraySizeWidth + EditorGUIUtility.singleLineHeight * 2;
            (Rect _, Rect rightRect) = RectUtils.SplitWidthRect(new Rect(controlRect)
            {
                y = controlRect.y + 1,
                height = controlRect.height -2,
            }, controlRect.width - rightWidth);

            ArraySizeAttribute arraySizeAttribute = allAttributes.OfType<ArraySizeAttribute>().FirstOrDefault();
            int min = -1;
            int max = -1;
            if (arraySizeAttribute != null)
            {
                (string error, bool dynamic, int min, int max) arraySize = ArraySizeAttributeDrawer.GetMinMax(arraySizeAttribute, property, info, parent);
                if(arraySize.error == "")
                {
                    min = arraySize.min;
                    max = arraySize.max;
                }
#if SAINTSFIELD_DEBUG
                else
                {
                    Debug.LogError(arraySize.error);
                }
#endif
                // Debug.Log($"{min} ~ {max}");
            }

            (Rect numberRect, Rect controlsRect) = RectUtils.SplitWidthRect(rightRect, arraySizeWidth);
            if (tableAttribute.HideRemoveButton && tableAttribute.HideAddButton)  // remove space
            {
                // numberRect = new Rect(numberRect)
                // {
                //     x = numberRect.x + arraySizeWidth,
                //     width = numberRect.width - arraySizeWidth,
                // };
                numberRect = rightRect;
            }

            // Debug.Log($"{min} ~ {max}");
            using(new EditorGUI.DisabledScope(tableAttribute.HideRemoveButton && tableAttribute.HideAddButton))
            using(new EditorGUI.DisabledScope(min > 0 && min == max))
            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                int newSize = EditorGUI.DelayedIntField(numberRect, _saintsTable.ArrayProp.arraySize);
                if (changed.changed)
                {
                    if(min > 0 && newSize < min)
                    {
                        newSize = min;
                    }
                    else if(max > 0 && newSize > max)
                    {
                        newSize = max;
                    }
                    if(newSize != _saintsTable.ArrayProp.arraySize)
                    {
                        ChangeArraySize(newSize, _saintsTable.ArrayProp);
                        _saintsTable.Reload();
                    }
                }
            }

            (Rect plusButton, Rect minusButton) = RectUtils.SplitWidthRect(controlsRect, EditorGUIUtility.singleLineHeight);

            if(!tableAttribute.HideAddButton)
            {
                using (new EditorGUI.DisabledScope(max >= 0 && _saintsTable.ArrayProp.arraySize >= max))
                {
                    if (GUI.Button(plusButton, "+"))
                    {
                        ChangeArraySize(_saintsTable.ArrayProp.arraySize + 1, _saintsTable.ArrayProp);
                        _saintsTable.Reload();
                    }
                }
            }

            // Debug.Log($"{_saintsTable.ArrayProp.arraySize} {max} {min}");
            if(!tableAttribute.HideRemoveButton)
            {
                using (new EditorGUI.DisabledScope(min >= 0 && _saintsTable.ArrayProp.arraySize <= min))
                {
                    if (GUI.Button(minusButton, "-"))
                    {
                        DeleteArrayElement(_saintsTable.ArrayProp, _saintsTable.GetSelection());
                        _saintsTable.Reload();
                    }
                }
            }

            try
            {
                _saintsTable.OnGUI(tableRect);
            }
            catch (NullReferenceException)
            {
                OnDisposeIMGUI();
            }
            catch (ObjectDisposedException)
            {
                OnDisposeIMGUI();
            }
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute, int index, FieldInfo info,
            object parent)
        {
            return _error != "";
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute,
            int index, FieldInfo info, object parent)
        {
            return ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property,
            GUIContent label, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, OnGUIPayload onGuiPayload, FieldInfo info, object parent)
        {
            return ImGuiHelpBox.Draw(position, _error, MessageType.Error);
        }
    }
}

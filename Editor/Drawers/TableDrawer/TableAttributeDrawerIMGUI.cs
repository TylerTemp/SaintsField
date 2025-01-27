using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Utils;
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
            private readonly IReadOnlyDictionary<int, string> _headerToPropName;
            private readonly Type _elementType;

            // public SaintsTable(TreeViewState state) : base(state)
            // {
            //     Reload();
            // }

            public SaintsTable(TreeViewState state, MultiColumnHeader multiColumnHeader, SerializedProperty arrayProp, IReadOnlyDictionary<int, string> headerToPropName, Type elementType) : base(state, multiColumnHeader)
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
                _headerToPropName = headerToPropName;
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
                    CellGUI(args.GetCellRect(i), item, args.GetColumn(i), ref args);
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
                SerializedProperty arrayItemProp = ArrayProp.GetArrayElementAtIndex(item.id);
                List<float> allHeight = new List<float>();

                foreach (string propName in _headerToPropName.Values)
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
                        SerializedProperty prop = serObj.FindProperty(propName);
                        allHeight.Add(EditorGUI.GetPropertyHeight(prop, GUIContent.none, true));
                    }
                    else  // generic type
                    {
                        SerializedProperty prop = arrayItemProp.FindPropertyRelative(propName);
                        allHeight.Add(EditorGUI.GetPropertyHeight(prop, GUIContent.none, true));
                    }
                }

                return allHeight.Max();
            }

            private void CellGUI(Rect getCellRect, TreeViewItem item, int getColumn, ref RowGUIArgs args)
            {
                SerializedProperty arrayItemProp = ArrayProp.GetArrayElementAtIndex(item.id);
                string propName = _headerToPropName[getColumn];

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
                        SerializedProperty prop = serObj.FindProperty(propName);
                        EditorGUI.PropertyField(getCellRect, prop,
                            prop.propertyType == SerializedPropertyType.Generic
                                ? new GUIContent(prop.displayName)
                                : GUIContent.none);
                    }
                }
                else  // generic type
                {
                    SerializedProperty prop = arrayItemProp.FindPropertyRelative(propName);
                    EditorGUI.PropertyField(getCellRect, prop,
                        prop.propertyType == SerializedPropertyType.Generic
                            ? new GUIContent(prop.displayName)
                            : GUIContent.none);
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

        protected override void OnDisposeIMGUI()
        {
            // ReSharper disable once InvertIf
            if (_saintsTable != null)
            {
                _saintsTable.OnDestroy();
                _saintsTable = null;
            }
        }

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute, FieldInfo info,
            bool hasLabelWidth, object parent)
        {
            int propertyIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);
            if (propertyIndex != 0)
            {
                return 0;
            }

            (string error, SerializedProperty arrayProp) = SerializedUtils.GetArrayProperty(property);
            _error = error;
            if (_error != "")
            {
                return SingleLineHeight;
            }

            if (_saintsTable == null)
            {
                Dictionary<int, string> headerToPropName = new Dictionary<int, string>();

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

                    using (SerializedObject serializedObject = new SerializedObject(obj0))
                    {
                        foreach ((SerializedProperty serializedProperty, int index) in SerializedUtils
                                     .GetAllField(serializedObject).WithIndex())
                        {
                            columns.Add(new MultiColumnHeaderState.Column
                            {
                                headerContent = new GUIContent(serializedProperty.displayName),
                                headerTextAlignment = TextAlignment.Left,
                                sortedAscending = true,
                                sortingArrowAlignment = TextAlignment.Right,
                                width = 100,
                                minWidth = 60,
                                autoResize = true,
                                allowToggleVisibility = true
                            });
                            headerToPropName[index] = serializedProperty.name;
                        }
                    }
                }
                else
                {
                    SerializedProperty firstProp = arrayProp.GetArrayElementAtIndex(0);
                    foreach ((SerializedProperty serializedProperty, int index) in SerializedUtils.GetPropertyChildren(firstProp)
                                 .Where(each => each != null).WithIndex())
                    {
                        string propName = serializedProperty.name;
                        columns.Add(new MultiColumnHeaderState.Column
                        {
                            headerContent = new GUIContent(propName),
                            headerTextAlignment = TextAlignment.Left,
                            sortedAscending = true,
                            sortingArrowAlignment = TextAlignment.Right,
                            width = 100,
                            minWidth = 60,
                            autoResize = true,
                            allowToggleVisibility = true,
                        });
                        headerToPropName[index] = propName;
                    }
                }
                _saintsTable = new SaintsTable(
                    new TreeViewState(),
                    new MultiColumnHeader(new MultiColumnHeaderState(columns.ToArray())),
                    arrayProp,
                    headerToPropName,
                    ReflectUtils.GetElementType(info.FieldType)
                );
            }

            return Mathf.Max(_saintsTable.totalHeight, EditorGUIUtility.singleLineHeight * (arrayProp.arraySize + 1)) + SingleLineHeight;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute,
            OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            int propertyIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);
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

            (Rect numberRect, Rect controlsRect) = RectUtils.SplitWidthRect(rightRect, arraySizeWidth);
            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                int newSize = EditorGUI.DelayedIntField(numberRect, _saintsTable.ArrayProp.arraySize);
                if (changed.changed)
                {
                    ChangeArraySize(newSize, _saintsTable.ArrayProp);
                    _saintsTable.Reload();
                }
            }

            (Rect plusButton, Rect minusButton) = RectUtils.SplitWidthRect(controlsRect, EditorGUIUtility.singleLineHeight);
            if(GUI.Button(plusButton, "+"))
            {
                ChangeArraySize(_saintsTable.ArrayProp.arraySize + 1, _saintsTable.ArrayProp);
                _saintsTable.Reload();
            }
            if(GUI.Button(minusButton, "-"))
            {
                DeleteArrayElement(_saintsTable.ArrayProp, _saintsTable.GetSelection());
                _saintsTable.Reload();
            }

            _saintsTable.OnGUI(tableRect);
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

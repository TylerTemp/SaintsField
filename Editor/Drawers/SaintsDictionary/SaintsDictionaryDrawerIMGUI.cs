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

namespace SaintsField.Editor.Drawers.SaintsDictionary
{
    public partial class SaintsDictionaryDrawer
    {
        private class SaintsDictionaryTable : TreeView
        {
            public readonly SerializedProperty KeysProp;
            public readonly SerializedProperty ValuesProp;

            public readonly SaintsDictionaryAttribute SaintsDictionaryAttribute;
            // private readonly IReadOnlyDictionary<int, IReadOnlyList<string>> _headerToPropNames;
            // private readonly Type _elementType;

            // public SaintsTable(TreeViewState state) : base(state)
            // {
            //     Reload();
            // }

            public SaintsDictionaryTable(TreeViewState state, MultiColumnHeader multiColumnHeader, SaintsDictionaryAttribute saintsDictionaryAttribute, SerializedProperty keysProp, SerializedProperty valuesProp) : base(state, multiColumnHeader)
            {
                // Custom setup
                rowHeight = 20;
                // columnIndexForTreeFoldouts = 2;
                showAlternatingRowBackgrounds = true;
                showBorder = true;
                // customFoldoutYOffset = (kRowHeights - EditorGUIUtility.singleLineHeight) * 0.5f;
                // extraSpaceBeforeIconAndLabel = kToggleWidth;
                // multiColumnHeader.sortingChanged += OnSortingChanged;
                // ArrayProp = arrayProp;
                // _headerToPropNames = headerToPropNames;
                // _elementType = elementType;

                SaintsDictionaryAttribute = saintsDictionaryAttribute ?? new SaintsDictionaryAttribute();
                KeysProp = keysProp;
                ValuesProp = valuesProp;

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
                SerializedProperty arrayItemProp = ArrayProp.GetArrayElementAtIndex(item.id);
                IReadOnlyList<string> propNames = _headerToPropNames[getColumn];

                if (string.IsNullOrEmpty(arrayItemProp.propertyPath))
                {
                    return;
                }

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
                            EditorGUI.PropertyField(getCellRect, prop,
                                guiContent);
                        }
                        else
                        {
                            Rect leftRect = getCellRect;
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
                        EditorGUI.PropertyField(getCellRect, prop,
                            guiContent);
                    }
                    else
                    {
                        Rect leftRect = getCellRect;
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

        protected override bool UseCreateFieldIMGUI => true;

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute, FieldInfo info,
            bool hasLabelWidth, object parent)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes, OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            EditorGUI.DrawRect(position, WarningColor);
        }
    }
}

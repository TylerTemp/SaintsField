#if UNITY_2021_3_OR_NEWER
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.ColorPalette.UIToolkit
{
#if UNITY_6000_0_OR_NEWER
    [UxmlElement]
#endif
    // ReSharper disable once ClassNeverInstantiated.Global
    // ReSharper disable once PartialTypeWithSinglePart
    public partial class ColorPaletteLabels: BindableElement
    {
        // ReSharper disable once UnusedMember.Global
        public ColorPaletteLabels() : this(null, null)
        {

        }

        private readonly VisualElement _containerRoot;
        private readonly SerializedProperty _arrayProp;

        public ColorPaletteLabels(VisualElement containerRoot, SerializedProperty arrayProp)
        {
            _containerRoot = containerRoot;

            style.flexDirection = FlexDirection.Row;
            style.flexWrap = Wrap.Wrap;
            style.alignItems = Align.FlexStart;

            // style.backgroundColor = Color.blue;

            if (arrayProp != null)
            {
                _arrayProp = arrayProp;
                bindingPath = arrayProp.propertyPath;
                this.TrackPropertyValue(arrayProp, OnTrackPropertyValue);
                OnTrackPropertyValue(arrayProp);
            }

            // this.RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanelEvent);
            // Debug.Log($"ColorPaletteLabels={value.Length}");
            // SetValueWithoutNotify(value);
        }

        private int _arraySize;
        public readonly List<ColorPaletteLabel> Labels = new List<ColorPaletteLabel>();

        private void OnTrackPropertyValue(SerializedProperty sp)
        {
            if (_detached)
            {
                return;
            }

            int newSize = sp.arraySize;
            if (_arraySize == newSize)
            {
                return;
            }

            if (newSize > _arraySize)
            {
                // Increase array size
                // Debug.Log($"increase size {_arraySize} -> {newSize} {GetHashCode()}");
                for (int i = _arraySize; i < newSize; i++)
                {
                    int thisIndex = i;
                    ColorPaletteLabel label = new ColorPaletteLabel();
                    SerializedProperty arrayElementProp = sp.GetArrayElementAtIndex(i);
                    label.BindProperty(arrayElementProp);
                    // Debug.Log($"increase size add {arrayElementProp.stringValue}@{i}: {arrayElementProp.propertyPath}");
                    label.OnDeleteClicked.AddListener(() =>
                    {
                        // Debug.Log($"Remove label {i}");
                        sp.DeleteArrayElementAtIndex(thisIndex);
                        sp.serializedObject.ApplyModifiedProperties();
                    });
                    Labels.Insert(i, label);
                    // Add(label);
                    Insert(i, label);

                    if (_allContainer != null)
                    {
                        LabelPointerManipulator _ = new LabelPointerManipulator(label, this, _allContainer);
                    }
                }

                // for (int i = 0; i < Labels.Count; i++)
                // {
                //     Debug.Log($"rebind {i} for {_arrayProp.propertyPath}");
                //     Labels[i].BindProperty(sp.GetArrayElementAtIndex(i));
                // }
            }
            else if (newSize < _arraySize)
            {
                // Debug.Log($"decrease size {_arraySize} -> {newSize}");
                Labels.Clear();

                int index = 0;
                foreach (ColorPaletteLabel colorPaletteLabel in Children().OfType<ColorPaletteLabel>().ToArray())
                {
                    if(index < newSize)
                    {
                        colorPaletteLabel.BindProperty(sp.GetArrayElementAtIndex(index));
                        Labels.Add(colorPaletteLabel);
                    }
                    else
                    {
                        // Debug.Log($"remove {colorPaletteLabel.value} for {_arrayProp.propertyPath}");
                        colorPaletteLabel.RemoveFromHierarchy();
#if UNITY_6000_0_OR_NEWER
                        colorPaletteLabel.Unbind();
#endif
                    }
                    index++;
                }
                Debug.Assert(Labels.Count == newSize);
            }

            _arraySize = newSize;
        }

        private bool _detached;

        private void OnDetachFromPanelEvent(DetachFromPanelEvent evt)
        {
            _detached = true;
        }

        private ColorPaletteLabelPlaceholder _placeholder;

        // private int _dragOverIndex = -1;

        public (bool isOver, Vector2 offset) DragOver(Vector2 worldMousePos, ColorPaletteLabel targetLabel)
        {
            (int index, Vector3 placedTargetRealPos) = GetDragIndex(worldMousePos, targetLabel);
            if (index < 0)
            {
                RemovePlaceholder();
                return (false, placedTargetRealPos);
            }

            int currentIndex = Labels.FindIndex(each => each == targetLabel);
            if(currentIndex + 1 != index)
            {
                Insert(index, _placeholder ??= new ColorPaletteLabelPlaceholder(targetLabel.value));
            }
            else
            {
                RemovePlaceholder();
            }
            return (true, placedTargetRealPos);
        }

        private (int index, Vector3 placedTargetRealPos) GetDragIndex(Vector2 worldMousePos, ColorPaletteLabel target)
        {
            // // Debug.Log(worldMousePos);
            // // const int insertIndex = 0;
            // int labelsCount = _worldBounds.Length;
            // for (int checkIndex = 0; checkIndex < labelsCount; checkIndex++)
            // {
            //     Rect wb = _worldBounds[checkIndex];
            //     if (wb.Contains(worldMousePos))
            //     {
            //         if (Labels[checkIndex] == target)
            //         {
            //             return -1;
            //         }
            //         return checkIndex;
            //     }
            // }
            //
            // // Debug.Log($"{worldMousePos.y}: {string.Join(",", Labels.Select(l => l.worldBound.yMax))}");
            //
            // if (labelsCount > 0 && _worldBounds.All(wb => wb.yMin < worldMousePos.y))
            // {
            //     // Debug.Log($"{GetHashCode()} drag over use last {labelsCount}");
            //     return labelsCount;
            // }
            //
            // // Debug.Log($"{GetHashCode()} drag over no match use 0");
            // return 0;

            if (!worldBound.Contains(worldMousePos))
            {
                return (-1, Vector2.zero);
            }

            int allCount = _worldBounds.Length;
            int currentIndex = Labels.FindIndex(each => each == target);
            bool isFromOtherLabel = currentIndex == -1;
            // Debug.Log($"{target.value} -> {string.Join(", ", Labels.Select(l => l.value))}; currentIndex={currentIndex}");
            // Debug.Assert(currentIndex >= 0);

            // bool found = false;
            for (int index = 0; index < _worldBounds.Length; index++)
            {
                Rect eachBound = _worldBounds[index];

                // ReSharper disable once InvertIf
                if (eachBound.Contains(worldMousePos))
                {
                    int placeIndex = index;
                    // Container eachContainer = _allContainers[index];
                    if (placeIndex == currentIndex)
                    {
                        return (-1, Vector3.zero);
                    }

                    if (isFromOtherLabel)
                    {
                        return (placeIndex, Vector3.zero);
                    }

                    bool isPre = placeIndex < currentIndex;
                    Vector2 currentIndexPos = _worldPos[currentIndex];
                    Vector2 pushedIndexPos = currentIndexPos;
                    if (isPre)
                    {
                        // Debug.Log($"isPre, currentIndex={currentIndex}");
                        int pushedAfterIndex = currentIndex + 1;
                        // if (pushedAfterIndex < _worldBounds.Length)
                        // {
                        //     // Debug.Log($"isPre, pushedAfterIndex={pushedAfterIndex}, pushedIndexPos={pushedIndexPos}");
                        // }

                        // last one, how do we get the fucking pushed position...?
                        pushedIndexPos = _worldPos[pushedAfterIndex];
                    }

                    // Debug.Log($"PlaceIndex={placeIndex}");
                    return (placeIndex, pushedIndexPos - currentIndexPos);


                }
            }

            if (allCount > 0 && _worldBounds.All(wb => wb.yMin < worldMousePos.y))
            {
                return (allCount, Vector3.zero);
            }

            if (isFromOtherLabel)
            {
                return (0, Vector3.zero);
            }

            Vector2 currentPos = _worldPos[currentIndex];
            Vector2 pushedPos = currentPos;
            if (currentIndex + 1 < _worldBounds.Length)
            {
                pushedPos = _worldPos[currentIndex + 1];
            }

            return (0, pushedPos - currentPos);
        }

        public void RemovePlaceholder()
        {
            _placeholder?.RemoveFromHierarchy();
            _placeholder = null;
        }

        // public void StartDrag(ColorPaletteLabel targetLabel)
        // {
        //     Remove(targetLabel);
        // }

        // public void RestoreLabel(ColorPaletteLabel targetLabel)
        // {
        //     int index = Labels.IndexOf(targetLabel);
        //     Debug.Assert(index != -1, targetLabel.value);
        //     Insert(index, targetLabel);
        // }

        // return: is new
        public bool AddOrSwap(Vector2 worldMousePos, ColorPaletteLabel targetLabel)
        {
            int existsIndex = Labels.IndexOf(targetLabel);
            // Debug.Log($"target={targetLabel.value}({targetLabel.GetHashCode()}); Labels={string.Join(", ", Labels.Select(l => $"{l.value}({l.GetHashCode()})"))}; existsIndex={existsIndex}");
            int insertIndex = GetDragIndex(worldMousePos, targetLabel).index;

            if (insertIndex == -1)
            {
                // Debug.Log("no index, skip");
                return false;
            }

            bool isNew = existsIndex == -1;
            if (isNew)  // is new
            {
                // Debug.Log($"insert new {targetLabel.value}@{insertIndex}; arraySize={_arrayProp.arraySize}");
                // ReSharper disable once ExtractCommonBranchingCode
                _arrayProp.InsertArrayElementAtIndex(insertIndex);
                _arrayProp.GetArrayElementAtIndex(insertIndex).stringValue = targetLabel.value;
            }
            else  // shifting
            {
                if (insertIndex > existsIndex)
                {
                    insertIndex--;
                }

                if (insertIndex >= Labels.Count)  // just swap to end
                {
                    insertIndex = Labels.Count - 1;
                }
                else if (insertIndex < 0)
                {
                    insertIndex = 0;
                }

                if (existsIndex == insertIndex)
                {
                    // Debug.Log($"no swap {existsIndex} <-> {insertIndex}");
                    return false;
                }

                // Debug.Log($"swap {existsIndex} <-> {insertIndex} ({GetHashCode()})");
                _arrayProp.MoveArrayElement(existsIndex, insertIndex);
                // (_arrayProp.GetArrayElementAtIndex(existsIndex).stringValue,
                //         _arrayProp.GetArrayElementAtIndex(insertIndex).stringValue) =
                //     (_arrayProp.GetArrayElementAtIndex(insertIndex).stringValue,
                //         _arrayProp.GetArrayElementAtIndex(existsIndex).stringValue);
                // Labels.Remove(targetLabel);
                // Remove(targetLabel);
                // _arrayProp.DeleteArrayElementAtIndex(existsIndex);
                // _arrayProp.InsertArrayElementAtIndex(insertIndex);
                // _arrayProp.GetArrayElementAtIndex(insertIndex).stringValue = targetLabel.value;
                // Labels.Insert(insertIndex, targetLabel);
                // Insert(insertIndex, targetLabel);
            }

            _arrayProp.serializedObject.ApplyModifiedProperties();
            return isNew;
        }

        public void RemoveLabel(ColorPaletteLabel targetLabel)
        {
            for (int index = 0; index < Labels.Count; index++)
            {
                ColorPaletteLabel label = Labels[index];
                if (label == targetLabel)
                {
                    // Debug.Log($"remove {targetLabel.value}@{index}");
                    // Labels.RemoveAt(index);
                    targetLabel.RemoveFromHierarchy();

                    _arrayProp.DeleteArrayElementAtIndex(index);
                    _arrayProp.serializedObject.ApplyModifiedProperties();

                    return;
                }
            }

            // Debug.LogError($"index of {targetLabel.value} not found in Labels {string.Join(", ", Labels.Select(l => l.value))}");
        }

        private IReadOnlyList<ColorInfoArray.Container> _allContainer;

        public void BindAllColorPaletteLabels(List<ColorInfoArray.Container> allColorPaletteLabels)
        {
            _allContainer = allColorPaletteLabels;
        }

        private Rect[] _worldBounds;
        private Vector2[] _worldPos;

        public void FrozenPositions(CleanableLabelInputTypeAhead typeAhead)
        {
            // Debug.Log(typeAhead);
            _worldBounds = Labels.Select(each => each.worldBound).ToArray();
            _worldPos = Labels
                .Select(each => each.worldBound.position)
                .Append(typeAhead.worldBound.position)
                .ToArray();

            // Debug.Log($"_worldPos={string.Join(", ", _worldPos)}");
        }
    }
}
#endif

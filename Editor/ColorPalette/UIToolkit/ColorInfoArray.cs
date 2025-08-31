#if UNITY_2021_3_OR_NEWER
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.ColorPalette.UIToolkit
{
    public class ColorInfoArray: VisualElement
    {
        public readonly struct Container
        {
            public readonly TemplateContainer Root;
            public readonly VisualElement ContainerRoot;
            public readonly VisualElement MoveIcon;
            public readonly ColorField ColorField;
            public readonly Button DeleteButton;
            public readonly ColorPaletteLabels ColorPaletteLabels;
            public readonly CleanableLabelInputTypeAhead CleanableLabelInputTypeAhead;

            private Container(TemplateContainer root, VisualElement containerRoot, VisualElement moveIcon,
                ColorField colorField, Button deleteButton, ColorPaletteLabels colorPaletteLabels,
                CleanableLabelInputTypeAhead cleanableLabelInputTypeAhead)
            {
                Root = root;
                ContainerRoot = containerRoot;
                MoveIcon = moveIcon;
                ColorField = colorField;
                DeleteButton = deleteButton;
                ColorPaletteLabels = colorPaletteLabels;
                CleanableLabelInputTypeAhead = cleanableLabelInputTypeAhead;
            }

            private const string ColorInfoContainerName = "color-info-container";

            public static Container CreateContainer(SerializedProperty colorInfoLabelsProp, CleanableLabelInputTypeAhead cleanableLabelInputTypeAhead)
            {
                Container result = CreateEmpty();

                ColorPaletteLabels colorPaletteLabels = new ColorPaletteLabels(result.ContainerRoot, colorInfoLabelsProp);
                colorPaletteLabels.Add(cleanableLabelInputTypeAhead);
                result.ContainerRoot.Add(colorPaletteLabels);
                return new Container(result.Root, result.ContainerRoot, result.MoveIcon, result.ColorField, result.DeleteButton, colorPaletteLabels, cleanableLabelInputTypeAhead);
            }

            public static Container CreateEmpty()
            {
                VisualTreeAsset containerTree = Util.LoadResource<VisualTreeAsset>("UIToolkit/ColorPalette/Container.uxml");

                TemplateContainer root = containerTree.CloneTree();
                root.name = ColorInfoContainerName;

                VisualElement containerRoot = root.Q<VisualElement>("container-root");

                VisualElement colorContainer = containerRoot.Q<VisualElement>("color-container");

                VisualElement moveButton = colorContainer.Q<VisualElement>("move");

                ColorField colorField = colorContainer.Q<ColorField>("color");
                // colorField.BindProperty(colorInfoColorProp);

                Button deleteButton = colorContainer.Q<Button>("delete");

                return new Container(root, containerRoot, moveButton, colorField, deleteButton, null, null);
            }
        }

        private readonly ScrollView _rootScoller;
        private readonly SerializedProperty _colorInfoArrayProp;

        // ReSharper disable once UnusedMember.Global
        public ColorInfoArray(): this(null, null){}

        public ColorInfoArray(ScrollView rootScoller, SerializedProperty colorInfoArrayProp)
        {
            _rootScoller = rootScoller;
            _colorInfoArrayProp = colorInfoArrayProp;
            this.TrackPropertyValue(colorInfoArrayProp, OnTrackPropertyValue);
            Rebuild();
        }

        private int _arraySize;

        private readonly List<Container> _allContainers = new List<Container>();

        private void OnTrackPropertyValue(SerializedProperty sp)
        {
            int newSize = sp.arraySize;
            if (_arraySize == newSize)
            {
                return;
            }

            Rebuild();


        }

        private TemplateContainer _lastGhost;

        private void Rebuild()
        {
            // Debug.Log($"ColorInfoArray rebuild: {_colorInfoArrayProp.arraySize}");
            Clear();
            _allContainers.Clear();

            for (int i = 0; i < _colorInfoArrayProp.arraySize; i++)
            {
                int thisIndex = i;
                SerializedProperty colorInfoProp = _colorInfoArrayProp.GetArrayElementAtIndex(thisIndex);
                SerializedProperty colorInfoLabelsProp = colorInfoProp.FindPropertyRelative(nameof(ColorPaletteArray.ColorInfo.labels));
                CleanableLabelInputTypeAhead cleanableLabelInputTypeAhead = new CleanableLabelInputTypeAhead(colorInfoLabelsProp, _rootScoller, _colorInfoArrayProp);
                Container container = Container.CreateContainer(colorInfoLabelsProp, cleanableLabelInputTypeAhead);

                SerializedProperty colorInfoColorProp = colorInfoProp.FindPropertyRelative(nameof(ColorPaletteArray.ColorInfo.color));
                container.ColorField.BindProperty(colorInfoColorProp);

                container.DeleteButton.clicked += () =>
                {
                    _colorInfoArrayProp.DeleteArrayElementAtIndex(thisIndex);
                    _colorInfoArrayProp.serializedObject.ApplyModifiedProperties();
                };

                Add(container.Root);
                _allContainers.Add(container);

                // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
                foreach (ColorPaletteLabel colorPaletteLabel in container.ColorPaletteLabels.Labels)
                {
                    LabelPointerManipulator _ = new LabelPointerManipulator(colorPaletteLabel, container.ColorPaletteLabels, _allContainers);
                }

                container.ColorPaletteLabels.BindAllColorPaletteLabels(_allContainers);
                ColorInfoManipulator __ = new ColorInfoManipulator(this, container);
            }

            _lastGhost = Container.CreateEmpty().Root;
            // _lastGhost.Clear();
            // _lastGhost.style.backgroundColor = Color.blue;
            _lastGhost.style.visibility = Visibility.Hidden;
            Add(_lastGhost);

            _arraySize = _colorInfoArrayProp.arraySize;
        }

        private Rect[] _worldBounds;
        private Vector2[] _worldPos;

        public void FrozenPositions(Container container)
        {
            _worldBounds = _allContainers.Select(each => each.Root.worldBound).ToArray();
            EnsurePlaceholder(container);
            // Add(container.Root);
            _worldPos = _allContainers
                .Select(each => each.Root.worldBound.position)
                .Append(_lastGhost.worldBound.position)
                .ToArray();
            // Remove(container.Root);
            Debug.Log($"_worldPos={string.Join(", ", _worldPos)}");
        }

        private Container _placeholder;

        // return if it's before current position; this is to fix a position bug
        public Vector2 DragOver(Vector2 worldMousePos, Container container)
        {
            (int index, Vector3 placedTargetRealPos) = GetDragIndex(worldMousePos, container);
            if (index < 0)
            {
                RemovePlaceholder();
                return placedTargetRealPos;
            }

            int currentIndex = _allContainers.FindIndex(each => each.Root == container.Root);
            if(currentIndex + 1 != index)
            {
                Insert(index, EnsurePlaceholder(container).Root);
            }
            else
            {
                RemovePlaceholder();
            }
            return placedTargetRealPos;
        }

        private (int index, Vector2 placedTargetRealPos) GetDragIndex(Vector2 worldMousePos, Container container)
        {
            int allCount = _allContainers.Count;
            int currentIndex = _allContainers.FindIndex(each => each.Root == container.Root);
            Vector2 currentIndexPos = _worldPos[currentIndex];

            if (!worldBound.Contains(worldMousePos))
            {
                return (-1, Vector2.zero);
            }

            // bool found = false;
            for (int index = 0; index < _allContainers.Count; index++)
            {
                Rect eachBound = _worldBounds[index];

                // ReSharper disable once InvertIf
                if (eachBound.Contains(worldMousePos))
                {
                    int placeIndex = index;
                    // Container eachContainer = _allContainers[index];
                    if (placeIndex == currentIndex)
                    {
                        return (-1, Vector2.zero);
                    }

                    // if (placeIndex == currentIndex + 1)
                    // {
                    //     placeIndex += 1;
                    //     // return (-1, Vector2.zero);
                    // }

                    bool isPre = placeIndex < currentIndex;
                    Vector2 pushedIndexPos = currentIndexPos;
                    if (isPre)
                    {
                        Debug.Log($"isPre, currentIndex={currentIndex}");
                        int pushedAfterIndex = currentIndex + 1;
                        if (pushedAfterIndex < _allContainers.Count)
                        {
                            pushedIndexPos = _worldPos[pushedAfterIndex];
                            Debug.Log($"isPre, pushedAfterIndex={pushedAfterIndex}, pushedIndexPos={pushedIndexPos}");
                        }
                        else  // last one, how do we get the fucking pushed position...?
                        {
                            pushedIndexPos = _worldPos[pushedAfterIndex];
                        }

                    }

                    Debug.Log($"PlaceIndex={placeIndex}");
                    return (placeIndex, pushedIndexPos - currentIndexPos);


                }
            }

            if (allCount > 0 && _worldBounds.All(wb => wb.yMin < worldMousePos.y))
            {
                return (allCount, Vector2.zero);
            }

            Vector2 pushedPos = currentIndexPos;
            if (currentIndex + 1 < _allContainers.Count)
            {
                pushedPos = _worldPos[currentIndex + 1];
            }

            return (0, pushedPos - currentIndexPos);
        }

        private Container EnsurePlaceholder(Container container)
        {
            if (_placeholder.Root == null)
            {
                _placeholder = Container.CreateEmpty();
                _placeholder.MoveIcon.RemoveFromHierarchy();
                _placeholder.DeleteButton.RemoveFromHierarchy();
                _placeholder.ColorField.SetValueWithoutNotify(container.ColorField.value);

                _placeholder.ContainerRoot.Add(new HelpBox("Move Here", HelpBoxMessageType.Info));

                VisualElement labelRoot = new VisualElement
                {
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                        flexWrap = Wrap.Wrap,
                    },
                };
                _placeholder.ContainerRoot.Add(labelRoot);

                foreach (ColorPaletteLabel colorPaletteLabel in container.ColorPaletteLabels.Labels)
                {
                    // Debug.Log($"add chip {colorPaletteLabel.value}");
                    DualButtonChip chip = new DualButtonChip(colorPaletteLabel.value);
                    chip.Button1.RemoveFromHierarchy();
                    chip.Button2.RemoveFromHierarchy();
                    labelRoot.Add(chip);
                }

                _placeholder.Root.SetEnabled(false);
            }

            return _placeholder;
        }

        public void RemovePlaceholder()
        {
            _placeholder.Root?.RemoveFromHierarchy();
            _placeholder = default;
        }

        public void DragEnd(Vector2 worldMousePos, Container container)
        {
            int index = GetDragIndex(worldMousePos, container).index;
            if (index < 0)
            {
                return;
            }

            RemovePlaceholder();
            int currentIndex = _allContainers.FindIndex(each => each.Root == container.Root);
            if (index == currentIndex)
            {
                return;
            }

            Debug.Assert(currentIndex != -1);
            Debug.Log($"Change Order {currentIndex} -> {index}");
            if (index > currentIndex)
            {
                index -= 1;
            }
            if (index >= _colorInfoArrayProp.arraySize)
            {
                index = _colorInfoArrayProp.arraySize - 1;
            }

            if (index < 0)
            {
                index = 0;
            }
            _colorInfoArrayProp.MoveArrayElement(currentIndex, index);
            _colorInfoArrayProp.serializedObject.ApplyModifiedProperties();

            Rebuild();
        }
    }
}
#endif

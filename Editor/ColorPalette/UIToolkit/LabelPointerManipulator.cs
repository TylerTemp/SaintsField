#if UNITY_2021_3_OR_NEWER
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.ColorPalette.UIToolkit
{
    public class LabelPointerManipulator: PointerManipulator
    {
        private readonly ColorPaletteLabel _targetLabel;
        private readonly ColorPaletteLabels _targetLabels;
        private readonly IReadOnlyCollection<ColorInfoArray.Container> _allColorPaletteLabels;

        private Vector2 _targetStartPosition;
        private Vector3 _pointerStartPosition;
        private bool _enabled;

        public LabelPointerManipulator(ColorPaletteLabel target, ColorPaletteLabels targetLabels, IReadOnlyCollection<ColorInfoArray.Container> allColorPaletteLabels)
        {
            _targetLabel = target;
            _targetLabels = targetLabels;
            this.target = target;
            _allColorPaletteLabels = allColorPaletteLabels;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            // Register the four callbacks on target.
            target.RegisterCallback<PointerDownEvent>(PointerDownHandler);
            target.RegisterCallback<PointerMoveEvent>(PointerMoveHandler);
            target.RegisterCallback<PointerUpEvent>(PointerUpHandler);
            target.RegisterCallback<PointerCaptureOutEvent>(PointerCaptureOutHandler);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            // Un-register the four callbacks from target.
            target.UnregisterCallback<PointerDownEvent>(PointerDownHandler);
            target.UnregisterCallback<PointerMoveEvent>(PointerMoveHandler);
            target.UnregisterCallback<PointerUpEvent>(PointerUpHandler);
            target.UnregisterCallback<PointerCaptureOutEvent>(PointerCaptureOutHandler);
        }

        // This method stores the starting position of target and the pointer,
        // makes target capture the pointer, and denotes that a drag is now in progress.
        private void PointerDownHandler(PointerDownEvent evt)
        {
            if (_targetLabel.Editing)
            {
                return;
            }

            _targetStartPosition = target.transform.position;
            _pointerStartPosition = evt.position;

            foreach (ColorInfoArray.Container container in _allColorPaletteLabels)
            {
                container.ColorPaletteLabels.FrozenPositions(container.CleanableLabelInputTypeAhead);
            }

            target.CapturePointer(evt.pointerId);
            _enabled = true;
            // _targetLabels.StartDrag(_targetLabel);
        }

        // This method checks whether a drag is in progress and whether target has captured the pointer.
        // If both are true, calculates a new position for target within the bounds of the window.
        private void PointerMoveHandler(PointerMoveEvent evt)
        {
            if (_enabled && target.HasPointerCapture(evt.pointerId))
            {
                Vector3 pointerDelta = evt.position - _pointerStartPosition;
                target.transform.position = _targetStartPosition + (Vector2)pointerDelta;

                bool captured = false;
                foreach (ColorInfoArray.Container container in _allColorPaletteLabels)
                {
                    ColorPaletteLabels colorPaletteLabels = container.ColorPaletteLabels;
                    if (captured)
                    {
                        colorPaletteLabels.RemovePlaceholder();
                    }
                    else
                    {
                        // Debug.Log($"{rootMousePosition}/{colorPaletteLabels.worldBound}");
                        (bool isOver, Vector2 offset) = colorPaletteLabels.DragOver(evt.originalMousePosition, _targetLabel);
                        if (isOver)
                        {
                            target.transform.position = (Vector2)pointerDelta - offset;
                            captured = true;
                        }
                    }
                }
            }
        }

        // This method checks whether a drag is in progress and whether target has captured the pointer.
        // If both are true, makes target release the pointer.
        private void PointerUpHandler(PointerUpEvent evt)
        {
            if (_enabled && target.HasPointerCapture(evt.pointerId))
            {
                DragEnd(evt.originalMousePosition);
                target.ReleasePointer(evt.pointerId);

                foreach (ColorInfoArray.Container container in _allColorPaletteLabels)
                {
                    ColorPaletteLabels colorPaletteLabels = container.ColorPaletteLabels;
                    colorPaletteLabels.RemovePlaceholder();
                }
                _enabled = false;
                _targetLabel.style.translate = StyleKeyword.Null;
            }
        }

        // This method checks whether a drag is in progress. If true, queries the root
        // of the visual tree to find all slots, decides which slot is the closest one
        // that overlaps target, and sets the position of target so that it rests on top
        // of that slot. Sets the position of target back to its original position
        // if there is no overlapping slot.
        private void PointerCaptureOutHandler(PointerCaptureOutEvent evt)
        {
            if (_enabled)
            {
                DragEnd(evt.originalMousePosition);
                _targetLabel.style.translate = StyleKeyword.Null;
                _enabled = false;
            }
        }

        private void DragEnd(Vector2 originalMousePosition)
        {
            bool captured = false;
            foreach (ColorInfoArray.Container container in _allColorPaletteLabels)
            {
                ColorPaletteLabels colorPaletteLabels = container.ColorPaletteLabels;
                if (captured)
                {
                    colorPaletteLabels.RemovePlaceholder();
                }
                else
                {
                    // Debug.Log($"{rootMousePosition}/{colorPaletteLabels.worldBound}");
                    (bool isOver, Vector2 _) = colorPaletteLabels.DragOver(originalMousePosition, _targetLabel);
                    if (isOver)
                    {
                        captured = true;
                        if (colorPaletteLabels.AddOrSwap(originalMousePosition, _targetLabel))
                        {
                            _targetLabels.RemoveLabel(_targetLabel);
                            // _targetLabel.style.translate = StyleKeyword.Initial;
                        }
                    }
                }
            }

            if (!captured)
            {
                _targetLabel.transform.position = _targetStartPosition;
            }

            // VisualElement slotsContainer = root.Q<VisualElement>("slots");
            // UQueryBuilder<VisualElement> allSlots =
            //     slotsContainer.Query<VisualElement>(className: "slot");
            // UQueryBuilder<VisualElement> overlappingSlots = allSlots.Where(OverlapsTarget);
            // VisualElement closestOverlappingSlot = FindClosestSlot(_allColorPaletteLabels.Where(OverlapsTarget));
            // Vector3 closestPos = Vector3.zero;
            // if (closestOverlappingSlot != null)
            // {
            //     closestPos = RootSpaceOfSlot(closestOverlappingSlot);
            //     closestPos = new Vector2(closestPos.x - 5, closestPos.y - 5);
            // }
            // target.transform.position =
            //     closestOverlappingSlot != null ?
            //     closestPos :
            //     targetStartPosition;
        }
    }
}
#endif

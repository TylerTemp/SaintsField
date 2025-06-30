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
        private readonly IReadOnlyCollection<ColorPaletteLabels> _allColorPaletteLabels;

        public LabelPointerManipulator(VisualElement root, ColorPaletteLabel target, ColorPaletteLabels targetLabels, IReadOnlyCollection<ColorPaletteLabels> allColorPaletteLabels)
        {
            _targetLabel = target;
            _targetLabels = targetLabels;
            this.target = target;
            this.root = root;
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
        private Vector2 targetStartPosition { get; set; }

        private Vector3 pointerStartPosition { get; set; }

        private bool enabled { get; set; }

        private VisualElement root { get; }

        // This method stores the starting position of target and the pointer,
        // makes target capture the pointer, and denotes that a drag is now in progress.
        private void PointerDownHandler(PointerDownEvent evt)
        {
            targetStartPosition = target.transform.position;
            pointerStartPosition = evt.position;
            target.CapturePointer(evt.pointerId);
            enabled = true;
            // _targetLabels.StartDrag(_targetLabel);
        }

        // This method checks whether a drag is in progress and whether target has captured the pointer.
        // If both are true, calculates a new position for target within the bounds of the window.
        private void PointerMoveHandler(PointerMoveEvent evt)
        {
            if (enabled && target.HasPointerCapture(evt.pointerId))
            {
                Vector3 pointerDelta = evt.position - pointerStartPosition;
                target.transform.position = targetStartPosition + (Vector2)pointerDelta;

                // Vector2 rootMousePosition = root.WorldToLocal(evt.originalMousePosition);

                bool captured = false;
                foreach (ColorPaletteLabels colorPaletteLabels in _allColorPaletteLabels)
                {
                    if (captured)
                    {
                        colorPaletteLabels.RemovePlaceholder();
                    }
                    else
                    {
                        // Debug.Log($"{rootMousePosition}/{colorPaletteLabels.worldBound}");
                        if (colorPaletteLabels.IfDragOver(evt.originalMousePosition, _targetLabel))
                        {
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
            if (enabled && target.HasPointerCapture(evt.pointerId))
            {
                DragEnd(evt.originalMousePosition);
                target.ReleasePointer(evt.pointerId);

                foreach (ColorPaletteLabels allColorPaletteLabels in _allColorPaletteLabels)
                {
                    allColorPaletteLabels.RemovePlaceholder();
                }
                enabled = false;
            }
        }

        // This method checks whether a drag is in progress. If true, queries the root
        // of the visual tree to find all slots, decides which slot is the closest one
        // that overlaps target, and sets the position of target so that it rests on top
        // of that slot. Sets the position of target back to its original position
        // if there is no overlapping slot.
        private void PointerCaptureOutHandler(PointerCaptureOutEvent evt)
        {
            if (enabled)
            {
                DragEnd(evt.originalMousePosition);
                enabled = false;
            }
        }

        private void DragEnd(Vector2 originalMousePosition)
        {
            bool captured = false;
            foreach (ColorPaletteLabels colorPaletteLabels in _allColorPaletteLabels)
            {
                if (captured)
                {
                    colorPaletteLabels.RemovePlaceholder();
                }
                else
                {
                    // Debug.Log($"{rootMousePosition}/{colorPaletteLabels.worldBound}");
                    if (colorPaletteLabels.IfDragOver(originalMousePosition, _targetLabel))
                    {
                        captured = true;
                        if (colorPaletteLabels.AddOrSwap(originalMousePosition, _targetLabel))
                        {
                            _targetLabels.RemoveLabel(_targetLabel);
                        }
                    }
                }
            }

            if (!captured)
            {
                _targetLabel.transform.position = targetStartPosition;
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

        private bool OverlapsTarget(VisualElement slot)
        {
            return target.worldBound.Overlaps(slot.worldBound);
        }

        private VisualElement FindClosestSlot(IEnumerable<VisualElement> slotsList)
        {
            // List<VisualElement> slotsList = slots.ToList();
            float bestDistanceSq = float.MaxValue;
            VisualElement closest = null;
            foreach (VisualElement slot in slotsList)
            {
                Vector3 displacement =
                    RootSpaceOfSlot(slot) - target.transform.position;
                float distanceSq = displacement.sqrMagnitude;
                if (distanceSq < bestDistanceSq)
                {
                    bestDistanceSq = distanceSq;
                    closest = slot;
                }
            }
            return closest;
        }

        private Vector3 RootSpaceOfSlot(VisualElement slot)
        {
            Vector2 slotWorldSpace = slot.parent.LocalToWorld(slot.layout.position);
            return root.WorldToLocal(slotWorldSpace);
        }
    }
}
#endif

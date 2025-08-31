#if UNITY_2021_3_OR_NEWER
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.ColorPalette.UIToolkit
{
    public class ColorInfoManipulator: PointerManipulator
    {
        private readonly ColorInfoArray _colorInfoArray;
        private readonly ColorInfoArray.Container _container;

        // private Vector2 _targetStartPosition;
        private Vector3 _pointerStartPosition;
        private Rect _targetStartBounds;
        private bool _enabled;

        public ColorInfoManipulator(ColorInfoArray colorInfoArray, ColorInfoArray.Container container)
        {
            _colorInfoArray = colorInfoArray;
            _container = container;
            target = container.Root;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            // Register the four callbacks on target.
            _container.MoveIcon.RegisterCallback<PointerDownEvent>(PointerDownHandler);
            target.RegisterCallback<PointerMoveEvent>(PointerMoveHandler);
            target.RegisterCallback<PointerUpEvent>(PointerUpHandler);
            target.RegisterCallback<PointerCaptureOutEvent>(PointerCaptureOutHandler);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            // Un-register the four callbacks from target.
            _container.MoveIcon.UnregisterCallback<PointerDownEvent>(PointerDownHandler);
            target.UnregisterCallback<PointerMoveEvent>(PointerMoveHandler);
            target.UnregisterCallback<PointerUpEvent>(PointerUpHandler);
            target.UnregisterCallback<PointerCaptureOutEvent>(PointerCaptureOutHandler);
        }

        private void PointerDownHandler(PointerDownEvent evt)
        {
            // _targetStartPosition = target.transform.position;
            _targetStartBounds = target.worldBound;
            _pointerStartPosition = evt.position;

            _colorInfoArray.FrozenPositions(_container);

            target.CapturePointer(evt.pointerId);
            _enabled = true;
            // Debug.Log($"_targetStartPosition={_targetStartPosition}");
            // _targetLabels.StartDrag(_targetLabel);
        }

        private void PointerMoveHandler(PointerMoveEvent evt)
        {
            if (_enabled && target.HasPointerCapture(evt.pointerId))
            {
                Vector3 pointerDelta = evt.position - _pointerStartPosition;
                // Debug.Log($"pointerDelta={pointerDelta}");

                Vector2 offsetPos = _colorInfoArray.DragOver(evt.originalMousePosition, _container);
                // Debug.Log($"offsetPos={offsetPos}/wp={target.worldBound.position}");
                // if (_colorInfoArray.DragOver(evt.originalMousePosition, _container))
                // {
                //     Debug.Log($"is pre! {pointerDelta.x} - {_targetStartBounds}");
                //     pointerDelta.x -= _targetStartBounds.width;
                //     // pointerDelta.y += _targetStartBounds.height;
                // }
                // else
                // {
                //     Debug.Log($"no pre! {pointerDelta.x} : {_targetStartBounds}");
                // }
                // target.transform.position = (Vector2)pointerDelta;
                target.transform.position = (Vector2)pointerDelta - offsetPos;
            }
        }

        private void PointerUpHandler(PointerUpEvent evt)
        {
            // ReSharper disable once InvertIf
            if (_enabled && target.HasPointerCapture(evt.pointerId))
            {
                _colorInfoArray.DragEnd(evt.originalMousePosition, _container);
                target.ReleasePointer(evt.pointerId);

                _colorInfoArray.RemovePlaceholder();
                _enabled = false;
                target.style.translate = StyleKeyword.Null;
            }
        }

        private void PointerCaptureOutHandler(PointerCaptureOutEvent evt)
        {
            if (_enabled)
            {
                _colorInfoArray.DragEnd(evt.originalMousePosition, _container);
                target.style.translate = StyleKeyword.Null;
                _enabled = false;
            }
        }
    }
}
#endif

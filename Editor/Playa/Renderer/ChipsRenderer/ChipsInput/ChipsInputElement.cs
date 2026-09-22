using System;
using System.Collections.Generic;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using SaintsField.Editor.Utils.WaitableUtils;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.Renderer.ChipsRenderer.ChipsInput
{
#if UNITY_6000_0_OR_NEWER
    [UxmlElement]
#endif
    public partial class ChipsInputElement: VisualElement, Util.ITicker
    {
#if !UNITY_6000_0_OR_NEWER
        public new class UxmlFactory : UxmlFactory<ChipsInputElement, UxmlTraits> { }
#endif

        public readonly UnityEvent<bool, string> OnSearchEvent = new UnityEvent<bool, string>();
        public readonly UnityEvent<Exception> OnErrorEvent = new UnityEvent<Exception>();
        // public readonly UnityEvent OnBlurEvent = new UnityEvent();
        public readonly UnityEvent OnEnterKey = new UnityEvent();

        private readonly VisualElement _inputContainer;
        private readonly TickerTextElement _actualInput;
        private bool _actualInputFocused;

        // private readonly VisualElement _overflowWrapper;

        private readonly List<DeletableChip> _deletableChips = new List<DeletableChip>();
        private readonly Dictionary<DeletableChip, IVisualElementScheduledItem> _chipAnimations =
            new Dictionary<DeletableChip, IVisualElementScheduledItem>();

        private SerializedProperty _arrayProp;
        private DeletableChip _draggedChip;
        private DeletableChip _inputAnchor;
        private Vector2 _dragPointerStart;
        private Vector2 _dragChipStart;
        private Vector2 _dragPointerPosition;
        private int _dragStartIndex = -1;
        private int _dragPointerId = -1;
        private bool _dragging;
        private int _layoutGeneration;

        private const float DragThreshold = 4f;
        private const float ReorderAnimationSeconds = 0.12f;

        public ChipsInputElement()
        {
            VisualTreeAsset chipTree = Util.LoadResource<VisualTreeAsset>("UIToolkit/ChipInput/ChipInput.uxml");
            chipTree.CloneTree(this);

            _inputContainer = this.Q<VisualElement>("inputContainer");

            _actualInput = _inputContainer.Q<TickerTextElement>("tickerTextElement");
            _actualInput.TextField.RegisterCallback<FocusEvent>(OnActualInputFocus);
            _actualInput.TextField.RegisterCallback<BlurEvent>(OnActualInputBlur);

            _actualInput.TextField.RegisterCallback<NavigationMoveEvent>(OnActualInputNavigate,
#if UNITY_6000_0_OR_NEWER
                CallbackOptions
#else
                TrickleDown
#endif
                    .TrickleDown);
            _actualInput.TextField.RegisterCallback<KeyDownEvent>(OnActualInputKeyDown,
#if UNITY_6000_0_OR_NEWER
                CallbackOptions
#else
                TrickleDown
#endif
                    .TrickleDown);

            _actualInput.TextField.RegisterCallback<KeyUpEvent>(e =>
            {
                if (e.keyCode is
                    KeyCode.Return
                    or KeyCode.KeypadEnter
                )
                {
                    OnEnterKey.Invoke();
                }
            });
            _actualInput.TextField.RegisterValueChangedCallback(OnActualInputValueChanged);

            RegisterCallback<PointerDownEvent>(OnPointerDown);

            // HelpBox helpBox = this.Q<HelpBox>("helpBox");
            // helpBox.style.display = DisplayStyle.None;
            _actualInput.OnErrorEvent.AddListener(OnErrorEvent.Invoke);

            // _overflowWrapper = this.Q<VisualElement>("overflowWrapper");
            // _overflowWrapper.style.display = DisplayStyle.None;
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button != 0 || evt.target is not VisualElement target)
            {
                return;
            }

            foreach (DeletableChip chip in _deletableChips)
            {
                if (chip.Contains(target))
                {
                    return;
                }
            }

            _actualInput.TextField.Focus();
            // _actualInput.TextField.Q("unity-base-field__input").Focus();
            // _actualInput.TextField.Q("unity-text-input").Focus();
            // _actualInput.TextField.Q(classes: "unity-text-element").Focus();
        }

        private void OnActualInputFocus(FocusEvent evt)
        {
            _actualInputFocused = true;

            _debounceSearch?.Pause();
            _debounceSearch = null;

            OnSearchEvent.Invoke(true, _actualInput.TextField.value);
        }


        private void OnActualInputBlur(BlurEvent evt)
        {
            _actualInputFocused = false;

            _debounceSearch?.Pause();
            _debounceSearch = null;
            // OnBlurEvent.Invoke();
        }

        public readonly UnityEvent<NavigationMoveEvent> BubbleNavigationMoveEvent =
            new UnityEvent<NavigationMoveEvent>();

        private void OnActualInputNavigate(NavigationMoveEvent evt)
        {
            if (!_actualInputFocused)
            {
                return;
            }

            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (evt.direction)
            {
                case NavigationMoveEvent.Direction.Up:
                case NavigationMoveEvent.Direction.Down:
                    BubbleNavigationMoveEvent.Invoke(evt);
#if UNITY_6000_0_OR_NEWER
                    _actualInput.TextField.focusController?.IgnoreEvent(evt);
#else
                    evt.PreventDefault();
#endif
                    evt.StopPropagation();
                    return;
            }

            TextField actualInput = (TextField)evt.currentTarget;
            if ((evt.modifiers & EventModifiers.Shift) != 0
                || (evt.modifiers & EventModifiers.Control) != 0
                || actualInput.cursorIndex != actualInput.selectIndex)
            {
                return;
            }

            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (evt.direction)
            {
                case NavigationMoveEvent.Direction.Left:
                {
                    if (actualInput.cursorIndex > 0)
                    {
                        return;
                    }

                    ActualInputMove(true);
                }
                    break;
                case NavigationMoveEvent.Direction.Right:
                {
                    if (actualInput.cursorIndex < actualInput.value.Length)
                    {
                        return;
                    }
                    ActualInputMove(false);
                }
                    break;
            }
        }

        public void BindProp(SerializedProperty arrayProp)
        {
            _arrayProp = arrayProp;
        }

        private void OnActualInputKeyDown(KeyDownEvent evt)
        {
            if (!_actualInputFocused || evt.keyCode != KeyCode.Backspace)
            {
                return;
            }

            TextField actualInput = (TextField)evt.currentTarget;
            if (actualInput.cursorIndex != 0 || actualInput.selectIndex != 0)
            {
                return;
            }

            VisualElement inputParent = _actualInput.parent;
            int inputIndex = inputParent?.IndexOf(_actualInput) ?? -1;
            if (inputIndex <= 0 || inputParent.ElementAt(inputIndex - 1) is not DeletableChip leftChip)
            {
                return;
            }

            evt.StopPropagation();
            leftChip.OnDeleteButtonClicked();
            ActualInputMove(true);
        }

        public void ActualInputMove(bool left)
        {
            VisualElement inputParent = _actualInput.parent;
            if (inputParent == null)
            {
                return;
            }

            int inputIndex = inputParent.IndexOf(_actualInput);
            int siblingIndex = inputIndex + (left ? -1 : 1);
            if (siblingIndex < 0 || siblingIndex >= inputParent.childCount)
            {
                return;
            }

            VisualElement sibling = inputParent.ElementAt(siblingIndex);
            if (left)
            {
                _actualInput.PlaceBehind(sibling);
            }
            else
            {
                _actualInput.PlaceInFront(sibling);
            }

        }

        public int GetInputIndex()
        {
            int index = 0;
            foreach (VisualElement child in _inputContainer.Children())
            {
                if (child == _actualInput)
                {
                    break;
                }

                if (child is DeletableChip)
                {
                    index++;
                }
            }

            return index;
        }

        private string _preSearchContent = "";
        // private double _preSearchTime = 0;
        private const int DebounceTimeMs = 600;
        private IVisualElementScheduledItem _debounceSearch;
        private void OnActualInputValueChanged(ChangeEvent<string> evt)
        {
            _debounceSearch?.Pause();
            _debounceSearch = null;

            string content = evt.newValue.Trim();
            // bool hasContent = string.IsNullOrEmpty(content);
            // UIToolkitUtils.SetDisplayStyle(_overflowWrapper, hasContent? DisplayStyle.None: DisplayStyle.Flex);
            if (_preSearchContent == content)
            {
                // Debug.Log($"Skip search {content}");
                return;
            }

            bool fireNow = string.IsNullOrEmpty(_preSearchContent) || string.IsNullOrEmpty(content);
            _preSearchContent = content;
            if (fireNow)
            {
                // Debug.Log($"Immediate search {_preSearchContent}");
                OnSearchEvent.Invoke(false, _preSearchContent);
            }
            else
            {
                // Debug.Log($"Delay search {_preSearchContent} in {DebounceTimeMs} ms");
                _debounceSearch = schedule
                    .Execute(() =>
                    {
                        // Debug.Log($"Fire search {_preSearchContent}");
                        OnSearchEvent.Invoke(false, _preSearchContent);
                    })
                    .StartingIn(DebounceTimeMs);
            }
        }

        public IEnumerable<DeletableChip> GetOrCreateChip(int size)
        {
            int leftCount = size;
            Queue<DeletableChip> copiedChips = new Queue<DeletableChip>(_deletableChips);
            while (leftCount > 0 && copiedChips.Count > 0)
            {
                 DeletableChip cached = copiedChips.Dequeue();
                 yield return cached;
                 leftCount--;
            }

            if (copiedChips.Count > 0)
            {
                foreach (DeletableChip chip in copiedChips)
                {
                    _deletableChips.Remove(chip);
                    chip.RemoveFromHierarchy();
                }
            }
            else if (leftCount > 0)
            {
                // Debug.Log($"need create {leftCount} chips");
                for (int index = 0; index < leftCount; index++)
                {
                    // Debug.Log($"create {index} chip");
                    DeletableChip newCreated = new DeletableChip();
                    newCreated.RegisterCallback<PointerDownEvent>(OnChipPointerDown);
                    newCreated.RegisterCallback<PointerMoveEvent>(OnChipPointerMove);
                    newCreated.RegisterCallback<PointerUpEvent>(OnChipPointerUp);
                    newCreated.RegisterCallback<PointerCaptureOutEvent>(OnChipPointerCaptureOut);
                    _deletableChips.Add(newCreated);
                    _inputContainer.Add(newCreated);
                    yield return newCreated;
                }
            }
        }

        private void OnChipPointerDown(PointerDownEvent evt)
        {
            if (evt.button != 0 || evt.currentTarget is not DeletableChip chip ||
                evt.target is VisualElement target && chip.Button2.Contains(target))
            {
                return;
            }

            _draggedChip = chip;
            _dragPointerStart = evt.position;
            _dragPointerPosition = evt.position;
            _dragChipStart = chip.worldBound.position;
            _dragStartIndex = _deletableChips.IndexOf(chip);
            _dragPointerId = evt.pointerId;
            _dragging = false;

            int inputIndex = _inputContainer.IndexOf(_actualInput);
            _inputAnchor = inputIndex > 0
                ? _inputContainer.ElementAt(inputIndex - 1) as DeletableChip
                : null;

            chip.CapturePointer(evt.pointerId);
        }

        private void OnChipPointerMove(PointerMoveEvent evt)
        {
            if (_draggedChip == null || evt.currentTarget != _draggedChip ||
                !_draggedChip.HasPointerCapture(evt.pointerId))
            {
                return;
            }

            _dragPointerPosition = evt.position;
            Vector2 pointerDelta = _dragPointerPosition - _dragPointerStart;
            if (!_dragging)
            {
                if (pointerDelta.sqrMagnitude < DragThreshold * DragThreshold)
                {
                    return;
                }

                _dragging = true;
                _draggedChip.style.opacity = 0.75f;
                _draggedChip.BringToFront();
                RestoreInputPosition();
            }

            int newIndex = GetDragIndex(_inputContainer.WorldToLocal(_dragPointerPosition));
            int currentIndex = _deletableChips.IndexOf(_draggedChip);
            if (newIndex != currentIndex)
            {
                AnimateReorder(currentIndex, newIndex);
            }

            UpdateDraggedPosition();
            evt.StopPropagation();
        }

        private int GetDragIndex(Vector2 localPointerPosition)
        {
            List<DeletableChip> otherChips = _deletableChips.FindAll(each => each != _draggedChip);
            int insertionIndex = otherChips.Count;
            for (int index = 0; index < otherChips.Count; index++)
            {
                Rect layout = otherChips[index].layout;
                float rowTolerance = Mathf.Max(2f, layout.height * 0.35f);
                if (localPointerPosition.y < layout.center.y - rowTolerance ||
                    Mathf.Abs(localPointerPosition.y - layout.center.y) <= rowTolerance &&
                    localPointerPosition.x < layout.center.x)
                {
                    insertionIndex = index;
                    break;
                }
            }

            return Mathf.Clamp(insertionIndex, 0, otherChips.Count);
        }

        private void AnimateReorder(int oldIndex, int newIndex)
        {
            Dictionary<DeletableChip, Vector2> oldPositions = new Dictionary<DeletableChip, Vector2>();
            foreach (DeletableChip chip in _deletableChips)
            {
                if (chip == _draggedChip)
                {
                    continue;
                }

                oldPositions[chip] = chip.worldBound.position;
                StopAnimation(chip);
                SetTranslation(chip, Vector2.zero);
            }

            _deletableChips.RemoveAt(oldIndex);
            _deletableChips.Insert(newIndex, _draggedChip);
            ApplyVisualOrder();

            int generation = ++_layoutGeneration;
            schedule.Execute(() =>
            {
                if (generation != _layoutGeneration || _draggedChip == null)
                {
                    return;
                }

                foreach (KeyValuePair<DeletableChip, Vector2> pair in oldPositions)
                {
                    Vector2 offset = pair.Value - pair.Key.worldBound.position;
                    if (offset.sqrMagnitude > 0.01f)
                    {
                        AnimateToLayout(pair.Key, offset);
                    }
                }

                UpdateDraggedPosition();
            });
        }

        private void ApplyVisualOrder()
        {
            foreach (DeletableChip chip in _deletableChips)
            {
                chip.BringToFront();
            }

            RestoreInputPosition();
        }

        private void RestoreInputPosition()
        {
            if (_inputAnchor == null)
            {
                _inputContainer.Insert(0, _actualInput);
                return;
            }

            int anchorIndex = _inputContainer.IndexOf(_inputAnchor);
            _inputContainer.Insert(anchorIndex + 1, _actualInput);
        }

        private void UpdateDraggedPosition()
        {
            if (!_dragging || _draggedChip == null)
            {
                return;
            }

            Vector2 desiredPosition = _dragChipStart + (_dragPointerPosition - _dragPointerStart);
            Vector2 layoutPosition = _draggedChip.worldBound.position - GetTranslation(_draggedChip);
            SetTranslation(_draggedChip, desiredPosition - layoutPosition);
        }

        private void AnimateToLayout(DeletableChip chip, Vector2 startOffset)
        {
            StopAnimation(chip);
            float startTime = Time.realtimeSinceStartup;
            SetTranslation(chip, startOffset);
            IVisualElementScheduledItem animation = null;
            animation = chip.schedule.Execute(() =>
            {
                float progress = Mathf.Clamp01((Time.realtimeSinceStartup - startTime) / ReorderAnimationSeconds);
                float eased = 1f - Mathf.Pow(1f - progress, 3f);
                SetTranslation(chip, Vector2.LerpUnclamped(startOffset, Vector2.zero, eased));
                if (progress >= 1f)
                {
                    animation.Pause();
                    SetTranslation(chip, Vector2.zero);
                    _chipAnimations.Remove(chip);
                }
            }).Every(16);
            _chipAnimations[chip] = animation;
        }

        private static Vector2 GetTranslation(VisualElement element)
        {
#if UNITY_6000_3_OR_NEWER
            return element.resolvedStyle.translate;
#else
            return element.transform.position;
#endif
        }

        private static void SetTranslation(VisualElement element, Vector2 translation)
        {
#if UNITY_6000_3_OR_NEWER
            element.style.translate = translation;
#else
            element.transform.position = translation;
#endif
        }

        private void StopAnimation(DeletableChip chip)
        {
            if (_chipAnimations.TryGetValue(chip, out IVisualElementScheduledItem animation))
            {
                animation.Pause();
                _chipAnimations.Remove(chip);
            }
        }

        private void OnChipPointerUp(PointerUpEvent evt)
        {
            if (_draggedChip == null || evt.currentTarget != _draggedChip ||
                !_draggedChip.HasPointerCapture(evt.pointerId))
            {
                return;
            }

            FinishDrag();
            ((DeletableChip)evt.currentTarget).ReleasePointer(evt.pointerId);
            evt.StopPropagation();
        }

        private void OnChipPointerCaptureOut(PointerCaptureOutEvent evt)
        {
            if (_draggedChip != null && evt.currentTarget == _draggedChip && evt.pointerId == _dragPointerId)
            {
                FinishDrag();
            }
        }

        private void FinishDrag()
        {
            DeletableChip draggedChip = _draggedChip;
            int oldIndex = _dragStartIndex;
            int newIndex = draggedChip == null ? -1 : _deletableChips.IndexOf(draggedChip);
            bool moved = _dragging && oldIndex >= 0 && newIndex >= 0 && oldIndex != newIndex;

            _draggedChip = null;
            _dragStartIndex = -1;
            _dragPointerId = -1;
            _dragging = false;
            _inputAnchor = null;
            _layoutGeneration++;

            if (draggedChip != null)
            {
                draggedChip.style.opacity = StyleKeyword.Null;
                SetTranslation(draggedChip, Vector2.zero);
            }

            if (!moved || !SerializedUtils.IsOk(_arrayProp))
            {
                return;
            }

            _arrayProp.MoveArrayElement(oldIndex, newIndex);
            _arrayProp.serializedObject.ApplyModifiedProperties();
            for (int index = 0; index < _deletableChips.Count; index++)
            {
                _deletableChips[index].Rebind(index);
            }
        }

        public void SetInputToLastChild()
        {
            _inputContainer.Insert(_inputContainer.childCount - 1, _actualInput);
        }

        private Waiter _waiter;
        private IVisualElementScheduledItem _scheduler;

        public void StartTrack(Waiter waiter, Action<object> succeedCallback)
        {
            _actualInput.StartTrack(waiter, succeedCallback);
        }

        // tree will steal the focus; we just focus again...
        // shitty hack
        public void InputFocus()
        {
            _actualInput.TextField.Focus();
        }
    }
}

using System.Collections.Generic;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEngine.Events;
using UnityEngine.UIElements;
// using UnityEngine;

namespace SaintsField.Editor.UIToolkitElements.ValueButtons
{
    public abstract class AbsValueButtonsCalcElement : VisualElement
    {
        protected AbsValueButtonsCalcElement()
        {
            style.flexDirection = FlexDirection.Row;
            style.overflow = Overflow.Hidden;
            style.opacity = 0;
            pickingMode = PickingMode.Ignore;

            // RegisterCallback<GeometryChangedEvent>(OnGeometryChangedEvent);

            UIToolkitUtils.OnAttachToPanelOnce(this, _ => schedule.Execute(StartToCalc).Every(150));

        }

        private readonly UnityEvent<IReadOnlyList<(IReadOnlyList<RichTextDrawer.RichTextChunk> displayChunks, float width)>> _readyEvent = new UnityEvent<IReadOnlyList<(IReadOnlyList<RichTextDrawer.RichTextChunk>, float)>>();
        private RichTextDrawer _richTextDrawer;
        private readonly List<AbsValueButton> _buttons = new List<AbsValueButton>();

        private bool _ready;

        public void AddReadyListener(UnityAction<IReadOnlyList<(IReadOnlyList<RichTextDrawer.RichTextChunk> displayChunks, float width)>> listener)
        {
            if (_ready)
            {
                listener.Invoke(_chunksWidth);
            }
            _readyEvent.AddListener(listener);
        }

        protected abstract AbsValueButton CreateValueButton(IReadOnlyList<RichTextDrawer.RichTextChunk> chunks);

        public void SetButtonLabels(IEnumerable<IReadOnlyList<RichTextDrawer.RichTextChunk>> buttonChunks)
        {
            _richTextDrawer ??= new RichTextDrawer();

            Clear();
            _buttons.Clear();
            foreach (IReadOnlyList<RichTextDrawer.RichTextChunk> chunks in buttonChunks)
            {
                // ValueButton btn = new ValueButton(chunks);
                AbsValueButton btn = CreateValueButton(chunks);
                // anyButton = btn;
                Add(btn);
                _buttons.Add(btn);
            }

            _ready = false;
            // schedule.Execute(() => ).StartingIn(500);
            // Debug.Log($"SetButtonLabels {_buttons.Count}, call OnGeometryChangedEvent for calc");
            // StartToCalc();
            // if (anyButton != null)
            // {
            //     anyButton.RegisterCallback<AttachToPanelEvent>(_ => OnGeometryChangedEvent(null));
            // }
        }

        // private void OnGeometryChangedEvent(GeometryChangedEvent _)
        // {
        //     StartToCalc();
        // }
        private readonly List<(IReadOnlyList<RichTextDrawer.RichTextChunk>, float)> _chunksWidth =
            new List<(IReadOnlyList<RichTextDrawer.RichTextChunk>, float)>();

        private static bool IsDisplayed(VisualElement ve)
        {
            for (VisualElement e = ve; e != null; e = e.parent)
            {
                if (e.resolvedStyle.display == DisplayStyle.None)
                    return false;
            }

            return true;
        }

        private void StartToCalc()
        {
            if (_ready)
            {
                // Debug.Log("Ready, skip");
                return;
            }

            if (_buttons.Count == 0)
            {
                // Debug.Log("No buttons, skip");
                return;
            }

            if (!IsDisplayed(this))
            {
                // Debug.Log("Not shown, skip");
                return;
            }

            // chunksWidth =
            //     new List<(IReadOnlyList<RichTextDrawer.RichTextChunk>, float)>();
            _chunksWidth.Clear();
            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (AbsValueButton button in _buttons)
            {
                float width = button.resolvedStyle.width;
                if (double.IsNaN(width) || width <= 0)
                {
                    // Debug.Log($"Button not ready {width}, skip");
                    // schedule.Execute(StartToCalc);
                    return;
                }

                // Debug.Log($"get button width {width}; curWidth={resolvedStyle.width}");

                _chunksWidth.Add((button.Chunks, width));
            }

            // Debug.Log($"all width: {string.Join(", ", _chunksWidth)}");
            _ready = true;

            _readyEvent.Invoke(_chunksWidth);
        }
    }
}

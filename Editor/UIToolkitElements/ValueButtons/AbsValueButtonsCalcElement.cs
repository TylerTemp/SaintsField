using System.Collections.Generic;
using SaintsField.Editor.Core;
using UnityEngine.Events;
using UnityEngine.UIElements;
// using UnityEngine;

namespace SaintsField.Editor.UIToolkitElements.ValueButtons
{
    public abstract class AbsValueButtonsCalcElement<T>: VisualElement where T: AbsValueButton
    {
        public AbsValueButtonsCalcElement()
        {
            style.flexDirection = FlexDirection.Row;
            style.overflow = Overflow.Hidden;
            style.opacity = 0;
            pickingMode = PickingMode.Ignore;

            RegisterCallback<GeometryChangedEvent>(OnGeometryChangedEvent);
        }

        public readonly UnityEvent<IReadOnlyList<(IReadOnlyList<RichTextDrawer.RichTextChunk> displayChunks, float width)>> ReadyEvent = new UnityEvent<IReadOnlyList<(IReadOnlyList<RichTextDrawer.RichTextChunk>, float)>>();
        private RichTextDrawer _richTextDrawer;
        private readonly List<AbsValueButton> _buttons = new List<AbsValueButton>();

        private bool _ready;

        public abstract AbsValueButton CreateValueButton(IReadOnlyList<RichTextDrawer.RichTextChunk> chunks);

        public void SetButtonLabels(IEnumerable<IReadOnlyList<RichTextDrawer.RichTextChunk>> buttonChunks)
        {
            _ready = false;
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

            // schedule.Execute(() => ).StartingIn(500);
            // Debug.Log($"SetButtonLabels {_buttons.Count}, call OnGeometryChangedEvent for calc");
            StartToCalc();
            // if (anyButton != null)
            // {
            //     anyButton.RegisterCallback<AttachToPanelEvent>(_ => OnGeometryChangedEvent(null));
            // }
        }

        private void OnGeometryChangedEvent(GeometryChangedEvent _)
        {
            StartToCalc();
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

            List<(IReadOnlyList<RichTextDrawer.RichTextChunk>, float)> chunksWidth =
                new List<(IReadOnlyList<RichTextDrawer.RichTextChunk>, float)>();
            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (AbsValueButton button in _buttons)
            {
                float width = button.resolvedStyle.width;
                if (double.IsNaN(width) || width <= 0)
                {
                    // Debug.Log($"Button not ready {width}, try later");
                    schedule.Execute(StartToCalc);
                    return;
                }

                // Debug.Log($"get button width {width}; curWidth={resolvedStyle.width}");

                chunksWidth.Add((button.Chunks, width));
            }

            // Debug.Log($"all width: {string.Join(", ", Widths)}");
            _ready = true;

            ReadyEvent.Invoke(chunksWidth);
        }
    }
}

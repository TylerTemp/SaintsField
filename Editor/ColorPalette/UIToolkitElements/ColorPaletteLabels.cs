#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.ColorPalette.UIToolkitElements
{
#if UNITY_6000_0_OR_NEWER
    [UxmlElement]
#endif
    // ReSharper disable once ClassNeverInstantiated.Global
    // ReSharper disable once PartialTypeWithSinglePart
    public partial class ColorPaletteLabels: BindableElement
    {
        // ReSharper disable once UnusedMember.Global
        public ColorPaletteLabels() : this(null)
        {

        }

        public ColorPaletteLabels(SerializedProperty arrayProp)
        {
            style.flexDirection = FlexDirection.Row;
            style.flexWrap = Wrap.Wrap;
            style.alignItems = Align.FlexStart;

            if (arrayProp != null)
            {
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
        private readonly List<ColorPaletteLabel> _labels = new List<ColorPaletteLabel>();

        private void OnTrackPropertyValue(SerializedProperty sp)
        {
            if (_detached)
            {
                return;
            }

            int newSize = sp.arraySize;
            if (_labels.Count == newSize)
            {
                return;
            }

            if (newSize > _arraySize)
            {
                // Increase array size
                for (int i = _arraySize; i < newSize; i++)
                {
                    int thisIndex = i;
                    ColorPaletteLabel label = new ColorPaletteLabel();
                    SerializedProperty arrayElementProp = sp.GetArrayElementAtIndex(i);
                    label.BindProperty(arrayElementProp);
                    // Debug.Log($"add {i}: {arrayElementProp.propertyPath}");
                    label.OnDeleteClicked.AddListener(() =>
                    {
                        // Debug.Log($"Remove label {i}");
                        sp.DeleteArrayElementAtIndex(thisIndex);
                        sp.serializedObject.ApplyModifiedProperties();
                    });
                    _labels.Add(label);
                    // Add(label);
                    Insert(i, label);
                }
            }
            else if (newSize < _arraySize)
            {
                // Decrease array size
                for (int i = _labels.Count - 1; i >= newSize; i--)
                {
                    // Debug.Log($"remove {i}: {_labels[i].bindingPath}");
                    Remove(_labels[i]);
                    _labels.RemoveAt(i);
                }

                for (int i = 0; i < _labels.Count; i++)
                {
                    // Debug.Log($"rebind {i}: {_labels[i].bindingPath}");
                    _labels[i].BindProperty(sp.GetArrayElementAtIndex(i));
                }
            }

            _arraySize = newSize;
        }

        private bool _detached;

        private void OnDetachFromPanelEvent(DetachFromPanelEvent evt)
        {
            _detached = true;
        }
    }
}
#endif

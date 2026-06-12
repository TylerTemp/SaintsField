using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Utils.IMGUIPlainDrawer
{
    public static class IMGUIInteger
    {
        private static readonly int DragHash = "SaintsFieldIntegerDrag".GetHashCode();
        private static int _activeId;
        private static float _dragStartMouseX;
        private static long _dragStartValue;

        public static float GetHeight(bool inHorizontalLayout) => IMGUIShared.GetSingleLineHeight(inHorizontalLayout);

        public static int DrawIntField(Rect position, GUIContent label, int value, bool inHorizontalLayout, bool labelGrayColor)
        {
            return (int)Mathf.Clamp(DrawLongField(position, label, value, inHorizontalLayout, labelGrayColor), int.MinValue, int.MaxValue);
        }

        public static long DrawLongField(Rect position, GUIContent label, long value, bool inHorizontalLayout, bool labelGrayColor)
        {
            if (!inHorizontalLayout)
            {
                return IMGUIShared.WithLabelColor(labelGrayColor, () => EditorGUI.LongField(position, label, value));
            }

            (Rect labelRect, Rect fieldRect) = IMGUIShared.GetStackedRects(position);
            int id = GUIUtility.GetControlID(DragHash, FocusType.Passive, labelRect);

            long dragValue = HandleLabelDrag(labelRect, id, value);
            if (dragValue != value)
            {
                value = dragValue;
                GUI.changed = true;
            }

            IMGUIShared.WithLabelColor(labelGrayColor, () => EditorGUI.HandlePrefixLabel(position, labelRect, label, id));
            return EditorGUI.LongField(fieldRect, value);
        }

        private static long HandleLabelDrag(Rect labelRect, int id, long value)
        {
            Event evt = Event.current;
            EditorGUIUtility.AddCursorRect(labelRect, MouseCursor.SlideArrow);

            switch (evt.GetTypeForControl(id))
            {
                case EventType.MouseDown:
                    if (evt.button == 0 && labelRect.Contains(evt.mousePosition))
                    {
                        GUIUtility.hotControl = id;
                        _activeId = id;
                        _dragStartMouseX = evt.mousePosition.x;
                        _dragStartValue = value;
                        evt.Use();
                    }
                    break;
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == id && _activeId == id)
                    {
                        float step = GetDragStep(_dragStartValue, evt);
                        long draggedValue = _dragStartValue + Mathf.RoundToInt((evt.mousePosition.x - _dragStartMouseX) / 3f * step);
                        if (draggedValue != value)
                        {
                            GUI.changed = true;
                            value = draggedValue;
                        }
                        evt.Use();
                    }
                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == id && _activeId == id)
                    {
                        GUIUtility.hotControl = 0;
                        _activeId = 0;
                        evt.Use();
                    }
                    break;
                case EventType.Repaint:
                    if (GUIUtility.hotControl == id && _activeId == id)
                    {
                        HandleUtility.Repaint();
                    }
                    break;
            }

            return value;
        }

        private static float GetDragStep(long value, Event evt)
        {
            float step = 1f;
            long absValue = System.Math.Abs(value);
            if (absValue >= 10)
            {
                step = Mathf.Pow(10f, Mathf.Floor(Mathf.Log10(absValue)) - 1f);
            }

            if (evt.shift)
            {
                step *= 10f;
            }
            if (evt.alt)
            {
                step *= 0.1f;
            }

            return Mathf.Max(1f, step);
        }
    }
}

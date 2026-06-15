using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Utils.IMGUIPlainDrawer
{
    public static class IMGUIFloat
    {
        private static readonly int FloatDragHash = "SaintsFieldFloatDrag".GetHashCode();
        private static readonly int DoubleDragHash = "SaintsFieldDoubleDrag".GetHashCode();

        private static int _activeFloatId;
        private static float _floatDragStartMouseX;
        private static float _floatDragStartValue;

        private static int _activeDoubleId;
        private static float _doubleDragStartMouseX;
        private static double _doubleDragStartValue;

        public static float GetHeight(bool inHorizontalLayout) => IMGUIShared.GetSingleLineHeight(inHorizontalLayout);

        public static float DrawFloatField(Rect position, GUIContent label, float value, bool inHorizontalLayout, bool labelGrayColor)
        {
            if (!inHorizontalLayout)
            {
                Rect contentRect = IMGUIShared.GetContentRect(position);
                return IMGUIShared.WithLabelColor(labelGrayColor, () => EditorGUI.FloatField(contentRect, label, value));
            }

            Rect contentRectForStack = IMGUIShared.GetContentRect(position);
            (Rect labelRect, Rect fieldRect) = IMGUIShared.GetStackedRects(position);
            int id = GUIUtility.GetControlID(FloatDragHash, FocusType.Passive, labelRect);

            float dragValue = HandleFloatLabelDrag(labelRect, id, value);
            if (!Mathf.Approximately(dragValue, value))
            {
                value = dragValue;
                GUI.changed = true;
            }

            IMGUIShared.WithLabelColor(labelGrayColor, () => EditorGUI.HandlePrefixLabel(contentRectForStack, labelRect, label, id));
            return EditorGUI.FloatField(fieldRect, value);
        }

        public static double DrawDoubleField(Rect position, GUIContent label, double value, bool inHorizontalLayout, bool labelGrayColor)
        {
            if (!inHorizontalLayout)
            {
                Rect contentRect = IMGUIShared.GetContentRect(position);
                return IMGUIShared.WithLabelColor(labelGrayColor, () => EditorGUI.DoubleField(contentRect, label, value));
            }

            Rect contentRectForStack = IMGUIShared.GetContentRect(position);
            (Rect labelRect, Rect fieldRect) = IMGUIShared.GetStackedRects(position);
            int id = GUIUtility.GetControlID(DoubleDragHash, FocusType.Passive, labelRect);

            double dragValue = HandleDoubleLabelDrag(labelRect, id, value);
            if (System.Math.Abs(dragValue - value) > double.Epsilon)
            {
                value = dragValue;
                GUI.changed = true;
            }

            IMGUIShared.WithLabelColor(labelGrayColor, () => EditorGUI.HandlePrefixLabel(contentRectForStack, labelRect, label, id));
            return EditorGUI.DoubleField(fieldRect, value);
        }

        private static float HandleFloatLabelDrag(Rect labelRect, int id, float value)
        {
            Event evt = Event.current;
            EditorGUIUtility.AddCursorRect(labelRect, MouseCursor.SlideArrow);

            switch (evt.GetTypeForControl(id))
            {
                case EventType.MouseDown:
                    if (evt.button == 0 && labelRect.Contains(evt.mousePosition))
                    {
                        GUIUtility.hotControl = id;
                        _activeFloatId = id;
                        _floatDragStartMouseX = evt.mousePosition.x;
                        _floatDragStartValue = value;
                        evt.Use();
                    }
                    break;
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == id && _activeFloatId == id)
                    {
                        float sensitivity = GetFloatDragSensitivity(_floatDragStartValue, evt);
                        float draggedValue = _floatDragStartValue + (evt.mousePosition.x - _floatDragStartMouseX) * sensitivity;
                        if (!Mathf.Approximately(draggedValue, value))
                        {
                            GUI.changed = true;
                            value = draggedValue;
                        }
                        evt.Use();
                    }
                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == id && _activeFloatId == id)
                    {
                        GUIUtility.hotControl = 0;
                        _activeFloatId = 0;
                        evt.Use();
                    }
                    break;
                case EventType.Repaint:
                    if (GUIUtility.hotControl == id && _activeFloatId == id)
                    {
                        HandleUtility.Repaint();
                    }
                    break;
            }

            return value;
        }

        private static double HandleDoubleLabelDrag(Rect labelRect, int id, double value)
        {
            Event evt = Event.current;
            EditorGUIUtility.AddCursorRect(labelRect, MouseCursor.SlideArrow);

            switch (evt.GetTypeForControl(id))
            {
                case EventType.MouseDown:
                    if (evt.button == 0 && labelRect.Contains(evt.mousePosition))
                    {
                        GUIUtility.hotControl = id;
                        _activeDoubleId = id;
                        _doubleDragStartMouseX = evt.mousePosition.x;
                        _doubleDragStartValue = value;
                        evt.Use();
                    }
                    break;
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == id && _activeDoubleId == id)
                    {
                        double sensitivity = GetDoubleDragSensitivity(_doubleDragStartValue, evt);
                        double draggedValue = _doubleDragStartValue + (evt.mousePosition.x - _doubleDragStartMouseX) * sensitivity;
                        if (System.Math.Abs(draggedValue - value) > double.Epsilon)
                        {
                            GUI.changed = true;
                            value = draggedValue;
                        }
                        evt.Use();
                    }
                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == id && _activeDoubleId == id)
                    {
                        GUIUtility.hotControl = 0;
                        _activeDoubleId = 0;
                        evt.Use();
                    }
                    break;
                case EventType.Repaint:
                    if (GUIUtility.hotControl == id && _activeDoubleId == id)
                    {
                        HandleUtility.Repaint();
                    }
                    break;
            }

            return value;
        }

        private static float GetFloatDragSensitivity(float value, Event evt)
        {
            float sensitivity = Mathf.Max(0.03f, Mathf.Abs(value) * 0.01f);
            if (evt.shift)
            {
                sensitivity *= 10f;
            }
            if (evt.alt)
            {
                sensitivity *= 0.1f;
            }

            return sensitivity;
        }

        private static double GetDoubleDragSensitivity(double value, Event evt)
        {
            double sensitivity = System.Math.Max(0.03d, System.Math.Abs(value) * 0.01d);
            if (evt.shift)
            {
                sensitivity *= 10d;
            }
            if (evt.alt)
            {
                sensitivity *= 0.1d;
            }

            return sensitivity;
        }
    }
}

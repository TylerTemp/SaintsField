using System;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(ProgressBarAttribute))]
    public class ProgressBarAttributeDrawer: SaintsPropertyDrawer
    {

        #region IMGUI

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute,
            bool hasLabelWidth)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute,
            object parent)
        {
            int controlId = GUIUtility.GetControlID(FocusType.Passive, position);
            Rect fieldRect = EditorGUI.PrefixLabel(position, controlId, label);
            // EditorGUI.DrawRect(position, Color.yellow);
            EditorGUI.DrawRect(fieldRect, EColor.Blue.GetColor());

            float curValue = property.floatValue;
            float percent = curValue / 100f;
            Rect fillRect = RectUtils.SplitWidthRect(fieldRect, fieldRect.width * percent).leftRect;

            EditorGUI.DrawRect(fillRect, EColor.Green.GetColor());

            Event e = Event.current;
            // Debug.Log($"{e.isMouse}, {e.mousePosition}");
            // ReSharper disable once InvertIf
            // Debug.Log($"{e.type} {e.isMouse}, {e.button}, {e.mousePosition}");

            if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && fieldRect.Contains(e.mousePosition))
            {
                float newValue = (e.mousePosition.x - fieldRect.x) / fieldRect.width * 100f;
                property.floatValue = newValue;
                SetValueChanged(property);
            }

            EditorGUI.DropShadowLabel(fieldRect, $"{curValue:0.00}%");

            // if(e.type == EventType.MouseDrag && )

            // if (position.Contains(e.mousePosition))
            // {
            //     Debug.Log($"cap: {e.type} {e.isMouse}, {e.button}, {e.mousePosition}");
            // }
        }

        #endregion

        // private struct

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            VisualElement container, Label fakeLabel, object parent)
        {
            ProgressBar progressBar = new ProgressBar
            {
                title = property.displayName,
                lowValue = 0f,
                highValue = 100f,
                value = property.floatValue,

                style =
                {
                    // color = EColor.Green.GetColor(),
                    // backgroundColor = EColor.Aqua.GetColor(),
                },
            };

            Type type = typeof(AbstractProgressBar);
            FieldInfo backgroundFieldInfo = type.GetField("m_Background", BindingFlags.NonPublic | BindingFlags.Instance);
            if (backgroundFieldInfo != null)
            {
                VisualElement background = (VisualElement) backgroundFieldInfo.GetValue(progressBar);
                Debug.Log(background);
                // background.style.backgroundColor = EColor.Aqua.GetColor();
                // backgroundFieldInfo.SetValue(element, curveRangeAttribute.Color.GetColor());
            }

            progressBar.CapturePointer(0);
            progressBar.RegisterCallback<PointerDownEvent>(evt =>
            {
                Debug.Log($"down: {evt.pointerId}");
            });
            progressBar.RegisterCallback<PointerUpEvent>(evt =>
            {
                Debug.Log($"up: {evt.pointerId}");
            });
            progressBar.RegisterCallback<PointerLeaveEvent>(evt =>
            {
                Debug.Log($"leave: {evt.pointerId}");
            });
            progressBar.RegisterCallback<PointerMoveEvent>(evt =>
            {
                Debug.Log(evt.localPosition);
                Debug.Log(evt.pointerId);
                Debug.Log(progressBar.HasPointerCapture(0));

                float curWidth = progressBar.resolvedStyle.width;
                if(float.IsNaN(curWidth))
                {
                    return;
                }

                float curValue = evt.localPosition.x / curWidth * 100f;
                progressBar.value = curValue;
            });

            // Debug.Log(progressBar.resolvedStyle.width);
            //
            progressBar.RegisterValueChangedCallback(evt =>
            {
                property.floatValue = evt.newValue;
                property.serializedObject.ApplyModifiedProperties();
                progressBar.title = evt.newValue.ToString();
                Debug.Log(evt.newValue);
            });
            //
            // progressBar.schedule.Execute(() =>
            // {
            //     progressBar.value += 2f;
            // }).Every(75).Until(() => progressBar.value >= 100f);
            //
            return progressBar;

            // return new Slider();
        }
    }
}

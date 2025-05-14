using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.HeaderGUI
{
    public class DrawHeaderGUI
    {
#if SAINTSFIELD_DEBUG_HEADER_GUI
        [InitializeOnLoadMethod]
#endif
        private static void Init()
        {
            EditorApplication.delayCall += InitLoad;
        }

        private static void InitLoad()
        {
            const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Static;

            FieldInfo fieldInfo = typeof(EditorGUIUtility).GetField("s_EditorHeaderItemsMethods", flags);
            if (fieldInfo == null)
            {
                return;  // it's changed internally
            }

            IList value = (IList)fieldInfo.GetValue(null);
            // Debug.Log($"value={value}");
            if (value == null)
            {
                EditorApplication.delayCall += InitLoad;
                return;
            }

            Type delegateType = value.GetType().GetGenericArguments()[0];

            MethodInfo methodInfo = typeof(DrawHeaderGUI).GetMethod(nameof(DrawMethod), flags);

            Debug.Log($"inject {methodInfo} into {value}");

            // ReSharper disable once AssignNullToNotNullAttribute
            value.Add(Delegate.CreateDelegate(delegateType, methodInfo));
        }

        private static bool DrawMethod(Rect rectangle, UnityEngine.Object[] targets)
        {
            if (rectangle.x < 0)
            {
                return false;
            }
            string title = ObjectNames.GetInspectorTitle(targets[0], targets.Length > 1);
            float titleWidth = EditorStyles.largeLabel.CalcSize(new GUIContent(title)).x;
            // Debug.Log($"{title}: {targets[0]}");

            const float prefixWidth = 60;
            const float gap = 10;

            rectangle.x = rectangle.xMax;
            rectangle.width = 0;

            float xMax = rectangle.xMax;
            float xMin = prefixWidth + gap + titleWidth;

            rectangle.x = xMin;
            rectangle.width = xMax - xMin;

            // rectangle.x -= 40;
            // rectangle.width += 40;

            EditorGUI.DrawRect(rectangle, Color.blue * new Color(1, 1, 1, 0.3f));

            Rect titleRect = new Rect(rectangle)
            {
                x = prefixWidth,
                width = titleWidth,
            };
            EditorGUI.DrawRect(titleRect, Color.yellow * new Color(1, 1, 1, 0.2f));
            // EditorGUI.LabelField(rectangle, title);

            return false;
        }
    }
}

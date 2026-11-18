using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace SaintsField.Editor.Core
{
    internal static class UnityPropertyHandler
    {
        private const BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private static readonly Type HandlerType = typeof(EditorGUI).Assembly.GetType("UnityEditor.PropertyHandler");
        private static readonly MethodInfo GetHandler = typeof(EditorGUI).Assembly
            .GetType("UnityEditor.ScriptAttributeUtility")?.GetMethod("GetHandler",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                null, new[] { typeof(SerializedProperty) }, null);
        private static readonly FieldInfo PropertyDrawers = HandlerType?.GetField("m_PropertyDrawers", InstanceFlags);
        private static readonly FieldInfo NestingLevel = HandlerType?.GetField("m_NestingLevel", InstanceFlags);
        private static readonly MethodInfo ApplyNestingContext = HandlerType?.GetMethod("ApplyNestingContext",
            InstanceFlags, null, new[] { typeof(int) }, null);

        /// <summary>
        /// Establishes the context Unity normally enters before invoking a drawer directly.
        /// Nested EditorGUI calls then continue after this drawer, including height queries from OnGUI.
        /// The returned scope restores Unity's previous nesting level, including when drawing throws.
        /// </summary>
        public static IDisposable EnterDrawer(SerializedProperty property, PropertyDrawer drawer)
        {
            if (drawer == null || GetHandler == null || PropertyDrawers == null || NestingLevel == null ||
                ApplyNestingContext == null)
            {
                return null;
            }

            object handler = GetHandler.Invoke(null, new object[] { property });
            if (!(PropertyDrawers.GetValue(handler) is List<PropertyDrawer> drawers))
            {
                return null;
            }

            int drawerIndex = drawers.IndexOf(drawer);
            if (drawerIndex < 0)
            {
                // SaintsField creates its own instances. Match the drawer and its attribute, not only its type.
                // Do not guess when multiple identical attributes make the position ambiguous.
                for (int index = 0; index < drawers.Count; index++)
                {
                    PropertyDrawer candidate = drawers[index];
                    if (candidate.GetType() != drawer.GetType() || !Equals(candidate.attribute, drawer.attribute))
                    {
                        continue;
                    }

                    if (drawerIndex >= 0)
                    {
                        return null;
                    }

                    drawerIndex = index;
                }
            }

            if (drawerIndex < 0)
            {
                return null;
            }

            // Never rewind a handler that has already advanced beyond this drawer.
            int nestingLevel = Math.Max((int)NestingLevel.GetValue(handler), drawerIndex + 1);
            return (IDisposable)ApplyNestingContext.Invoke(handler, new object[] { nestingLevel });
        }
    }
}

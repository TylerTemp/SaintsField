using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace SaintsField.Editor.Utils.IMGUIPlainDrawer
{
    public static class IMGUIList
    {
        private class ListContext
        {
            public SerializedProperty Property;
            public IReadOnlyList<Attribute> AllAttributes;
            public Type RawType;
            public FieldInfo FieldInfo;
            public GUIContent Label;
            public bool InHorizontalLayout;
            public bool LabelGrayColor;
            public ReorderableList ReorderableList;
            public RichTextDrawer RichTextDrawer;
        }

        private static readonly Dictionary<IMGUIDrawerCache.DrawerId, ListContext> CachedContexts =
            new Dictionary<IMGUIDrawerCache.DrawerId, ListContext>();

        private static void InvalidateReorderableList(SerializedProperty property, ListContext context)
        {
            IMGUIDrawerCache.DrawerId key = new IMGUIDrawerCache.DrawerId(property, 0);
            if (context != null)
            {
                context.ReorderableList = null;
            }
            IMGUIDrawerCache.CachedReorderableList.Remove(key);
        }

        private static ListContext EnsureContext(SerializedProperty property, IReadOnlyList<Attribute> allAttributes,
            Type rawType, GUIContent label, FieldInfo fieldInfo, bool inHorizontalLayout, bool labelGrayColor)
        {
            IMGUIDrawerCache.DrawerId key = new IMGUIDrawerCache.DrawerId(property, 0);
            if (!CachedContexts.TryGetValue(key, out ListContext context))
            {
                context = new ListContext();
                CachedContexts[key] = context;
            }

            context.Property = property;
            context.AllAttributes = allAttributes;
            context.RawType = rawType;
            context.FieldInfo = fieldInfo;
            context.Label = label;
            context.InHorizontalLayout = inHorizontalLayout;
            context.LabelGrayColor = labelGrayColor;

            if (!IMGUIDrawerCache.CachedReorderableList.TryGetValue(key, out ReorderableList reorderableList) ||
                reorderableList == null)
            {
                reorderableList = CreateReorderableList(context);
                IMGUIDrawerCache.CachedReorderableList[key] = reorderableList;
            }

            context.ReorderableList = reorderableList;
            context.ReorderableList.headerHeight = 0f;
            return context;
        }

        private static ReorderableList CreateReorderableList(ListContext context)
        {
            ReorderableList reorderableList =
                new ReorderableList(context.Property.serializedObject, context.Property, true, false, true, true);
            reorderableList.elementHeightCallback += index => GetElementHeight(context, index);
            reorderableList.drawElementCallback += (rect, index, _, _) => DrawElement(rect, index, context);
            return reorderableList;
        }

        private static void DrawHeader(Rect rect, ListContext context,
            IEnumerable<RichTextDrawer.RichTextChunk> richTextChunks)
        {
            const float sizeWidth = 36f;
            const float sizeGap = 4f;

            Rect sizeRect = new Rect(rect)
            {
                x = rect.xMax - sizeWidth,
                width = sizeWidth,
                height = EditorGUIUtility.singleLineHeight,
            };

            Rect foldoutRect = new Rect(rect)
            {
                width = Mathf.Max(0f, rect.width - sizeWidth - sizeGap),
                height = EditorGUIUtility.singleLineHeight,
            };

            context.Property.isExpanded =
                EditorGUI.Foldout(
                    foldoutRect,
                    context.Property.isExpanded,
                    context.Label,
                    true);

            if (richTextChunks != null)
            {
                RichTextDrawer textDrawer = context.RichTextDrawer ?? new RichTextDrawer();
                textDrawer.DrawChunks(foldoutRect, richTextChunks);
            }

            SerializedProperty sizeProp = context.Property.FindPropertyRelative("Array.size");
            if (sizeProp != null)
            {
                EditorGUI.BeginChangeCheck();
                int newSize = EditorGUI.DelayedIntField(sizeRect, GUIContent.none, sizeProp.intValue);
                if (EditorGUI.EndChangeCheck())
                {
                    sizeProp.intValue = Math.Max(0, newSize);
                    context.Property.serializedObject.ApplyModifiedProperties();
                }
                return;
            }

            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                int newSize = EditorGUI.DelayedIntField(sizeRect, GUIContent.none, context.Property.arraySize);
                if (changed.changed)
                {
                    context.Property.arraySize = Math.Max(0, newSize);
                    context.Property.serializedObject.ApplyModifiedProperties();
                }
            }
        }

        private static float GetElementHeight(ListContext context, int index)
        {
            if (!context.Property.isExpanded || index >= context.Property.arraySize)
            {
                return 0f;
            }

            SerializedProperty itemProp = context.Property.GetArrayElementAtIndex(index);
            Type itemType = ReflectUtils.GetElementType(context.RawType);
            string itemLabel = $"Element {index}";
            PropertyDrawer itemDrawer =
                IMGUIRawDraw.GetAndCacheDrawer(itemProp, context.AllAttributes, context.FieldInfo, itemLabel);
            return IMGUIRawDraw.GetPropertyHeight(itemDrawer, new GUIContent(itemLabel), itemProp, context.AllAttributes,
                itemType, context.FieldInfo, context.InHorizontalLayout);
        }

        private static void DrawElement(Rect rect, int index, ListContext context)
        {
            if (!context.Property.isExpanded || index >= context.Property.arraySize)
            {
                return;
            }

            SerializedProperty itemProp = context.Property.GetArrayElementAtIndex(index);
            Type itemType = ReflectUtils.GetElementType(context.RawType);
            string itemLabel = $"Element {index}";
            Attribute[] childProp = context.AllAttributes
                .Where(each => each is SerializeReference || each is PropertyAttribute)
                .ToArray();
            PropertyDrawer itemDrawer =
                IMGUIRawDraw.GetAndCacheDrawer(itemProp, childProp, context.FieldInfo, itemLabel);
            IMGUIRawDraw.OnGUI(itemDrawer, rect, itemProp, childProp, itemType, new GUIContent(itemLabel), null,
                context.FieldInfo, context.InHorizontalLayout, context.LabelGrayColor);
        }

        public static float GetHeight(SerializedProperty property, IReadOnlyList<Attribute> allAttributes, Type rawType,
            GUIContent label, FieldInfo fieldInfo, bool inHorizontalLayout, bool labelGrayColor)
        {
            ListContext context = EnsureContext(property, allAttributes, rawType, label, fieldInfo, inHorizontalLayout,
                labelGrayColor);
            if (!context.Property.isExpanded)
            {
                return EditorGUIUtility.singleLineHeight + 2f;
            }

            try
            {
                return EditorGUIUtility.singleLineHeight + context.ReorderableList.GetHeight() + 2f;
            }
            catch (ObjectDisposedException)
            {
                InvalidateReorderableList(property, context);
            }
            catch (NullReferenceException)
            {
                InvalidateReorderableList(property, context);
            }

            context = EnsureContext(property, allAttributes, rawType, label, fieldInfo, inHorizontalLayout,
                labelGrayColor);

            try
            {
                return EditorGUIUtility.singleLineHeight + context.ReorderableList.GetHeight() + 2f;
            }
            catch (ObjectDisposedException)
            {
                InvalidateReorderableList(property, context);
            }
            catch (NullReferenceException)
            {
                InvalidateReorderableList(property, context);
            }

            return EditorGUIUtility.singleLineHeight + 2f;
        }

        public static void DrawField(Rect position, SerializedProperty property, IReadOnlyList<Attribute> allAttributes,
            Type rawType, GUIContent label, IEnumerable<RichTextDrawer.RichTextChunk> richTextChunks,
            FieldInfo fieldInfo, bool inHorizontalLayout, bool labelGrayColor)
        {
            ListContext context = EnsureContext(property, allAttributes, rawType, label, fieldInfo, inHorizontalLayout,
                labelGrayColor);

            Rect usePosition = new Rect(position)
            {
                y = position.y + 1f,
                height = Mathf.Max(0f, position.height - 2f),
            };

            try
            {
                Rect headerRect = new Rect(usePosition)
                {
                    height = EditorGUIUtility.singleLineHeight,
                };
                DrawHeader(headerRect, context, richTextChunks);

                if (!context.Property.isExpanded)
                {
                    return;
                }

                Rect listRect = new Rect(usePosition)
                {
                    y = headerRect.yMax,
                    height = Mathf.Max(0f, usePosition.height - headerRect.height),
                };
                context.ReorderableList.DoList(listRect);
            }
            catch (ObjectDisposedException)
            {
                InvalidateReorderableList(property, context);
            }
            catch (NullReferenceException)
            {
                InvalidateReorderableList(property, context);
            }
        }
    }
}

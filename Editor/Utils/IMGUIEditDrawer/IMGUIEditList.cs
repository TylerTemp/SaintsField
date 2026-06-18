using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Utils;
using SaintsField.Utils;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace SaintsField.Editor.Utils.IMGUIEditDrawer
{
    internal static class IMGUIEditList
    {
        private const float VerticalPadding = 1f;

        private sealed class ListContext
        {
            public string Key;
            public string Label;
            public Type ValueType;
            public Type ElementType;
            public object RawValue;
            public List<object> Items;
            public Action<object> BeforeSet;
            public Action<object> SetterOrNull;
            public bool LabelGrayColor;
            public bool InHorizontalLayout;
            public IReadOnlyList<Attribute> AllAttributes;
            public IReadOnlyList<object> Targets;
            public IRichTextTagProvider RichTextTagProvider;
            public ReorderableList ReorderableList;
        }

        private static readonly Dictionary<string, ListContext> ListContexts = new Dictionary<string, ListContext>();

        public static (bool ok, float height) GetPropertyHeight(
            string label, Type valueType, object value, Action<object> beforeSet, Action<object> setterOrNull,
            bool labelGrayColor, bool inHorizontalLayout,
            IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets,
            IRichTextTagProvider richTextTagProvider, string foldoutViewKey)
        {
            (bool isEnumerable, Type elementType) = GetEnumerableElementType(valueType, value);
            if (!isEnumerable)
            {
                return (false, 0f);
            }

            ListContext context = EnsureListContext(label, valueType, value, beforeSet, setterOrNull,
                labelGrayColor, inHorizontalLayout, allAttributes, targets, richTextTagProvider, foldoutViewKey,
                elementType);
            return (true, GetListHeight(context));
        }

        public static bool TryOnGUI(
            Rect position,
            string label, Type valueType, object value, Action<object> beforeSet, Action<object> setterOrNull,
            bool labelGrayColor, bool inHorizontalLayout,
            IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets,
            IRichTextTagProvider richTextTagProvider, string foldoutViewKey)
        {
            (bool isEnumerable, Type elementType) = GetEnumerableElementType(valueType, value);
            if (!isEnumerable)
            {
                return false;
            }

            ListContext context = EnsureListContext(label, valueType, value, beforeSet, setterOrNull,
                labelGrayColor, inHorizontalLayout, allAttributes, targets, richTextTagProvider, foldoutViewKey,
                elementType);
            DrawList(position, context);
            return true;
        }

        private static (bool ok, Type elementType) GetEnumerableElementType(Type valueType, object value)
        {
            Type type = value?.GetType() ?? valueType;
            if (type == null || type == typeof(string) || typeof(UnityEngine.Object).IsAssignableFrom(type))
            {
                return (false, null);
            }

            if (type.IsArray)
            {
                return (true, type.GetElementType());
            }

            if (!typeof(IEnumerable).IsAssignableFrom(type))
            {
                return (false, null);
            }

            Type elementType = ReflectUtils.GetElementType(type);
            if (elementType == type)
            {
                elementType = typeof(object);
            }

            return (true, elementType);
        }

        private static bool IsExpanded(string key) =>
            IMGUIEdit.ViewKey.ContainsKey(key) && IMGUIEdit.ViewKey[key];

        private static void SetExpanded(string key, bool expanded) => IMGUIEdit.ViewKey[key] = expanded;

        private static ListContext EnsureListContext(string label, Type valueType, object value,
            Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout,
            IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets,
            IRichTextTagProvider richTextTagProvider, string foldoutViewKey, Type elementType)
        {
            string key = $"{foldoutViewKey}.list";
            if (!ListContexts.ContainsKey(key))
            {
                ListContexts[key] = new ListContext
                {
                    Key = key,
                };
            }

            ListContext context = ListContexts[key];
            context.Label = label;
            context.ValueType = valueType;
            context.ElementType = elementType;
            context.RawValue = value;
            context.Items = ToObjectList(value);
            context.BeforeSet = beforeSet;
            context.SetterOrNull = setterOrNull;
            context.LabelGrayColor = labelGrayColor;
            context.InHorizontalLayout = inHorizontalLayout;
            context.AllAttributes = allAttributes;
            context.Targets = targets;
            context.RichTextTagProvider = richTextTagProvider;

            if (context.ReorderableList == null)
            {
                context.ReorderableList = CreateReorderableList(context);
            }
            else
            {
                context.ReorderableList.list = context.Items;
            }

            context.ReorderableList.draggable = setterOrNull != null;
            context.ReorderableList.displayAdd = setterOrNull != null;
            context.ReorderableList.displayRemove = setterOrNull != null;
            context.ReorderableList.headerHeight = 0f;
            return context;
        }

        private static ReorderableList CreateReorderableList(ListContext context)
        {
            ReorderableList reorderableList = new ReorderableList(context.Items, context.ElementType, true, false,
                true, true);
            reorderableList.elementHeightCallback = index => GetListElementHeight(context, index);
            reorderableList.drawElementCallback = (rect, index, _, _) => DrawListElement(rect, index, context);
            reorderableList.onAddCallback = _ =>
            {
                context.Items.Add(CreateDefaultValue(context.ElementType));
                ApplyListValue(context);
            };
            reorderableList.onRemoveCallback = list =>
            {
                if (list.index >= 0 && list.index < context.Items.Count)
                {
                    context.Items.RemoveAt(list.index);
                    ApplyListValue(context);
                }
            };
            reorderableList.onReorderCallback = _ => ApplyListValue(context);
            return reorderableList;
        }

        private static List<object> ToObjectList(object value)
        {
            if (RuntimeUtil.IsNull(value))
            {
                return new List<object>();
            }

            return ((IEnumerable)value).Cast<object>().ToList();
        }

        private static float GetListHeight(ListContext context)
        {
            if (!IsExpanded(context.Key))
            {
                return EditorGUIUtility.singleLineHeight + VerticalPadding * 2;
            }

            return EditorGUIUtility.singleLineHeight + context.ReorderableList.GetHeight() + VerticalPadding * 2;
        }

        private static void DrawList(Rect position, ListContext context)
        {
            Rect contentRect = new Rect(position)
            {
                y = position.y + VerticalPadding,
                height = Mathf.Max(0f, position.height - VerticalPadding * 2),
            };

            Rect headerRect = new Rect(contentRect)
            {
                height = EditorGUIUtility.singleLineHeight,
            };
            DrawListHeader(headerRect, context);

            if (!IsExpanded(context.Key))
            {
                return;
            }

            Rect listRect = new Rect(contentRect)
            {
                y = headerRect.yMax,
                height = Mathf.Max(0f, contentRect.yMax - headerRect.yMax),
            };
            context.ReorderableList.DoList(listRect);
        }

        private static void DrawListHeader(Rect rect, ListContext context)
        {
            const float sizeWidth = 48f;
            Rect sizeRect = new Rect(rect)
            {
                x = rect.xMax - sizeWidth,
                width = sizeWidth,
            };
            Rect foldoutRect = new Rect(rect)
            {
                width = Mathf.Max(0f, rect.width - sizeWidth - 4f),
            };

            bool expanded = EditorGUI.Foldout(foldoutRect, IsExpanded(context.Key),
                new GUIContent($"{context.Label}"), true);
            SetExpanded(context.Key, expanded);

            using (new EditorGUI.DisabledScope(context.SetterOrNull == null))
            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                int newSize = EditorGUI.DelayedIntField(sizeRect, context.Items.Count);
                if (changed.changed)
                {
                    ResizeList(context, Math.Max(0, newSize));
                    ApplyListValue(context);
                }
            }
        }

        private static void ResizeList(ListContext context, int newSize)
        {
            while (context.Items.Count < newSize)
            {
                context.Items.Add(CreateDefaultValue(context.ElementType));
            }

            while (context.Items.Count > newSize)
            {
                context.Items.RemoveAt(context.Items.Count - 1);
            }
        }

        private static float GetListElementHeight(ListContext context, int index)
        {
            if (index < 0 || index >= context.Items.Count)
            {
                return 0f;
            }

            return IMGUIEdit.GetPropertyHeight(
                $"Element {index}",
                context.ElementType,
                context.Items[index],
                null,
                context.SetterOrNull == null ? null : newValue =>
                {
                    context.Items[index] = newValue;
                    ApplyListValue(context);
                },
                context.LabelGrayColor,
                context.InHorizontalLayout,
                context.AllAttributes,
                context.Targets,
                context.RichTextTagProvider,
                $"{context.Key}.[{index}]") + 2f;
        }

        private static void DrawListElement(Rect rect, int index, ListContext context)
        {
            if (index < 0 || index >= context.Items.Count)
            {
                return;
            }

            Rect useRect = new Rect(rect)
            {
                y = rect.y + 1f,
                height = Mathf.Max(0f, rect.height - 2f),
            };
            IMGUIEdit.OnGUI(
                useRect,
                $"Element {index}",
                context.ElementType,
                context.Items[index],
                null,
                context.SetterOrNull == null ? null : newValue =>
                {
                    context.Items[index] = newValue;
                    ApplyListValue(context);
                },
                context.LabelGrayColor,
                context.InHorizontalLayout,
                context.AllAttributes,
                context.Targets,
                context.RichTextTagProvider,
                $"{context.Key}.[{index}]");
        }

        private static void ApplyListValue(ListContext context)
        {
            if (context.SetterOrNull == null)
            {
                return;
            }

            object newValue = MakeCollectionValue(context.ValueType, context.ElementType, context.RawValue,
                context.Items);
            context.BeforeSet?.Invoke(context.RawValue);
            context.SetterOrNull(newValue);
            context.RawValue = newValue;
        }

        private static object MakeCollectionValue(Type valueType, Type elementType, object rawValue,
            List<object> items)
        {
            if (valueType?.IsArray == true)
            {
                Array array = Array.CreateInstance(elementType, items.Count);
                for (int index = 0; index < items.Count; index++)
                {
                    array.SetValue(items[index], index);
                }

                return array;
            }

            if (rawValue is IList existingList && !existingList.IsReadOnly && !existingList.IsFixedSize)
            {
                existingList.Clear();
                foreach (object item in items)
                {
                    existingList.Add(item);
                }

                return existingList;
            }

            Type listType = typeof(List<>).MakeGenericType(elementType);
            IList list = (IList)Activator.CreateInstance(listType);
            foreach (object item in items)
            {
                list.Add(item);
            }

            if (valueType == null || valueType.IsAssignableFrom(listType) || valueType.IsInterface)
            {
                return list;
            }

            if (typeof(IList).IsAssignableFrom(valueType) && valueType.GetConstructor(Type.EmptyTypes) != null)
            {
                IList concrete = (IList)Activator.CreateInstance(valueType);
                foreach (object item in items)
                {
                    concrete.Add(item);
                }

                return concrete;
            }

            return list;
        }

        private static object CreateDefaultValue(Type type)
        {
            if (type == typeof(string))
            {
                return "";
            }

            if (type == typeof(Guid))
            {
                return Guid.NewGuid();
            }

            if (type?.IsEnum == true)
            {
                Array values = Enum.GetValues(type);
                return values.Length == 0 ? Activator.CreateInstance(type) : values.GetValue(0);
            }

            return type?.IsValueType == true ? Activator.CreateInstance(type) : null;
        }
    }
}

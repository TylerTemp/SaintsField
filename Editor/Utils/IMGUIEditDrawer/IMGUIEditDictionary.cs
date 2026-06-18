using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Utils.IMGUIEditDrawer
{
    internal static class IMGUIEditDictionary
    {
        private const float VerticalPadding = 1f;
        private const float SizeWidth = 50f;
        private const float ButtonWidth = 18f;
        private const float ControlGap = 4f;
        private const float TablePadding = 2f;
        private const float CellPadding = 2f;
        private const float AddPanelPadding = 4f;

        private sealed class DictionaryContext
        {
            public string Key;
            public string Label;
            public Type ValueType;
            public Type KeyType;
            public Type ValueValueType;
            public object RawValue;
            public Action<object> BeforeSet;
            public Action<object> SetterOrNull;
            public bool LabelGrayColor;
            public bool InHorizontalLayout;
            public IReadOnlyList<Attribute> AllAttributes;
            public IReadOnlyList<object> Targets;
            public IRichTextTagProvider RichTextTagProvider;
            public bool IsReadOnly;
            public PropertyInfo KeysProperty;
            public PropertyInfo IndexerProperty;
            public MethodInfo RemoveMethod;
            public MethodInfo ContainsKeyMethod;
            public HashSet<int> SelectedIndexes = new HashSet<int>();
            public bool AddPanelOpen;
            public object AddKey;
            public object AddValue;
            public string AddError = "";
            public string Error = "";
        }

        private static readonly Dictionary<string, DictionaryContext> DictionaryContexts =
            new Dictionary<string, DictionaryContext>();

        public static (bool ok, float height) GetPropertyHeight(
            string label, Type valueType, object value, Action<object> beforeSet, Action<object> setterOrNull,
            bool labelGrayColor, bool inHorizontalLayout,
            IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets,
            IRichTextTagProvider richTextTagProvider, string foldoutViewKey)
        {
            (bool isDictionary, Type keyType, Type dictValueType, bool isReadOnly) =
                GetDictionaryTypes(valueType, value);
            if (!isDictionary)
            {
                return (false, 0f);
            }

            DictionaryContext context = EnsureDictionaryContext(label, valueType, value, beforeSet, setterOrNull,
                labelGrayColor, inHorizontalLayout, allAttributes, targets, richTextTagProvider, foldoutViewKey,
                keyType, dictValueType, isReadOnly);
            return (true, GetDictionaryHeight(context));
        }

        public static bool TryOnGUI(
            Rect position,
            string label, Type valueType, object value, Action<object> beforeSet, Action<object> setterOrNull,
            bool labelGrayColor, bool inHorizontalLayout,
            IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets,
            IRichTextTagProvider richTextTagProvider, string foldoutViewKey)
        {
            (bool isDictionary, Type keyType, Type dictValueType, bool isReadOnly) =
                GetDictionaryTypes(valueType, value);
            if (!isDictionary)
            {
                return false;
            }

            DictionaryContext context = EnsureDictionaryContext(label, valueType, value, beforeSet, setterOrNull,
                labelGrayColor, inHorizontalLayout, allAttributes, targets, richTextTagProvider, foldoutViewKey,
                keyType, dictValueType, isReadOnly);
            DrawDictionary(position, context);
            return true;
        }

        private static (bool ok, Type keyType, Type valueType, bool isReadOnly) GetDictionaryTypes(Type valueType,
            object value)
        {
            Type type = value?.GetType() ?? valueType;
            if (type == null)
            {
                return (false, null, null, false);
            }

            Type keyType = null;
            Type valueValueType = null;
            bool isReadOnly = false;
            IEnumerable<Type> candidates = type.GetInterfaces().Where(each => each.IsGenericType);
            if (type.IsGenericType)
            {
                candidates = candidates.Prepend(type);
            }

            foreach (Type candidate in candidates)
            {
                Type definition = candidate.GetGenericTypeDefinition();
                if (definition == typeof(IDictionary<,>))
                {
                    Type[] args = candidate.GetGenericArguments();
                    keyType = args[0];
                    valueValueType = args[1];
                    return (true, keyType, valueValueType, false);
                }

                if (definition == typeof(IReadOnlyDictionary<,>))
                {
                    Type[] args = candidate.GetGenericArguments();
                    keyType = args[0];
                    valueValueType = args[1];
                    isReadOnly = true;
                }
            }

            return (keyType != null, keyType, valueValueType, isReadOnly);
        }

        private static bool IsExpanded(string key) =>
            IMGUIEdit.ViewKey.ContainsKey(key) && IMGUIEdit.ViewKey[key];

        private static void SetExpanded(string key, bool expanded) => IMGUIEdit.ViewKey[key] = expanded;

        private static DictionaryContext EnsureDictionaryContext(string label, Type valueType, object value,
            Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout,
            IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets,
            IRichTextTagProvider richTextTagProvider, string foldoutViewKey, Type keyType, Type valueValueType,
            bool isReadOnly)
        {
            string key = $"{foldoutViewKey}.dictionary";
            if (!DictionaryContexts.ContainsKey(key))
            {
                DictionaryContexts[key] = new DictionaryContext
                {
                    Key = key,
                };
            }

            DictionaryContext context = DictionaryContexts[key];
            context.Label = label;
            context.ValueType = valueType;
            context.KeyType = keyType;
            context.ValueValueType = valueValueType;
            context.RawValue = value;
            context.BeforeSet = beforeSet;
            context.SetterOrNull = setterOrNull;
            context.LabelGrayColor = labelGrayColor;
            context.InHorizontalLayout = inHorizontalLayout;
            context.AllAttributes = allAttributes;
            context.Targets = targets;
            context.RichTextTagProvider = richTextTagProvider;
            (string accessError, PropertyInfo keysProperty, PropertyInfo indexerProperty, MethodInfo removeMethod,
                    MethodInfo containsKeyMethod) =
                GetDictionaryAccess(valueType, value, keyType);
            context.KeysProperty = keysProperty;
            context.IndexerProperty = indexerProperty;
            context.RemoveMethod = removeMethod;
            context.ContainsKeyMethod = containsKeyMethod;
            context.IsReadOnly = isReadOnly || GetDictionaryReadOnly(value);
            context.Error = RuntimeUtil.IsNull(value) ? "" : accessError;
            if (!CanEditDictionary(context))
            {
                context.AddPanelOpen = false;
            }

            if (context.AddPanelOpen)
            {
                context.AddError = GetAddKeyError(context);
            }

            ClampDictionarySelection(context, GetDictionaryKeys(context).Count());
            return context;
        }

        private static (string error, PropertyInfo keysProperty, PropertyInfo indexerProperty,
            MethodInfo removeMethod, MethodInfo containsKeyMethod) GetDictionaryAccess(Type valueType, object value,
            Type keyType)
        {
            Type type = value?.GetType() ?? valueType;
            if (type == null)
            {
                return ("Dictionary value edit requires a dictionary type.", null, null, null, null);
            }

            PropertyInfo keysProperty = type.GetProperty("Keys");
            PropertyInfo indexerProperty = keyType == null ? null : type.GetProperty("Item", new[] { keyType });
            MethodInfo removeMethod = keyType == null ? null : type.GetMethod("Remove", new[] { keyType });
            MethodInfo containsKeyMethod = keyType == null ? null : type.GetMethod("ContainsKey", new[] { keyType });

            if (typeof(IDictionary).IsAssignableFrom(type))
            {
                return ("", keysProperty, indexerProperty, removeMethod, containsKeyMethod);
            }

            if (keysProperty == null)
            {
                return ("Dictionary value edit requires a Keys property.", null, indexerProperty, removeMethod,
                    containsKeyMethod);
            }

            if (indexerProperty == null)
            {
                return ("Dictionary value edit requires an indexer property.", keysProperty, null, removeMethod,
                    containsKeyMethod);
            }

            return ("", keysProperty, indexerProperty, removeMethod, containsKeyMethod);
        }

        private static bool GetDictionaryReadOnly(object rawValue)
        {
            if (RuntimeUtil.IsNull(rawValue))
            {
                return false;
            }

            if (rawValue is IDictionary dictionary && dictionary.IsReadOnly)
            {
                return true;
            }

            PropertyInfo isReadOnlyProperty = rawValue.GetType().GetProperty("IsReadOnly");
            if (isReadOnlyProperty?.PropertyType == typeof(bool))
            {
                return (bool)isReadOnlyProperty.GetValue(rawValue);
            }

            return false;
        }

        private static float GetDictionaryHeight(DictionaryContext context)
        {
            float singleLineHeight = EditorGUIUtility.singleLineHeight;
            float height = singleLineHeight + VerticalPadding * 2;
            if (context.Error != "")
            {
                return height + ImGuiHelpBox.GetHeight(context.Error, EditorGUIUtility.currentViewWidth,
                    MessageType.Error);
            }

            if (!IsExpanded(context.Key) || RuntimeUtil.IsNull(context.RawValue))
            {
                return height;
            }

            height += TablePadding * 2 + singleLineHeight + singleLineHeight;
            foreach (object key in GetDictionaryKeys(context))
            {
                object dictValue = GetDictionaryValue(context, key);
                height += GetDictionaryRowHeight(context, key, dictValue) + 1f;
            }

            if (context.AddPanelOpen && CanEditDictionary(context))
            {
                height += GetDictionaryAddPanelHeight(context, EditorGUIUtility.currentViewWidth) + 2f;
            }

            return height;
        }

        private static void DrawDictionary(Rect position, DictionaryContext context)
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
            DrawDictionaryHeader(headerRect, context);

            Rect leftRect = new Rect(contentRect)
            {
                y = headerRect.yMax,
                height = Mathf.Max(0f, contentRect.yMax - headerRect.yMax),
            };

            if (context.Error != "")
            {
                ImGuiHelpBox.Draw(leftRect, context.Error, MessageType.Error);
                return;
            }

            if (!IsExpanded(context.Key) || RuntimeUtil.IsNull(context.RawValue))
            {
                return;
            }

            GUI.Box(leftRect, GUIContent.none, EditorStyles.helpBox);
            Rect workRect = ShrinkRect(leftRect, TablePadding);

            (Rect tableHeaderRect, Rect afterHeaderRect) =
                RectUtils.SplitHeightRect(workRect, EditorGUIUtility.singleLineHeight);
            DrawDictionaryTableHeader(tableHeaderRect);
            leftRect = afterHeaderRect;

            object[] keys = GetDictionaryKeys(context).ToArray();
            for (int index = 0; index < keys.Length; index++)
            {
                object key = keys[index];
                object dictValue = GetDictionaryValue(context, key);
                float rowHeight = GetDictionaryRowHeight(context, key, dictValue);

                (Rect rowRect, Rect afterRowRect) = RectUtils.SplitHeightRect(leftRect, rowHeight);
                DrawDictionaryRow(rowRect, context, index, key, dictValue);
                leftRect = afterRowRect;
            }

            (Rect footerRect, Rect afterFooterRect) =
                RectUtils.SplitHeightRect(leftRect, EditorGUIUtility.singleLineHeight);
            DrawDictionaryFooter(footerRect, context);
            leftRect = afterFooterRect;

            if (context.AddPanelOpen && CanEditDictionary(context))
            {
                Rect addPanelRect = new Rect(leftRect)
                {
                    y = leftRect.y + 2f,
                    height = Mathf.Max(0f, GetDictionaryAddPanelHeight(context, leftRect.width)),
                };
                DrawDictionaryAddPanel(addPanelRect, context);
            }
        }

        private static void DrawDictionaryHeader(Rect rect, DictionaryContext context)
        {
            int count = RuntimeUtil.IsNull(context.RawValue) ? 0 : GetDictionaryKeys(context).Count();
            Rect sizeRect = new Rect(rect)
            {
                x = rect.xMax - SizeWidth,
                width = SizeWidth,
            };
            Rect foldoutRect = new Rect(rect)
            {
                width = Mathf.Max(0f, rect.width - SizeWidth - ControlGap),
            };

            bool expanded = EditorGUI.Foldout(foldoutRect, IsExpanded(context.Key), new GUIContent(context.Label),
                true);
            SetExpanded(context.Key, expanded);

            using (new EditorGUI.DisabledScope(!CanEditDictionary(context)))
            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                int newSize = EditorGUI.DelayedIntField(sizeRect, count);
                if (changed.changed)
                {
                    ChangeDictionarySize(context, Mathf.Max(0, newSize));
                }
            }
        }

        private static float GetDictionaryRowHeight(DictionaryContext context, object key, object dictValue)
        {
            float keyHeight = IMGUIEdit.GetPropertyHeight("", context.KeyType, key, null,
                CanEditDictionary(context) ? _ => { } : null, context.LabelGrayColor, false,
                Array.Empty<Attribute>(), context.Targets, context.RichTextTagProvider,
                GetDictionaryElementKey(context, "key", key));
            float valueHeight = IMGUIEdit.GetPropertyHeight("", context.ValueValueType, dictValue, null,
                CanEditDictionary(context) ? _ => { } : null, context.LabelGrayColor, false,
                context.AllAttributes, context.Targets, context.RichTextTagProvider,
                GetDictionaryElementKey(context, "value", key));
            return Mathf.Max(keyHeight, valueHeight, EditorGUIUtility.singleLineHeight) + 2f;
        }

        private static void DrawDictionaryTableHeader(Rect rect)
        {
            GUI.Box(rect, GUIContent.none, EditorStyles.toolbar);
            (Rect keyRect, Rect valueRect) = GetDictionaryCellRects(rect);
            EditorGUI.LabelField(ShrinkRect(keyRect, CellPadding), "Keys", EditorStyles.miniBoldLabel);
            EditorGUI.LabelField(ShrinkRect(valueRect, CellPadding), "Values", EditorStyles.miniBoldLabel);
        }

        private static void DrawDictionaryRow(Rect rect, DictionaryContext context, int index, object key,
            object dictValue)
        {
            HandleDictionaryRowSelection(rect, context, index);

            bool selected = context.SelectedIndexes.Contains(index);
            if (selected)
            {
                EditorGUI.DrawRect(rect, new Color(0.24f, 0.48f, 0.90f, 0.35f));
            }
            else if (index % 2 == 1)
            {
                EditorGUI.DrawRect(rect, EditorGUIUtility.isProSkin
                    ? new Color(1f, 1f, 1f, 0.03f)
                    : new Color(0f, 0f, 0f, 0.04f));
            }

            (Rect keyRect, Rect valueRect) = GetDictionaryCellRects(rect);
            Rect separatorRect = new Rect(keyRect)
            {
                x = keyRect.xMax,
                width = 1f,
            };
            EditorGUI.DrawRect(separatorRect, EditorGUIUtility.isProSkin
                ? new Color(1f, 1f, 1f, 0.12f)
                : new Color(0f, 0f, 0f, 0.12f));

            using (new ZeroLabelWidthScope())
            {
                IMGUIEdit.OnGUI(ShrinkRect(keyRect, CellPadding), "", context.KeyType, key, null,
                    CanEditDictionary(context) ? newKey => ChangeDictionaryKey(context, key, newKey) : null,
                    context.LabelGrayColor, false, Array.Empty<Attribute>(), context.Targets,
                    context.RichTextTagProvider, GetDictionaryElementKey(context, "key", key));

                IMGUIEdit.OnGUI(ShrinkRect(valueRect, CellPadding), "", context.ValueValueType, dictValue, null,
                    CanEditDictionary(context) ? newValue => SetDictionaryValue(context, key, newValue) : null,
                    context.LabelGrayColor, false, context.AllAttributes, context.Targets,
                    context.RichTextTagProvider, GetDictionaryElementKey(context, "value", key));
            }
        }

        private static bool CanEditDictionary(DictionaryContext context) =>
            context.SetterOrNull != null
            && context.Error == ""
            && !context.IsReadOnly
            && !RuntimeUtil.IsNull(context.RawValue)
            && (context.RawValue is IDictionary dictionary
                ? !dictionary.IsReadOnly && !dictionary.IsFixedSize
                : context.IndexerProperty?.CanWrite == true
                  && context.RemoveMethod != null
                  && context.ContainsKeyMethod != null);

        private static IEnumerable<object> GetDictionaryKeys(DictionaryContext context)
        {
            if (RuntimeUtil.IsNull(context.RawValue))
            {
                return Array.Empty<object>();
            }

            if (context.RawValue is IDictionary dictionary)
            {
                return dictionary.Keys.Cast<object>();
            }

            if (context.KeysProperty?.GetValue(context.RawValue) is IEnumerable keys)
            {
                return keys.Cast<object>();
            }

            return Array.Empty<object>();
        }

        private static object GetDictionaryValue(DictionaryContext context, object key)
        {
            if (RuntimeUtil.IsNull(context.RawValue))
            {
                return null;
            }

            if (context.RawValue is IDictionary dictionary)
            {
                return dictionary[key];
            }

            return context.IndexerProperty?.GetValue(context.RawValue, new[] { key });
        }

        private static void SetDictionaryValue(DictionaryContext context, object key, object value)
        {
            if (!CanEditDictionary(context))
            {
                return;
            }

            string error = SetDictionaryValueRaw(context, key, value);
            if (error != "")
            {
                context.Error = error;
                return;
            }

            ApplyDictionaryValue(context);
        }

        private static void ChangeDictionaryKey(DictionaryContext context, object oldKey, object newKey)
        {
            if (!CanEditDictionary(context) || RuntimeUtil.IsNull(newKey) || Util.GetIsEqual(oldKey, newKey))
            {
                return;
            }

            (string containsError, bool contains) = DictionaryContainsKey(context, newKey);
            if (containsError != "" || contains)
            {
                context.Error = containsError;
                return;
            }

            object oldValue = GetDictionaryValue(context, oldKey);
            string setError = SetDictionaryValueRaw(context, newKey, oldValue);
            if (setError != "")
            {
                context.Error = setError;
                return;
            }

            string removeError = RemoveDictionaryEntryRaw(context, oldKey);
            if (removeError != "")
            {
                context.Error = removeError;
                return;
            }

            ApplyDictionaryValue(context);
        }

        private static void DrawDictionaryFooter(Rect rect, DictionaryContext context)
        {
            int count = RuntimeUtil.IsNull(context.RawValue) ? 0 : GetDictionaryKeys(context).Count();
            Rect addButtonRect = new Rect(rect)
            {
                x = rect.xMax - ButtonWidth,
                width = ButtonWidth,
            };
            Rect removeButtonRect = new Rect(addButtonRect)
            {
                x = addButtonRect.x - ControlGap - ButtonWidth,
            };
            Rect sizeRect = new Rect(rect)
            {
                width = SizeWidth,
            };
            Rect labelRect = new Rect(rect)
            {
                x = sizeRect.xMax + ControlGap,
                width = Mathf.Max(0f, removeButtonRect.x - sizeRect.xMax - ControlGap * 2),
            };

            using (new EditorGUI.DisabledScope(!CanEditDictionary(context)))
            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                int newSize = EditorGUI.DelayedIntField(sizeRect, count);
                if (changed.changed)
                {
                    ChangeDictionarySize(context, Mathf.Max(0, newSize));
                }
            }

            EditorGUI.LabelField(labelRect, "Items", EditorStyles.miniLabel);

            using (new EditorGUI.DisabledScope(!CanEditDictionary(context) || count == 0))
            {
                if (GUI.Button(removeButtonRect, "-", EditorStyles.miniButtonLeft))
                {
                    RemoveSelectedDictionaryEntries(context);
                }
            }

            using (new EditorGUI.DisabledScope(!CanEditDictionary(context) || context.AddPanelOpen))
            {
                if (GUI.Button(addButtonRect, "+", EditorStyles.miniButtonRight))
                {
                    OpenAddDictionaryPanel(context);
                }
            }
        }

        private static float GetDictionaryAddPanelHeight(DictionaryContext context, float width)
        {
            float contentWidth = Mathf.Max(0f, width - AddPanelPadding * 2);
            float height = AddPanelPadding * 2;
            height += IMGUIEdit.GetPropertyHeight("Key", context.KeyType, context.AddKey, null, _ => { },
                context.LabelGrayColor, context.InHorizontalLayout, Array.Empty<Attribute>(), context.Targets,
                context.RichTextTagProvider, $"{context.Key}.add.key");
            height += IMGUIEdit.GetPropertyHeight("Value", context.ValueValueType, context.AddValue, null, _ => { },
                context.LabelGrayColor, context.InHorizontalLayout, context.AllAttributes, context.Targets,
                context.RichTextTagProvider, $"{context.Key}.add.value");
            if (context.AddError != "")
            {
                height += ImGuiHelpBox.GetHeight(context.AddError, contentWidth, MessageType.Error);
            }

            height += EditorGUIUtility.singleLineHeight;
            return height;
        }

        private static void DrawDictionaryAddPanel(Rect rect, DictionaryContext context)
        {
            GUI.Box(rect, GUIContent.none, EditorStyles.helpBox);
            Rect leftRect = ShrinkRect(rect, AddPanelPadding);

            float keyHeight = IMGUIEdit.GetPropertyHeight("Key", context.KeyType, context.AddKey, null, _ => { },
                context.LabelGrayColor, context.InHorizontalLayout, Array.Empty<Attribute>(), context.Targets,
                context.RichTextTagProvider, $"{context.Key}.add.key");
            (Rect keyRect, Rect afterKeyRect) = RectUtils.SplitHeightRect(leftRect, keyHeight);
            IMGUIEdit.OnGUI(keyRect, "Key", context.KeyType, context.AddKey, null, newKey =>
            {
                context.AddKey = newKey;
                context.AddError = GetAddKeyError(context);
            }, context.LabelGrayColor, context.InHorizontalLayout, Array.Empty<Attribute>(), context.Targets,
                context.RichTextTagProvider, $"{context.Key}.add.key");

            float valueHeight = IMGUIEdit.GetPropertyHeight("Value", context.ValueValueType, context.AddValue, null,
                _ => { }, context.LabelGrayColor, context.InHorizontalLayout, context.AllAttributes, context.Targets,
                context.RichTextTagProvider, $"{context.Key}.add.value");
            (Rect valueRect, Rect afterValueRect) = RectUtils.SplitHeightRect(afterKeyRect, valueHeight);
            IMGUIEdit.OnGUI(valueRect, "Value", context.ValueValueType, context.AddValue, null,
                newValue => context.AddValue = newValue, context.LabelGrayColor, context.InHorizontalLayout,
                context.AllAttributes, context.Targets, context.RichTextTagProvider, $"{context.Key}.add.value");

            leftRect = afterValueRect;
            if (context.AddError != "")
            {
                (Rect errorRect, Rect afterErrorRect) = RectUtils.SplitHeightRect(leftRect,
                    ImGuiHelpBox.GetHeight(context.AddError, leftRect.width, MessageType.Error));
                ImGuiHelpBox.Draw(errorRect, context.AddError, MessageType.Error);
                leftRect = afterErrorRect;
            }

            (Rect buttonRect, _) = RectUtils.SplitHeightRect(leftRect, EditorGUIUtility.singleLineHeight);
            (Rect okRect, Rect cancelRect) = RectUtils.SplitWidthRect(buttonRect, buttonRect.width * 0.5f);
            okRect.width = Mathf.Max(0f, okRect.width - 1f);
            cancelRect.x += 1f;
            cancelRect.width = Mathf.Max(0f, cancelRect.width - 1f);

            using (new EditorGUI.DisabledScope(context.AddError != ""))
            {
                if (GUI.Button(okRect, "OK"))
                {
                    ConfirmAddDictionaryEntry(context);
                }
            }

            if (GUI.Button(cancelRect, "Cancel"))
            {
                context.AddPanelOpen = false;
            }
        }

        private static void OpenAddDictionaryPanel(DictionaryContext context)
        {
            if (!CanEditDictionary(context))
            {
                return;
            }

            context.AddKey = CreateDictionaryPanelDefaultValue(context.KeyType);
            context.AddValue = CreateDictionaryPanelDefaultValue(context.ValueValueType);
            context.AddPanelOpen = true;
            context.AddError = GetAddKeyError(context);
        }

        private static void ConfirmAddDictionaryEntry(DictionaryContext context)
        {
            context.AddError = GetAddKeyError(context);
            if (context.AddError != "")
            {
                return;
            }

            string error = SetDictionaryValueRaw(context, context.AddKey, context.AddValue);
            if (error != "")
            {
                context.AddError = error;
                return;
            }

            context.AddPanelOpen = false;
            context.SelectedIndexes.Clear();
            ApplyDictionaryValue(context);
            GUI.changed = true;
        }

        private static string GetAddKeyError(DictionaryContext context)
        {
            if (RuntimeUtil.IsNull(context.AddKey))
            {
                return "Key can not be null.";
            }

            (string error, bool contains) = DictionaryContainsKey(context, context.AddKey);
            if (error != "")
            {
                return error;
            }

            return contains ? "Key already exists." : "";
        }

        private static object CreateDictionaryPanelDefaultValue(Type type) =>
            type?.IsValueType == true ? Activator.CreateInstance(type) : null;

        private static void ChangeDictionarySize(DictionaryContext context, int newSize)
        {
            if (!CanEditDictionary(context))
            {
                return;
            }

            object[] keys = GetDictionaryKeys(context).ToArray();
            if (newSize == keys.Length)
            {
                return;
            }

            if (newSize > keys.Length)
            {
                OpenAddDictionaryPanel(context);
                return;
            }

            RemoveDictionaryEntries(context, keys.Skip(newSize));
        }

        private static void RemoveSelectedDictionaryEntries(DictionaryContext context)
        {
            object[] keys = GetDictionaryKeys(context).ToArray();
            if (keys.Length == 0)
            {
                return;
            }

            List<int> removeIndexes = context.SelectedIndexes
                .Where(each => each >= 0 && each < keys.Length)
                .OrderByDescending(each => each)
                .ToList();
            if (removeIndexes.Count == 0)
            {
                removeIndexes.Add(keys.Length - 1);
            }

            RemoveDictionaryEntries(context, removeIndexes.Select(index => keys[index]));
        }

        private static void RemoveDictionaryEntries(DictionaryContext context, IEnumerable<object> keys)
        {
            if (!CanEditDictionary(context))
            {
                return;
            }

            string error = "";
            foreach (object key in keys.ToArray())
            {
                error = RemoveDictionaryEntryRaw(context, key);
                if (error != "")
                {
                    break;
                }
            }

            if (error != "")
            {
                context.Error = error;
                return;
            }

            context.SelectedIndexes.Clear();
            ApplyDictionaryValue(context);
            GUI.changed = true;
        }

        private static (string error, bool contains) DictionaryContainsKey(DictionaryContext context, object key)
        {
            try
            {
                if (context.RawValue is IDictionary dictionary)
                {
                    return ("", dictionary.Contains(key));
                }

                if (context.ContainsKeyMethod == null)
                {
                    return ("Dictionary value edit requires a ContainsKey method.", false);
                }

                return ("", (bool)context.ContainsKeyMethod.Invoke(context.RawValue, new[] { key }));
            }
            catch (Exception e)
            {
                return (GetExceptionMessage(e), false);
            }
        }

        private static string SetDictionaryValueRaw(DictionaryContext context, object key, object value)
        {
            try
            {
                if (context.RawValue is IDictionary dictionary)
                {
                    dictionary[key] = value;
                    return "";
                }

                if (context.IndexerProperty == null)
                {
                    return "Dictionary value edit requires an indexer property.";
                }

                context.IndexerProperty.SetValue(context.RawValue, value, new[] { key });
                return "";
            }
            catch (Exception e)
            {
                return GetExceptionMessage(e);
            }
        }

        private static string RemoveDictionaryEntryRaw(DictionaryContext context, object key)
        {
            try
            {
                if (context.RawValue is IDictionary dictionary)
                {
                    dictionary.Remove(key);
                    return "";
                }

                if (context.RemoveMethod == null)
                {
                    return "Dictionary value edit requires a Remove method.";
                }

                context.RemoveMethod.Invoke(context.RawValue, new[] { key });
                return "";
            }
            catch (Exception e)
            {
                return GetExceptionMessage(e);
            }
        }

        private static void HandleDictionaryRowSelection(Rect rect, DictionaryContext context, int index)
        {
            Event current = Event.current;
            if (current.type != EventType.MouseDown || current.button != 0 || !rect.Contains(current.mousePosition))
            {
                return;
            }

            if (current.control || current.command)
            {
                if (context.SelectedIndexes.Contains(index))
                {
                    context.SelectedIndexes.Remove(index);
                }
                else
                {
                    context.SelectedIndexes.Add(index);
                }
            }
            else
            {
                context.SelectedIndexes.Clear();
                context.SelectedIndexes.Add(index);
            }

            GUI.changed = true;
        }

        private static void ClampDictionarySelection(DictionaryContext context, int count)
        {
            context.SelectedIndexes.RemoveWhere(each => each < 0 || each >= count);
        }

        private static (Rect keyRect, Rect valueRect) GetDictionaryCellRects(Rect rect)
        {
            float keyWidth = Mathf.Max(40f, rect.width * 0.5f);
            Rect keyRect = new Rect(rect)
            {
                width = Mathf.Min(keyWidth, rect.width),
            };
            Rect valueRect = new Rect(rect)
            {
                x = keyRect.xMax + 1f,
                width = Mathf.Max(0f, rect.width - keyRect.width - 1f),
            };
            return (keyRect, valueRect);
        }

        private static Rect ShrinkRect(Rect rect, float padding) => new Rect(rect)
        {
            x = rect.x + padding,
            y = rect.y + padding,
            width = Mathf.Max(0f, rect.width - padding * 2),
            height = Mathf.Max(0f, rect.height - padding * 2),
        };

        private static string GetDictionaryElementKey(DictionaryContext context, string part, object key) =>
            $"{context.Key}.{part}.{key?.GetHashCode() ?? 0}";

        private static string GetExceptionMessage(Exception exception) =>
            exception is TargetInvocationException { InnerException: { } innerException }
                ? innerException.Message
                : exception.Message;

        private sealed class ZeroLabelWidthScope : IDisposable
        {
            private readonly float _oldLabelWidth;

            public ZeroLabelWidthScope()
            {
                _oldLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 1f;
            }

            public void Dispose()
            {
                EditorGUIUtility.labelWidth = _oldLabelWidth;
            }
        }

        private static void ApplyDictionaryValue(DictionaryContext context)
        {
            context.BeforeSet?.Invoke(context.RawValue);
            context.SetterOrNull?.Invoke(context.RawValue);
        }
    }
}

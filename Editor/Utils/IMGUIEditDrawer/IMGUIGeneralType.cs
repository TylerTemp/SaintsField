using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.ReferencePicker;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Editor.Utils.IMGUIPlainDrawer;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Utils.IMGUIEditDrawer
{
    // normal class/struct, like GeneralTypeEdit
    public static class IMGUIGeneralType
    {
        private const float HeaderPadding = 2f;
        private const float FoldoutWidth = 12f;
        private const float FieldSpacing = 2f;
        private const float ChildIndentWidth = 15f;

        private static readonly Dictionary<string, Type> UnityObjectOverrideTypes = new Dictionary<string, Type>();

        private static GUIStyle _richPopupStyle;
        private static GUIStyle RichPopupStyle => _richPopupStyle ??= new GUIStyle(EditorStyles.popup)
        {
            richText = true,
        };

        private static GUIStyle _richButtonStyle;
        private static GUIStyle RichButtonStyle => _richButtonStyle ??= new GUIStyle(GUI.skin.button)
        {
            richText = true,
            alignment = TextAnchor.MiddleLeft,
            padding = new RectOffset(6, 6, 0, 0),
        };

        private static float HeaderHeight => HeaderPadding + EditorGUIUtility.singleLineHeight;
        private static float ErrorHeight => EditorGUIUtility.singleLineHeight * 2f + HeaderPadding;

        public static float GetPropertyHeight(
            string label, Type valueType, object value, Action<object> beforeSet, Action<object> setterOrNull,
            bool labelGrayColor, bool inHorizontalLayout,
            IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets,
            IRichTextTagProvider richTextTagProvider,
            string foldoutViewKey)
        {
            string key = MakeKey(label, valueType, foldoutViewKey);
            GeneralContext context = BuildContext(valueType, value, key);
            bool expanded = IsExpanded(key) && context.AllowExpand;

            if (!expanded)
            {
                return HeaderHeight;
            }

            float height = HeaderHeight;
            if (ShouldDrawUnityObjectField(context, value))
            {
                return height + HeaderHeight;
            }

            if (context.ValueIsNull)
            {
                return height;
            }

            (IReadOnlyList<FieldInfo> fields, IReadOnlyList<PropertyInfo> properties) =
                GetDrawableMembers(value.GetType());

            foreach (FieldInfo fieldInfo in fields)
            {
                if (!TryGetMemberValue(value, fieldInfo, out object fieldValue, out _))
                {
                    height += ErrorHeight;
                    continue;
                }

                string subKey = $"{key}.{fieldInfo.Name}";
                height += IMGUIEdit.GetPropertyHeight(
                    ObjectNames.NicifyVariableName(fieldInfo.Name),
                    fieldInfo.FieldType,
                    fieldValue,
                    beforeSet,
                    setterOrNull,
                    labelGrayColor,
                    inHorizontalLayout,
                    ReflectCache.GetCustomAttributes(fieldInfo),
                    targets,
                    richTextTagProvider,
                    subKey);
            }

            foreach (PropertyInfo propertyInfo in properties)
            {
                if (!TryGetMemberValue(value, propertyInfo, out object propertyValue, out _))
                {
                    height += ErrorHeight;
                    continue;
                }

                string subKey = $"{key}.{propertyInfo.Name}";
                height += IMGUIEdit.GetPropertyHeight(
                    ObjectNames.NicifyVariableName(propertyInfo.Name),
                    propertyInfo.PropertyType,
                    propertyValue,
                    beforeSet,
                    propertyInfo.CanWrite ? setterOrNull : null,
                    labelGrayColor,
                    inHorizontalLayout,
                    ReflectCache.GetCustomAttributes(propertyInfo),
                    targets,
                    richTextTagProvider,
                    subKey);
            }

            return height;
        }

        public static void OnGUI(
            Rect position,
            string label, Type valueType, object value, Action<object> beforeSet, Action<object> setterOrNull,
            bool labelGrayColor, bool inHorizontalLayout,
            IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets,
            IRichTextTagProvider richTextTagProvider,
            string foldoutViewKey)
        {
            string key = MakeKey(label, valueType, foldoutViewKey);
            GeneralContext context = BuildContext(valueType, value, key);
            bool expanded = IsExpanded(key) && context.AllowExpand;

            Rect headerRect = new Rect(position)
            {
                height = EditorGUIUtility.singleLineHeight,
            };

            bool changedByHeader = DrawHeader(headerRect, label, value, beforeSet, setterOrNull, labelGrayColor, key,
                context, expanded);
            if (changedByHeader || !context.AllowExpand || !IsExpanded(key))
            {
                return;
            }

            Rect bodyRect = new Rect(position)
            {
                y = position.y + HeaderHeight,
                height = Mathf.Max(0f, position.height - HeaderHeight),
            };

            if (ShouldDrawUnityObjectField(context, value))
            {
                DrawUnityObjectField(bodyRect, value, beforeSet, setterOrNull, context);
                return;
            }

            if (context.ValueIsNull)
            {
                return;
            }

            DrawMembers(bodyRect, value, beforeSet, setterOrNull, labelGrayColor, inHorizontalLayout, targets,
                richTextTagProvider, key);
        }

        private static bool DrawHeader(
            Rect position,
            string label,
            object value,
            Action<object> beforeSet,
            Action<object> setterOrNull,
            bool labelGrayColor,
            string key,
            GeneralContext context,
            bool expanded)
        {
            Rect lineRect = position;
            if (context.AllowExpand)
            {
                Rect foldoutRect = new Rect(lineRect)
                {
                    width = FoldoutWidth,
                };
                bool newExpanded = GUI.Toggle(foldoutRect, expanded, GUIContent.none, EditorStyles.foldout);
                if (newExpanded != expanded)
                {
                    IMGUIEdit.ViewKey[key] = newExpanded;
                    expanded = newExpanded;
                }

                lineRect.xMin = foldoutRect.xMax + FieldSpacing;
            }
            else
            {
                IMGUIEdit.ViewKey[key] = false;
            }

            Rect fieldRect = lineRect;
            if (!string.IsNullOrEmpty(label))
            {
                float labelWidth = Mathf.Min(EditorGUIUtility.labelWidth, Mathf.Max(0f, lineRect.width - 80f));
                Rect labelRect = new Rect(lineRect)
                {
                    width = labelWidth,
                };
                fieldRect = new Rect(lineRect)
                {
                    xMin = labelRect.xMax,
                };

                using (new LabelColorScoop(labelGrayColor))
                {
                    if (context.AllowExpand && GUI.Button(labelRect, label, EditorStyles.label))
                    {
                        IMGUIEdit.ViewKey[key] = !expanded;
                    }
                    else if (!context.AllowExpand)
                    {
                        EditorGUI.LabelField(labelRect, label);
                    }
                }
            }

            return DrawTypeControl(fieldRect, value, beforeSet, setterOrNull, key, context);
        }

        private static bool DrawTypeControl(
            Rect position,
            object value,
            Action<object> beforeSet,
            Action<object> setterOrNull,
            string key,
            GeneralContext context)
        {
            bool canEdit = setterOrNull != null;
            if (context.OptionTypes.Length <= 1 && context.CanBeNull)
            {
                Type createType = context.OptionTypes.Length == 1 ? context.OptionTypes[0] : null;
                bool hasValue = !context.ValueIsNull || context.OnUnityType;
                string text = GetNullOrCreateText(context, createType, hasValue);
                bool enabled = canEdit && (hasValue || createType != null);

                using (new GUIEnabledScoop(enabled))
                {
                    if (GUI.Button(position, text, RichButtonStyle))
                    {
                        SetToType(hasValue ? null : createType, value, beforeSet, setterOrNull, key, context);
                        return true;
                    }
                }

                return false;
            }

            if (context.OptionTypes.Length <= 1)
            {
                using (new GUIEnabledScoop(false))
                {
                    GUI.Button(position, GetDropdownLabel(context), RichButtonStyle);
                }

                return false;
            }

            using (new GUIEnabledScoop(canEdit))
            {
                if (!GUI.Button(position, GetDropdownLabel(context), RichPopupStyle))
                {
                    return false;
                }
            }

            AdvancedDropdownMetaInfo metaInfo = new AdvancedDropdownMetaInfo
            {
                CurValues = context.SelectedType == null ? Array.Empty<object>() : new object[] { context.SelectedType },
                DropdownListValue = BuildDropdownList(context),
                SelectStacks = Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>(),
            };

            PopupWindow.Show(position, new SaintsTreeDropdownIMGUI(
                metaInfo,
                Mathf.Max(position.width, 220f),
                320f,
                false,
                (curItem, _) =>
                {
                    SetToType((Type)curItem, value, beforeSet, setterOrNull, key, context);
                    return null;
                }));

            return false;
        }

        private static void DrawUnityObjectField(
            Rect position,
            object value,
            Action<object> beforeSet,
            Action<object> setterOrNull,
            GeneralContext context)
        {
            Rect fieldRect = new Rect(position)
            {
                x = position.x + ChildIndentWidth,
                width = Mathf.Max(0f, position.width - ChildIndentWidth),
                height = EditorGUIUtility.singleLineHeight,
            };

            using (new GUIEnabledScoop(setterOrNull != null))
            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                Object result = EditorGUI.ObjectField(fieldRect, GUIContent.none, (Object)value,
                    context.UnityObjectOverrideType, true);
                if (!changed.changed || setterOrNull == null)
                {
                    return;
                }

                beforeSet?.Invoke(value);
                setterOrNull(result);
            }
        }

        private static void DrawMembers(
            Rect position,
            object value,
            Action<object> beforeSet,
            Action<object> setterOrNull,
            bool labelGrayColor,
            bool inHorizontalLayout,
            IReadOnlyList<object> targets,
            IRichTextTagProvider richTextTagProvider,
            string key)
        {
            Rect indentedRect = new Rect(position)
            {
                x = position.x + ChildIndentWidth,
                width = Mathf.Max(0f, position.width - ChildIndentWidth),
            };
            float y = indentedRect.y;

            (IReadOnlyList<FieldInfo> fields, IReadOnlyList<PropertyInfo> properties) =
                GetDrawableMembers(value.GetType());

            foreach (FieldInfo fieldInfo in fields)
            {
                string subKey = $"{key}.{fieldInfo.Name}";
                string childLabel = ObjectNames.NicifyVariableName(fieldInfo.Name);
                if (!TryGetMemberValue(value, fieldInfo, out object fieldValue, out string error))
                {
                    Rect errorRect = NextRect(indentedRect, ref y, ErrorHeight);
                    EditorGUI.HelpBox(errorRect, error, MessageType.Error);
                    continue;
                }

                float height = IMGUIEdit.GetPropertyHeight(childLabel, fieldInfo.FieldType, fieldValue,
                    beforeSet, setterOrNull, labelGrayColor, inHorizontalLayout,
                    ReflectCache.GetCustomAttributes(fieldInfo), targets, richTextTagProvider, subKey);
                Rect childRect = NextRect(indentedRect, ref y, height);

                IMGUIEdit.OnGUI(
                    childRect,
                    childLabel,
                    fieldInfo.FieldType,
                    fieldValue,
                    _ => beforeSet?.Invoke(value),
                    newValue =>
                    {
                        fieldInfo.SetValue(value, newValue);
                        setterOrNull?.Invoke(value);
                    },
                    labelGrayColor,
                    inHorizontalLayout,
                    ReflectCache.GetCustomAttributes(fieldInfo),
                    targets,
                    richTextTagProvider,
                    subKey);
            }

            foreach (PropertyInfo propertyInfo in properties)
            {
                string subKey = $"{key}.{propertyInfo.Name}";
                string childLabel = ObjectNames.NicifyVariableName(propertyInfo.Name);
                if (!TryGetMemberValue(value, propertyInfo, out object propertyValue, out string error))
                {
                    Rect errorRect = NextRect(indentedRect, ref y, ErrorHeight);
                    EditorGUI.HelpBox(errorRect, error, MessageType.Error);
                    continue;
                }

                Action<object> propertySetter = propertyInfo.CanWrite
                    ? newValue =>
                    {
                        propertyInfo.SetValue(value, newValue);
                        setterOrNull?.Invoke(value);
                    }
                    : null;

                float height = IMGUIEdit.GetPropertyHeight(childLabel, propertyInfo.PropertyType, propertyValue,
                    beforeSet, propertySetter, labelGrayColor, inHorizontalLayout,
                    ReflectCache.GetCustomAttributes(propertyInfo), targets, richTextTagProvider, subKey);
                Rect childRect = NextRect(indentedRect, ref y, height);

                IMGUIEdit.OnGUI(
                    childRect,
                    childLabel,
                    propertyInfo.PropertyType,
                    propertyValue,
                    _ => beforeSet?.Invoke(value),
                    propertySetter,
                    labelGrayColor,
                    inHorizontalLayout,
                    ReflectCache.GetCustomAttributes(propertyInfo),
                    targets,
                    richTextTagProvider,
                    subKey);
            }
        }

        private static Rect NextRect(Rect source, ref float y, float height)
        {
            Rect rect = new Rect(source)
            {
                y = y,
                height = height,
            };
            y += height;
            return rect;
        }

        private static bool ShouldDrawUnityObjectField(GeneralContext context, object value)
        {
            return context.UnityObjectOverrideType != null &&
                   (context.ValueIsNull || context.CanHaveUnityTypes.Contains(value.GetType()));
        }

        private static void SetToType(
            Type newType,
            object currentValue,
            Action<object> beforeSet,
            Action<object> setterOrNull,
            string key,
            GeneralContext context)
        {
            if (setterOrNull == null)
            {
                return;
            }

            beforeSet?.Invoke(currentValue);

            bool isUnityObjectType = newType != null && context.CanHaveUnityTypes.Contains(newType);
            if (isUnityObjectType)
            {
                UnityObjectOverrideTypes[key] = newType;
            }
            else
            {
                UnityObjectOverrideTypes.Remove(key);
            }

            if (newType == null || isUnityObjectType)
            {
                setterOrNull(null);
                if (newType != null)
                {
                    IMGUIEdit.ViewKey[key] = true;
                }

                return;
            }

            object newValue;
            try
            {
                newValue = Activator.CreateInstance(newType);
            }
            catch (Exception)
            {
                newValue = RuntimeHelpers.GetUninitializedObject(newType);
            }

            setterOrNull(ReferencePickerAttributeDrawer.CopyObj(currentValue, newValue));
            IMGUIEdit.ViewKey[key] = true;
        }

        private static GeneralContext BuildContext(Type valueType, object value, string key)
        {
            bool valueIsNull = RuntimeUtil.IsNull(value);
            Type fieldType = valueType ?? (valueIsNull ? null : value.GetType());

            Type[] canHaveUnityTypes = fieldType == null
                ? Array.Empty<Type>()
                : TypeCache.GetTypesDerivedFrom(fieldType)
                    .Prepend(fieldType)
                    .Where(each => !each.IsAbstract)
                    .Where(each => !each.ContainsGenericParameters)
                    .Where(each => typeof(Object).IsAssignableFrom(each))
                    .ToArray();

            if (!valueIsNull && canHaveUnityTypes.Contains(value.GetType()))
            {
                UnityObjectOverrideTypes[key] = value.GetType();
            }

            UnityObjectOverrideTypes.TryGetValue(key, out Type unityObjectOverrideType);
            if (unityObjectOverrideType != null && !canHaveUnityTypes.Contains(unityObjectOverrideType))
            {
                UnityObjectOverrideTypes.Remove(key);
                unityObjectOverrideType = null;
            }

            bool onUnityType = unityObjectOverrideType != null && canHaveUnityTypes.Contains(unityObjectOverrideType);
            Type[] optionTypes = fieldType == null
                ? Array.Empty<Type>()
                : ReferencePickerAttributeDrawer.GetTypesDerivedFrom(fieldType).ToArray();

            return new GeneralContext
            {
                FieldType = fieldType,
                ValueIsNull = valueIsNull,
                CanBeNull = fieldType == null || !fieldType.IsValueType,
                CanHaveUnityTypes = canHaveUnityTypes,
                UnityObjectOverrideType = onUnityType ? unityObjectOverrideType : null,
                OnUnityType = onUnityType,
                OptionTypes = optionTypes,
                SelectedType = onUnityType
                    ? unityObjectOverrideType
                    : valueIsNull
                        ? null
                        : value.GetType(),
                AllowExpand = !valueIsNull || onUnityType,
            };
        }

        private static Dropdown<Type> BuildDropdownList(GeneralContext context)
        {
            Dropdown<Type> dropdown = new Dropdown<Type>();
            if (context.CanBeNull)
            {
                dropdown.Add("[Null]", null);
                if (context.OptionTypes.Length > 0)
                {
                    dropdown.AddSeparator();
                }
            }

            Dictionary<string, List<Type>> namespaceToTypes = new Dictionary<string, List<Type>>();
            foreach (Type type in context.OptionTypes)
            {
                string typeNamespace = type.Namespace ?? "";
                if (!namespaceToTypes.TryGetValue(typeNamespace, out List<Type> groupedTypes))
                {
                    namespaceToTypes[typeNamespace] = groupedTypes = new List<Type>();
                }

                groupedTypes.Add(type);
            }

            foreach (string typeNamespace in namespaceToTypes.Keys.OrderBy(each => each))
            {
                Dropdown<Type> namespaceDropdown =
                    new Dropdown<Type>(typeNamespace == "" ? "[No Namespace]" : typeNamespace);
                foreach (Type type in namespaceToTypes[typeNamespace])
                {
                    namespaceDropdown.Add(type.Name, type);
                }

                dropdown.Add(namespaceDropdown);
            }

            return dropdown;
        }

        private static string GetDropdownLabel(GeneralContext context)
        {
            if (context.ValueIsNull)
            {
                return context.OnUnityType ? FormatTypeName(context.UnityObjectOverrideType) : "Null";
            }

            return FormatTypeName(context.SelectedType);
        }

        private static string GetNullOrCreateText(GeneralContext context, Type createType, bool hasValue)
        {
            if (hasValue)
            {
                Type displayType = context.OnUnityType ? context.UnityObjectOverrideType : context.SelectedType;
                return $"{FormatTypeName(displayType)} -> Null";
            }

            if (createType == null)
            {
                return context.FieldType == null ? "Null" : $"Null ({FormatTypeName(context.FieldType)})";
            }

            return $"Null -> {FormatTypeName(createType)}";
        }

        private static string FormatTypeName(Type type)
        {
            if (type == null)
            {
                return "Null";
            }

            return $"{type.Name} <color=#808080>({type.Namespace})</color>";
        }

        private static bool IsExpanded(string key)
        {
            return IMGUIEdit.ViewKey.TryGetValue(key, out bool expanded) && expanded;
        }

        private static string MakeKey(string label, Type valueType, string foldoutViewKey)
        {
            return string.IsNullOrEmpty(foldoutViewKey) ? $"{label}.{valueType?.FullName}" : foldoutViewKey;
        }

        private static (IReadOnlyList<FieldInfo> fields, IReadOnlyList<PropertyInfo> properties) GetDrawableMembers(
            Type type)
        {
            const BindingFlags bindAttrNormal = BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy;

            List<FieldInfo> fields = type.GetFields(bindAttrNormal).ToList();
            Dictionary<string, FieldInfo> backingToFieldInfo = fields
                .Where(each => each.Name.StartsWith("<") && each.Name.EndsWith(">k__BackingField"))
                .ToDictionary(each => each.Name);

            List<PropertyInfo> properties = type.GetProperties(bindAttrNormal)
                .Where(each => each.CanRead)
                .Where(each => each.GetIndexParameters().Length == 0)
                .ToList();

            foreach (PropertyInfo propertyInfo in properties)
            {
                string backingName = $"<{propertyInfo.Name}>k__BackingField";
                if (backingToFieldInfo.TryGetValue(backingName, out FieldInfo duplicateInfo))
                {
                    fields.Remove(duplicateInfo);
                }
            }

            return (
                fields.Where(each => !AbsRenderer.SkipTypeDrawing(each.FieldType)).ToArray(),
                properties.Where(each => !AbsRenderer.SkipTypeDrawing(each.PropertyType)).ToArray());
        }

        private static bool TryGetMemberValue(object target, FieldInfo fieldInfo, out object value, out string error)
        {
            try
            {
                value = fieldInfo.GetValue(target);
                error = "";
                return true;
            }
            catch (Exception e)
            {
                value = null;
                error = e.InnerException?.Message ?? e.Message;
                return false;
            }
        }

        private static bool TryGetMemberValue(object target, PropertyInfo propertyInfo, out object value, out string error)
        {
            try
            {
                value = propertyInfo.GetValue(target);
                error = "";
                return true;
            }
            catch (Exception e)
            {
                value = null;
                error = e.InnerException?.Message ?? e.Message;
                return false;
            }
        }

        private sealed class GeneralContext
        {
            public Type FieldType;
            public bool ValueIsNull;
            public bool CanBeNull;
            public bool AllowExpand;
            public bool OnUnityType;
            public Type UnityObjectOverrideType;
            public Type SelectedType;
            public Type[] CanHaveUnityTypes = Array.Empty<Type>();
            public Type[] OptionTypes = Array.Empty<Type>();
        }
    }
}

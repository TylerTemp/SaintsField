using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.EnumFlagsDrawers.FlagsTreeDropdownDrawer;
#if UNITY_2021_3_OR_NEWER
using SaintsField.Editor.Drawers.ReferencePicker;
#endif
using SaintsField.Editor.Drawers.SaintsRowDrawer;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Utils.IMGUIPlainDrawer
{
    public static class IMGUIRawDraw
    {
        private const int DefaultDrawerCacheIndex = 0;
        private const int ReferencePickerDrawerCacheIndex = 2;
        private const int EnumDropdownDrawerCacheIndex = 3;

        public static readonly Color LabelGrayColor = EColor.EditorSeparator.GetColor();
        private const float VerticalPadding = 1f;
        private static RichTextDrawer _richTextDrawer;

        public static bool UseWideMode() => EditorGUIUtility.wideMode || EditorGUIUtility.currentViewWidth > 330f;

        private static float GetSingleLineHeight(bool inHorizontalLayout) =>
            EditorGUIUtility.singleLineHeight * (inHorizontalLayout ? 2 : 1) + VerticalPadding * 2;

        private static float GetResponsiveMultiLineHeight(bool inHorizontalLayout, int narrowRows) =>
            EditorGUIUtility.singleLineHeight * ((inHorizontalLayout || !UseWideMode()) ? narrowRows : 1) +
            VerticalPadding * 2;

        private static Rect GetContentRect(Rect position) => new Rect(position)
        {
            y = position.y + VerticalPadding,
            height = Mathf.Max(0f, position.height - VerticalPadding * 2),
        };

        private static void DrawRichText(Rect labelRect, IEnumerable<RichTextDrawer.RichTextChunk> richTextChunks)
        {
            if (richTextChunks == null)
            {
                return;
            }

            _richTextDrawer ??= new RichTextDrawer();
            _richTextDrawer.DrawChunks(labelRect, richTextChunks);
        }

        private static void DrawSingleLineRichText(Rect position, GUIContent label,
            IEnumerable<RichTextDrawer.RichTextChunk> richTextChunks, bool inHorizontalLayout)
        {
            if (richTextChunks == null)
            {
                return;
            }

            Rect contentRect = GetContentRect(position);
            Rect labelRect = new Rect(contentRect)
            {
                height = EditorGUIUtility.singleLineHeight,
            };

            if (!inHorizontalLayout)
            {
                labelRect.width = label.text == "" ? 0f : Mathf.Min(EditorGUIUtility.labelWidth, contentRect.width);
            }

            DrawRichText(labelRect, richTextChunks);
        }

        private static void DrawResponsiveRichText(Rect position, GUIContent label,
            IEnumerable<RichTextDrawer.RichTextChunk> richTextChunks, bool inHorizontalLayout)
        {
            if (richTextChunks == null)
            {
                return;
            }

            Rect labelRect = new Rect(position)
            {
                height = EditorGUIUtility.singleLineHeight,
            };

            if (!inHorizontalLayout && UseWideMode())
            {
                labelRect.width = label.text == "" ? 0f : Mathf.Min(EditorGUIUtility.labelWidth, position.width);
            }

            DrawRichText(labelRect, richTextChunks);
        }

        public static (Type drawerType, Attribute drawerAttribute) GetDrawerAndAttribute(
            SerializedProperty property,
            IReadOnlyList<Attribute> allAttributes,
            FieldInfo fieldInfo)
        {

            (Type useDrawerType, Attribute useAttribute) = Util.GetDrawerAndAttribute(property, allAttributes, fieldInfo);

            if (useDrawerType == null)
            {
                return (null, null);
            }

            MethodInfo imGuiMethod = useDrawerType.GetMethod(
                "OnGUI",
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                types: new[] { typeof(Rect), typeof(SerializedProperty), typeof(GUIContent) },
                modifiers: null
            );
            if (imGuiMethod == null)
            {
                return (null, null);
            }

            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (imGuiMethod.DeclaringType == typeof(PropertyDrawer))
            {
#if SAINTSFIELD_DEBUG
                Debug.LogWarning($"{useDrawerType.FullName} has no OnGUI for {fieldInfo.FieldType.FullName}");
#endif
                // no override means it is very likely to be a UI Toolkit drawer.
                return (typeof(IMGUIGetUIToolkitDrawer), null);
            }

            return (useDrawerType, useAttribute);
        }

        public static PropertyDrawer GetAndCacheDrawer(SerializedProperty property, IReadOnlyList<Attribute> allAttributes, FieldInfo fieldInfo, string label)
        {
            (Type useDrawerType, Attribute useAttribute) = GetDrawerAndAttribute(
                property,
                allAttributes,
                fieldInfo);

            IMGUIDrawerCache.DrawerId drawerKey = new IMGUIDrawerCache.DrawerId(property, DefaultDrawerCacheIndex);
            if (!IMGUIDrawerCache.CachedDrawers.TryGetValue(drawerKey, out PropertyDrawer imguiDrawer))
            {
                PropertyDrawer drawerInstance = null;
                if (useDrawerType != null)
                {
                    drawerInstance = typeof(SaintsPropertyDrawer).IsAssignableFrom(useDrawerType)
                        ? SaintsPropertyDrawer.MakePropertyDrawer(useDrawerType, fieldInfo, useAttribute, label)
                        : new UnityPropertyFieldProxyDrawer();
                }

                imguiDrawer =
                    IMGUIDrawerCache.CachedDrawers[drawerKey] =
                        drawerInstance;
            }

            return imguiDrawer;
        }

        private static SaintsRowAttributeDrawer GetAndCacheSaintsRowDrawer(SerializedProperty property, FieldInfo fieldInfo, string label,
            bool inHorizontalLayout)
        {
            IMGUIDrawerCache.DrawerId drawerKey = new IMGUIDrawerCache.DrawerId(property, DefaultDrawerCacheIndex);
            if (IMGUIDrawerCache.CachedSaintsRowDrawers.TryGetValue(drawerKey, out SaintsRowAttributeDrawer saintsRowDrawer))
            {
                return saintsRowDrawer;
            }

            saintsRowDrawer = (SaintsRowAttributeDrawer)SaintsPropertyDrawer.MakePropertyDrawer(typeof(SaintsRowAttributeDrawer), fieldInfo,
                new SaintsRowAttribute(), label);
            saintsRowDrawer.InHorizontalLayout = inHorizontalLayout;
            return IMGUIDrawerCache.CachedSaintsRowDrawers[drawerKey] = saintsRowDrawer;
        }

#if UNITY_2021_3_OR_NEWER
        private static bool NeedReferencePickerDrawer(SerializedProperty property, IReadOnlyList<Attribute> allAttributes)
            => property.propertyType == SerializedPropertyType.ManagedReference &&
               allAttributes.All(each => each is not ReferencePickerAttribute);

        private static ReferencePickerAttributeDrawer GetAndCacheReferencePickerDrawer(SerializedProperty property,
            FieldInfo fieldInfo, string label, bool inHorizontalLayout)
        {
            IMGUIDrawerCache.DrawerId drawerKey = new IMGUIDrawerCache.DrawerId(property, ReferencePickerDrawerCacheIndex);
            if (IMGUIDrawerCache.CachedDrawers.TryGetValue(drawerKey, out PropertyDrawer drawer) &&
                drawer is ReferencePickerAttributeDrawer cachedDrawer)
            {
                cachedDrawer.InHorizontalLayout = inHorizontalLayout;
                return cachedDrawer;
            }

            ReferencePickerAttribute referencePickerAttribute = new ReferencePickerAttribute();
            ReferencePickerAttributeDrawer referencePickerDrawer =
                (ReferencePickerAttributeDrawer)SaintsPropertyDrawer.MakePropertyDrawer(
                    typeof(ReferencePickerAttributeDrawer), fieldInfo, referencePickerAttribute, label);
            referencePickerDrawer.InHorizontalLayout = inHorizontalLayout;
            return (ReferencePickerAttributeDrawer)(IMGUIDrawerCache.CachedDrawers[drawerKey] = referencePickerDrawer);
        }
#endif

        private static SaintsPropertyDrawer GetAndCacheEnumDropdownDrawer(SerializedProperty property, Type rawType,
            FieldInfo fieldInfo, string label, bool inHorizontalLayout)
        {
            IMGUIDrawerCache.DrawerId drawerKey = new IMGUIDrawerCache.DrawerId(property, EnumDropdownDrawerCacheIndex);
            if (IMGUIDrawerCache.CachedDrawers.TryGetValue(drawerKey, out PropertyDrawer drawer))
            {
                SaintsPropertyDrawer cachedDrawer = (SaintsPropertyDrawer)drawer;
                cachedDrawer.InHorizontalLayout = inHorizontalLayout;
                return cachedDrawer;
            }

            bool hasFlags = rawType?.GetCustomAttributes(typeof(FlagsAttribute), true).Length > 0;
            Attribute dropdownAttribute = hasFlags
                ? new FlagsTreeDropdownAttribute()
                : new DropdownAttribute();
            Type drawerType = hasFlags
                ? typeof(FlagsTreeDropdownAttributeDrawer)
                : typeof(TreeDropdownAttributeDrawer);
            SaintsPropertyDrawer enumDropdownDrawer =
                (SaintsPropertyDrawer)SaintsPropertyDrawer.MakePropertyDrawer(
                    drawerType, fieldInfo, dropdownAttribute, label);
            enumDropdownDrawer.OverrideAttributes = new[] { dropdownAttribute };
            enumDropdownDrawer.InHorizontalLayout = inHorizontalLayout;
            return (SaintsPropertyDrawer)(IMGUIDrawerCache.CachedDrawers[drawerKey] = enumDropdownDrawer);
        }

        public static float GetPropertyHeight(
            PropertyDrawer imguiDrawer,
            GUIContent useGUIContent,
            SerializedProperty property,
            IReadOnlyList<Attribute> allAttributes,
            Type rawType,
            FieldInfo fieldInfo,
            bool inHorizontalLayout)
        {
            if (imguiDrawer != null)
            {
                return imguiDrawer.GetPropertyHeight(property, useGUIContent);
            }

            return GetPropertyHeightRawFallback(
                property, allAttributes, rawType, useGUIContent, fieldInfo, inHorizontalLayout
            );
        }

        public static float GetPropertyHeightRawFallback(
            SerializedProperty property,
            IReadOnlyList<Attribute> allAttributes,
            Type rawType,
            GUIContent label,
            FieldInfo fieldInfo,
            bool inHorizontalLayout)
        {
            SerializedPropertyType propertyType = property.propertyType;

            switch (propertyType)
            {
                case SerializedPropertyType.Generic:
                case SerializedPropertyType.ManagedReference:
                {
                    // Debug.Log($"generic/managed process {property.propertyPath}/{property.isArray} allAttributes={string.Join(", ", allAttributes)}");
                    if (property.isArray)
                    {
                        return IMGUIList.GetHeight(property, allAttributes, rawType, label, fieldInfo,
                            inHorizontalLayout, false);
                    }

#if UNITY_2021_3_OR_NEWER
                    if (NeedReferencePickerDrawer(property, allAttributes))
                    {
                        float fullWidth = EditorGUIUtility.currentViewWidth - EditorGUI.indentLevel * 15;
                        return GetAndCacheReferencePickerDrawer(property, fieldInfo, label.text, inHorizontalLayout)
                            .GetReferencePickerHeight(property, label, fullWidth, allAttributes, fieldInfo);
                    }
#endif

                    return GetAndCacheSaintsRowDrawer(property, fieldInfo, label.text, inHorizontalLayout)
                        .GetRowFieldHeight(property, label, fieldInfo);
                }
                    // throw new ArgumentOutOfRangeException(nameof(propertyType), propertyType, "Should Not Put it here");
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.Boolean:
                case SerializedPropertyType.Float:
                case SerializedPropertyType.String:
                case SerializedPropertyType.Color:
                case SerializedPropertyType.ObjectReference:
                case SerializedPropertyType.LayerMask:
                case SerializedPropertyType.ArraySize:
                case SerializedPropertyType.Character:
                case SerializedPropertyType.AnimationCurve:
                case SerializedPropertyType.Gradient:
                case SerializedPropertyType.ExposedReference:
                case SerializedPropertyType.FixedBufferSize:
                case SerializedPropertyType.Hash128:
                    return GetSingleLineHeight(inHorizontalLayout);
                case SerializedPropertyType.Enum:
                {
                    float dropdownHeight = GetAndCacheEnumDropdownDrawer(property, rawType, fieldInfo, label.text,
                        inHorizontalLayout).GetPropertyHeight(property, label);
                    return Mathf.Max(GetSingleLineHeight(inHorizontalLayout), dropdownHeight);
                }
                case SerializedPropertyType.Vector2:
                {
                    return IMGUIVector2.GetHeight(inHorizontalLayout);
                }
                case SerializedPropertyType.Vector3:
                case SerializedPropertyType.Vector4:
                case SerializedPropertyType.Quaternion:
                case SerializedPropertyType.Vector2Int:
                case SerializedPropertyType.Vector3Int:
                    return GetResponsiveMultiLineHeight(inHorizontalLayout, 2);
                case SerializedPropertyType.Rect:
                    return IMGUIRect.GetHeight();
                case SerializedPropertyType.Bounds:
                    return IMGUIBounds.GetHeight();
                case SerializedPropertyType.RectInt:
                    return IMGUIRectInt.GetHeight();
                case SerializedPropertyType.BoundsInt:
                    return IMGUIBoundsInt.GetHeight();
                default:
                    return 0f;
            }
        }

        public static void OnGUI(
            PropertyDrawer imguiDrawer,
            Rect position,
            SerializedProperty property,
            IReadOnlyList<Attribute> allAttributes,
            Type rawType,
            GUIContent label,
            IEnumerable<RichTextDrawer.RichTextChunk> richTextChunks,
            FieldInfo fieldInfo,
            bool inHorizontalLayout,
            bool labelGrayColor)
        {
            if (imguiDrawer == null)
            {
                OnGUIRawFallback(
                    position,
                    property,
                    allAttributes,
                    rawType,
                    label,
                    fieldInfo,
                    richTextChunks,
                    inHorizontalLayout,
                    labelGrayColor
                );
            }
            else
            {
                bool isSaintsDrawer = false;
                if (imguiDrawer is SaintsPropertyDrawer saintsDrawer)
                {
                    // ReSharper disable once PossibleMultipleEnumeration
                    saintsDrawer.overrideRichTextChunks = richTextChunks;
                    isSaintsDrawer = true;
                }

                imguiDrawer.OnGUI(position, property, label);

                if (richTextChunks != null && !isSaintsDrawer)
                {
                    Rect labelRect = new Rect(position)
                    {
                        y = position.y + 1,
                        height = EditorGUIUtility.singleLineHeight,
                        width = position.width - EditorGUI.PrefixLabel(position, new GUIContent(" ")).width,
                    };

                    DrawRichText(
                        labelRect,
                        // ReSharper disable once PossibleMultipleEnumeration
                        richTextChunks);
                }
            }

        }

        public static void OnGUIRawFallback(Rect position,
            SerializedProperty property,
            IReadOnlyList<Attribute> allAttributes,
            Type rawType,
            GUIContent label,
            FieldInfo fieldInfo,
            IEnumerable<RichTextDrawer.RichTextChunk> richTextChunks,
            bool inHorizontalLayout,
            bool labelGrayColor)
        {
            SerializedPropertyType propertyType = property.propertyType;

            switch (propertyType)
            {
                case SerializedPropertyType.Generic:
                case SerializedPropertyType.ManagedReference:
                {
                    if (property.isArray)
                    {
                        IMGUIList.DrawField(
                            position,
                            property,
                            allAttributes,
                            rawType,
                            label,
                            richTextChunks,
                            fieldInfo,
                            inHorizontalLayout,
                            labelGrayColor);
                        return;
                    }
#if UNITY_2021_3_OR_NEWER
                    if (NeedReferencePickerDrawer(property, allAttributes))
                    {
                        ReferencePickerAttributeDrawer referencePickerDrawer =
                            GetAndCacheReferencePickerDrawer(property, fieldInfo, label.text, inHorizontalLayout);
                        referencePickerDrawer.overrideRichTextChunks = richTextChunks;
                        referencePickerDrawer.DrawReferencePicker(position, property, label, allAttributes, fieldInfo);
                        return;
                    }
#endif
                    GetAndCacheSaintsRowDrawer(property, fieldInfo, label.text, inHorizontalLayout)
                        .DrawRowField(position, property, label, fieldInfo);
                    DrawRichText(new Rect(position)
                    {
                        height = EditorGUIUtility.singleLineHeight,
                    }, richTextChunks);
                    return;
                }
                case SerializedPropertyType.Vector2:
                {
                    using EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope();

                    Vector2 result = IMGUIVector2.DrawField(position, label, property.vector2Value,
                        inHorizontalLayout, labelGrayColor);
                    DrawResponsiveRichText(position, label, richTextChunks, inHorizontalLayout);

                    if (changed.changed)
                    {
                        property.vector2Value = result;
                        ApplyModifiedPropertiesAndNotify(property);
                    }

                    return;
                }
                case SerializedPropertyType.Integer:
                {
                    using EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope();
                    long result = IMGUIInteger.DrawLongField(position, label, property.longValue, inHorizontalLayout, labelGrayColor);
                    DrawSingleLineRichText(position, label, richTextChunks, inHorizontalLayout);

                    if (changed.changed)
                    {
                        if (rawType == typeof(byte))
                        {
                            property.longValue = Math.Clamp(result, byte.MinValue, byte.MaxValue);
                        }
                        else if (rawType == typeof(sbyte))
                        {
                            property.longValue = Math.Clamp(result, sbyte.MinValue, sbyte.MaxValue);
                        }
                        else if (rawType == typeof(short))
                        {
                            property.longValue = Math.Clamp(result, short.MinValue, short.MaxValue);
                        }
                        else if (rawType == typeof(ushort))
                        {
                            property.longValue = Math.Clamp(result, ushort.MinValue, ushort.MaxValue);
                        }
                        else if (rawType == typeof(uint))
                        {
                            property.longValue = Math.Max(0, result);
                        }
                        else if (rawType == typeof(ulong))
                        {
                            property.longValue = Math.Max(0, result);
                        }
                        else
                        {
                            property.longValue = result;
                        }
                        ApplyModifiedPropertiesAndNotify(property);
                    }

                    return;
                }
                case SerializedPropertyType.Boolean:
                {
                    using EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope();
                    bool result = IMGUIBool.DrawField(position, label, property.boolValue, inHorizontalLayout, labelGrayColor);
                    DrawSingleLineRichText(position, label, richTextChunks, inHorizontalLayout);

                    if (changed.changed)
                    {
                        property.boolValue = result;
                        ApplyModifiedPropertiesAndNotify(property);
                    }

                    return;
                }
                case SerializedPropertyType.Float:
                {
                    using EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope();
                    double result = rawType == typeof(double)
                        ? IMGUIFloat.DrawDoubleField(position, label, property.doubleValue, inHorizontalLayout, labelGrayColor)
                        : IMGUIFloat.DrawFloatField(position, label, property.floatValue, inHorizontalLayout, labelGrayColor);
                    DrawSingleLineRichText(position, label, richTextChunks, inHorizontalLayout);

                    if (changed.changed)
                    {
                        if (rawType == typeof(double))
                        {
                            property.doubleValue = result;
                        }
                        else
                        {
                            property.floatValue = (float)result;
                        }
                        ApplyModifiedPropertiesAndNotify(property);
                    }

                    return;
                }
                case SerializedPropertyType.String:
                {
                    using EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope();
                    string result = IMGUIText.DrawField(position, label, richTextChunks, property.stringValue, inHorizontalLayout, labelGrayColor);
                    if (inHorizontalLayout)
                    {
                        DrawSingleLineRichText(position, label, richTextChunks, inHorizontalLayout);
                    }

                    if (changed.changed)
                    {
                        property.stringValue = result;
                        ApplyModifiedPropertiesAndNotify(property);
                    }

                    return;
                }
                case SerializedPropertyType.Color:
                {
                    using EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope();
                    Color result = IMGUIColor.DrawField(position, label, property.colorValue, inHorizontalLayout, labelGrayColor);
                    DrawSingleLineRichText(position, label, richTextChunks, inHorizontalLayout);

                    if (changed.changed)
                    {
                        property.colorValue = result;
                        ApplyModifiedPropertiesAndNotify(property);
                    }

                    return;
                }
                case SerializedPropertyType.ObjectReference:
                {
                    Type objectType = typeof(UnityEngine.Object).IsAssignableFrom(rawType)
                        ? rawType
                        : typeof(UnityEngine.Object);
                    using EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope();
                    UnityEngine.Object result = IMGUIObject.DrawField(position, label, property.objectReferenceValue, objectType, true, inHorizontalLayout, labelGrayColor);
                    DrawSingleLineRichText(position, label, richTextChunks, inHorizontalLayout);

                    if (changed.changed)
                    {
                        property.objectReferenceValue = result;
                        ApplyModifiedPropertiesAndNotify(property);
                    }

                    return;
                }
                case SerializedPropertyType.LayerMask:
                {
                    using EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope();
                    Rect contentRect = GetContentRect(position);
                    int result;
                    if (!inHorizontalLayout)
                    {
                        using(new LabelColorScoop(labelGrayColor))
                        {
                            result = EditorGUI.LayerField(contentRect, label, property.intValue);
                        }
                    }
                    else
                    {
                        Rect labelRect = new Rect(contentRect)
                        {
                            height = EditorGUIUtility.singleLineHeight,
                        };

                        Rect fieldRect = new Rect(contentRect)
                        {
                            y = contentRect.y + EditorGUIUtility.singleLineHeight,
                            height = EditorGUIUtility.singleLineHeight,
                        };

                        using(new LabelColorScoop(labelGrayColor))
                        {
                            EditorGUI.HandlePrefixLabel(contentRect, labelRect, label, 0);
                        }
                        result = EditorGUI.LayerField(fieldRect, property.intValue);
                    }
                    DrawSingleLineRichText(position, label, richTextChunks, inHorizontalLayout);

                    if (changed.changed)
                    {
                        property.intValue = result;
                        ApplyModifiedPropertiesAndNotify(property);
                    }

                    return;
                }
                case SerializedPropertyType.Enum:
                {
                    SaintsPropertyDrawer enumDropdownDrawer =
                        GetAndCacheEnumDropdownDrawer(property, rawType, fieldInfo, label.text, inHorizontalLayout);
                    enumDropdownDrawer.overrideRichTextChunks = richTextChunks;
                    using (new LabelColorScoop(labelGrayColor))
                    {
                        enumDropdownDrawer.OnGUI(position, property, label);
                    }

                    return;
                }
                case SerializedPropertyType.Vector3:
                {
                    using EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope();
                    Vector3 result;
                    using(new InHorizontalLayoutScoop(inHorizontalLayout, position))
                    using(new LabelColorScoop(labelGrayColor))
                    {
                        result = EditorGUI.Vector3Field(position, label, property.vector3Value);
                    }
                    DrawResponsiveRichText(position, label, richTextChunks, inHorizontalLayout);

                    if (changed.changed)
                    {
                        property.vector3Value = result;
                        ApplyModifiedPropertiesAndNotify(property);
                    }

                    return;
                }
                case SerializedPropertyType.Vector4:
                {
                    using EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope();
                    Vector4 result;
                    using(new InHorizontalLayoutScoop(inHorizontalLayout, position))
                    using(new LabelColorScoop(labelGrayColor))
                    {
                        result = EditorGUI.Vector4Field(position, label, property.vector4Value);
                    }
                    DrawResponsiveRichText(position, label, richTextChunks, inHorizontalLayout);

                    if (changed.changed)
                    {
                        property.vector4Value = result;
                        ApplyModifiedPropertiesAndNotify(property);
                    }

                    return;
                }
                case SerializedPropertyType.Rect:
                {
                    using EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope();
                    Rect result = IMGUIRect.DrawField(position, label, property.rectValue, inHorizontalLayout, labelGrayColor);
                    DrawResponsiveRichText(position, label, richTextChunks, inHorizontalLayout);

                    if (changed.changed)
                    {
                        property.rectValue = result;
                        ApplyModifiedPropertiesAndNotify(property);
                    }

                    return;
                }
                case SerializedPropertyType.ArraySize:
                {
                    using EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope();
                    int result = IMGUIInteger.DrawIntField(position, label, property.intValue, inHorizontalLayout, labelGrayColor);
                    DrawSingleLineRichText(position, label, richTextChunks, inHorizontalLayout);

                    if (changed.changed)
                    {
                        property.intValue = Math.Max(0, result);
                        ApplyModifiedPropertiesAndNotify(property);
                    }

                    return;
                }
                case SerializedPropertyType.Character:
                {
                    using EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope();
                    string current = property.intValue == 0 ? "" : ((char)property.intValue).ToString();
                    string result = IMGUIText.DrawField(position, label, richTextChunks, current, inHorizontalLayout, labelGrayColor);
                    if (inHorizontalLayout)
                    {
                        DrawSingleLineRichText(position, label, richTextChunks, inHorizontalLayout);
                    }

                    if (changed.changed)
                    {
                        property.intValue = string.IsNullOrEmpty(result) ? 0 : result[0];
                        ApplyModifiedPropertiesAndNotify(property);
                    }

                    return;
                }
                case SerializedPropertyType.AnimationCurve:
                {
                    using EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope();
                    AnimationCurve result = IMGUIAnimationCurve.DrawField(position, label, property.animationCurveValue, inHorizontalLayout, labelGrayColor);
                    DrawSingleLineRichText(position, label, richTextChunks, inHorizontalLayout);

                    if (changed.changed)
                    {
                        property.animationCurveValue = result;
                        ApplyModifiedPropertiesAndNotify(property);
                    }

                    return;
                }
                case SerializedPropertyType.Bounds:
                {
                    using EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope();
                    Bounds result = IMGUIBounds.DrawField(position, label, property.boundsValue, inHorizontalLayout, labelGrayColor);
                    DrawResponsiveRichText(position, label, richTextChunks, inHorizontalLayout);

                    if (changed.changed)
                    {
                        property.boundsValue = result;
                        ApplyModifiedPropertiesAndNotify(property);
                    }

                    return;
                }
                case SerializedPropertyType.Gradient:
                {
                    using EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope();
                    Gradient result = IMGUIGradient.DrawField(position, label, property.gradientValue, inHorizontalLayout, labelGrayColor);
                    DrawSingleLineRichText(position, label, richTextChunks, inHorizontalLayout);

                    if (changed.changed)
                    {
                        property.gradientValue = result;
                        ApplyModifiedPropertiesAndNotify(property);
                    }

                    return;
                }
                case SerializedPropertyType.Quaternion:
                {
                    using EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope();
                    Quaternion current = property.quaternionValue;
                    Vector4 result;
                    using(new InHorizontalLayoutScoop(inHorizontalLayout, position))
                    using(new LabelColorScoop(labelGrayColor))
                    {
                        result = EditorGUI.Vector4Field(position, label, new Vector4(current.x, current.y, current.z, current.w));
                    }
                    DrawResponsiveRichText(position, label, richTextChunks, inHorizontalLayout);

                    if (changed.changed)
                    {
                        property.quaternionValue = new Quaternion(result.x, result.y, result.z, result.w);
                        ApplyModifiedPropertiesAndNotify(property);
                    }

                    return;
                }
                case SerializedPropertyType.ExposedReference:
                {
                    Type objectType = typeof(UnityEngine.Object).IsAssignableFrom(rawType)
                        ? rawType
                        : typeof(UnityEngine.Object);
                    using EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope();
                    UnityEngine.Object result = IMGUIObject.DrawField(position, label, property.exposedReferenceValue, objectType, true, inHorizontalLayout, labelGrayColor);
                    DrawSingleLineRichText(position, label, richTextChunks, inHorizontalLayout);

                    if (changed.changed)
                    {
                        property.exposedReferenceValue = result;
                        ApplyModifiedPropertiesAndNotify(property);
                    }

                    return;
                }
                case SerializedPropertyType.FixedBufferSize:
                {
                    using (new EditorGUI.DisabledScope(true))
                    {
                        IMGUIInteger.DrawIntField(position, label, property.fixedBufferSize, inHorizontalLayout, labelGrayColor);
                    }
                    DrawSingleLineRichText(position, label, richTextChunks, inHorizontalLayout);

                    return;
                }
                case SerializedPropertyType.Vector2Int:
                {
                    using EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope();
                    Vector2Int result;
                    using(new InHorizontalLayoutScoop(inHorizontalLayout, position))
                    using(new LabelColorScoop(labelGrayColor))
                    {
                        result = EditorGUI.Vector2IntField(position, label, property.vector2IntValue);
                    }
                    DrawResponsiveRichText(position, label, richTextChunks, inHorizontalLayout);

                    if (changed.changed)
                    {
                        property.vector2IntValue = result;
                        ApplyModifiedPropertiesAndNotify(property);
                    }

                    return;
                }
                case SerializedPropertyType.Vector3Int:
                {
                    using EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope();
                    Vector3Int result;
                    using(new InHorizontalLayoutScoop(inHorizontalLayout, position))
                    using(new LabelColorScoop(labelGrayColor))
                    {
                        result = EditorGUI.Vector3IntField(position, label, property.vector3IntValue);
                    }
                    DrawResponsiveRichText(position, label, richTextChunks, inHorizontalLayout);

                    if (changed.changed)
                    {
                        property.vector3IntValue = result;
                        ApplyModifiedPropertiesAndNotify(property);
                    }

                    return;
                }
                case SerializedPropertyType.RectInt:
                {
                    using EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope();
                    RectInt result = IMGUIRectInt.DrawField(position, label, property.rectIntValue, inHorizontalLayout, labelGrayColor);
                    DrawResponsiveRichText(position, label, richTextChunks, inHorizontalLayout);

                    if (changed.changed)
                    {
                        property.rectIntValue = result;
                        ApplyModifiedPropertiesAndNotify(property);
                    }

                    return;
                }
                case SerializedPropertyType.BoundsInt:
                {
                    using EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope();
                    BoundsInt result = IMGUIBoundsInt.DrawField(position, label, property.boundsIntValue, inHorizontalLayout, labelGrayColor);
                    DrawResponsiveRichText(position, label, richTextChunks, inHorizontalLayout);

                    if (changed.changed)
                    {
                        property.boundsIntValue = result;
                        ApplyModifiedPropertiesAndNotify(property);
                    }

                    return;
                }
                case SerializedPropertyType.Hash128:
                {
                    using EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope();
                    string result = IMGUIHash128.DrawField(position, label, property.hash128Value, inHorizontalLayout, labelGrayColor);
                    DrawSingleLineRichText(position, label, richTextChunks, inHorizontalLayout);

                    if (changed.changed)
                    {
                        try
                        {
                            property.hash128Value = Hash128.Parse(result);
                            ApplyModifiedPropertiesAndNotify(property);
                        }
                        catch (FormatException)
                        {
                        }
                    }

                    return;
                }
                default:
                    return;
            }
        }

        private static void ApplyModifiedPropertiesAndNotify(SerializedProperty property)
        {
            property.serializedObject.ApplyModifiedProperties();
            SaintsEditorApplicationChanged.OnSaintsFieldChangedEvent.Invoke();
        }
    }

    internal sealed class UnityPropertyFieldProxyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(position, property, label, true);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.SaintsRowDrawer;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Utils.IMGUIPlainDrawer
{
    public static class IMGUIRawDraw
    {
        public static readonly Color LabelGrayColor = EColor.EditorSeparator.GetColor();

        public static bool UseWideMode() => EditorGUIUtility.wideMode || EditorGUIUtility.currentViewWidth > 330f;

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

            IMGUIDrawerCache.DrawerId drawerKey = new IMGUIDrawerCache.DrawerId(property, 0);
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
            IMGUIDrawerCache.DrawerId drawerKey = new IMGUIDrawerCache.DrawerId(property, 1);
            if (IMGUIDrawerCache.CachedSaintsRowDrawers.TryGetValue(drawerKey, out SaintsRowAttributeDrawer saintsRowDrawer))
            {
                return saintsRowDrawer;
            }

            saintsRowDrawer = (SaintsRowAttributeDrawer)SaintsPropertyDrawer.MakePropertyDrawer(typeof(SaintsRowAttributeDrawer), fieldInfo,
                new SaintsRowAttribute(), label);
            saintsRowDrawer.InHorizontalLayout = inHorizontalLayout;
            return IMGUIDrawerCache.CachedSaintsRowDrawers[drawerKey] = saintsRowDrawer;
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
                case SerializedPropertyType.Enum:
                case SerializedPropertyType.ArraySize:
                case SerializedPropertyType.Character:
                case SerializedPropertyType.AnimationCurve:
                case SerializedPropertyType.Gradient:
                case SerializedPropertyType.ExposedReference:
                case SerializedPropertyType.FixedBufferSize:
                case SerializedPropertyType.Hash128:
                    return IMGUIShared.GetSingleLineHeight(inHorizontalLayout);
                case SerializedPropertyType.Vector2:
                {
                    return IMGUIVector2.GetHeight(inHorizontalLayout);
                }
                case SerializedPropertyType.Vector3:
                case SerializedPropertyType.Vector4:
                case SerializedPropertyType.Quaternion:
                case SerializedPropertyType.Vector2Int:
                case SerializedPropertyType.Vector3Int:
                    return IMGUIShared.GetResponsiveMultiLineHeight(inHorizontalLayout, 2);
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
            FieldInfo fieldInfo,
            bool inHorizontalLayout,
            bool labelGrayColor)
        {
            if (imguiDrawer == null)
            {
                OnGUIRawFallback(
                    position,
                    property,
                    allAttributes, rawType, label, fieldInfo, inHorizontalLayout, labelGrayColor
                );
            }
            else
            {
                imguiDrawer.OnGUI(position, property, label);
            }

        }

        public static void OnGUIRawFallback(
            Rect position,
            SerializedProperty property,
            IReadOnlyList<Attribute> allAttributes,
            Type rawType,
            GUIContent label,
            FieldInfo fieldInfo,
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
                        IMGUIList.DrawField(position, property, allAttributes, rawType, label, fieldInfo,
                            inHorizontalLayout, labelGrayColor);
                        return;
                    }
                    GetAndCacheSaintsRowDrawer(property, fieldInfo, label.text, inHorizontalLayout)
                        .DrawRowField(position, property, label, fieldInfo);
                    return;
                }
                case SerializedPropertyType.Vector2:
                {
                    using EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope();

                    Vector2 result = IMGUIVector2.DrawField(position, label, property.vector2Value,
                        inHorizontalLayout, labelGrayColor);

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
                    string result = IMGUIText.DrawField(position, label, property.stringValue, inHorizontalLayout, labelGrayColor);

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
                    int result = IMGUIShared.DrawStackedField(position, label, inHorizontalLayout, labelGrayColor,
                        (rect, content) => EditorGUI.LayerField(rect, content, property.intValue),
                        rect => EditorGUI.LayerField(rect, property.intValue));

                    if (changed.changed)
                    {
                        property.intValue = result;
                        ApplyModifiedPropertiesAndNotify(property);
                    }

                    return;
                }
                case SerializedPropertyType.Enum:
                {
                    GUIContent[] enumContents = property.enumDisplayNames.Select(each => new GUIContent(each)).ToArray();
                    using EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope();
                    int result = IMGUIEnum.DrawField(position, label, property.enumValueIndex, enumContents, inHorizontalLayout, labelGrayColor);

                    if (changed.changed)
                    {
                        property.enumValueIndex = result;
                        ApplyModifiedPropertiesAndNotify(property);
                    }

                    return;
                }
                case SerializedPropertyType.Vector3:
                {
                    using EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope();
                    Vector3 result = IMGUIShared.WithLabelColor(labelGrayColor, () =>
                    {
                        bool oldWideMode = EditorGUIUtility.wideMode;
                        float oldLabelWidth = EditorGUIUtility.labelWidth;
                        try
                        {
                            if (inHorizontalLayout)
                            {
                                EditorGUIUtility.wideMode = false;
                                EditorGUIUtility.labelWidth = position.width;
                            }

                            return EditorGUI.Vector3Field(position, label, property.vector3Value);
                        }
                        finally
                        {
                            EditorGUIUtility.wideMode = oldWideMode;
                            EditorGUIUtility.labelWidth = oldLabelWidth;
                        }
                    });

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
                    Vector4 result = IMGUIShared.WithLabelColor(labelGrayColor, () =>
                    {
                        bool oldWideMode = EditorGUIUtility.wideMode;
                        float oldLabelWidth = EditorGUIUtility.labelWidth;
                        try
                        {
                            if (inHorizontalLayout)
                            {
                                EditorGUIUtility.wideMode = false;
                                EditorGUIUtility.labelWidth = position.width;
                            }

                            return EditorGUI.Vector4Field(position, label, property.vector4Value);
                        }
                        finally
                        {
                            EditorGUIUtility.wideMode = oldWideMode;
                            EditorGUIUtility.labelWidth = oldLabelWidth;
                        }
                    });

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
                    string result = IMGUIText.DrawField(position, label, current, inHorizontalLayout, labelGrayColor);

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
                    Vector4 result = IMGUIShared.WithLabelColor(labelGrayColor, () =>
                    {
                        bool oldWideMode = EditorGUIUtility.wideMode;
                        float oldLabelWidth = EditorGUIUtility.labelWidth;
                        try
                        {
                            if (inHorizontalLayout)
                            {
                                EditorGUIUtility.wideMode = false;
                                EditorGUIUtility.labelWidth = position.width;
                            }

                            return EditorGUI.Vector4Field(position, label, new Vector4(current.x, current.y, current.z, current.w));
                        }
                        finally
                        {
                            EditorGUIUtility.wideMode = oldWideMode;
                            EditorGUIUtility.labelWidth = oldLabelWidth;
                        }
                    });

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

                    return;
                }
                case SerializedPropertyType.Vector2Int:
                {
                    using EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope();
                    Vector2Int result = IMGUIShared.WithLabelColor(labelGrayColor, () =>
                    {
                        bool oldWideMode = EditorGUIUtility.wideMode;
                        float oldLabelWidth = EditorGUIUtility.labelWidth;
                        try
                        {
                            if (inHorizontalLayout)
                            {
                                EditorGUIUtility.wideMode = false;
                                EditorGUIUtility.labelWidth = position.width;
                            }

                            return EditorGUI.Vector2IntField(position, label, property.vector2IntValue);
                        }
                        finally
                        {
                            EditorGUIUtility.wideMode = oldWideMode;
                            EditorGUIUtility.labelWidth = oldLabelWidth;
                        }
                    });

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
                    Vector3Int result = IMGUIShared.WithLabelColor(labelGrayColor, () =>
                    {
                        bool oldWideMode = EditorGUIUtility.wideMode;
                        float oldLabelWidth = EditorGUIUtility.labelWidth;
                        try
                        {
                            if (inHorizontalLayout)
                            {
                                EditorGUIUtility.wideMode = false;
                                EditorGUIUtility.labelWidth = position.width;
                            }

                            return EditorGUI.Vector3IntField(position, label, property.vector3IntValue);
                        }
                        finally
                        {
                            EditorGUIUtility.wideMode = oldWideMode;
                            EditorGUIUtility.labelWidth = oldLabelWidth;
                        }
                    });

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

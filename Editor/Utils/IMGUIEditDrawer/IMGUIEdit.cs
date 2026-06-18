using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils.IMGUIPlainDrawer;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Utils.IMGUIEditDrawer
{
    public static partial class IMGUIEdit
    {
        public static void OnGUI(
            Rect position,
            string label, Type valueType, object value, Action<object> beforeSet, Action<object> setterOrNull,
            bool labelGrayColor, bool inHorizontalLayout,
            IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets,
            IRichTextTagProvider richTextTagProvider,
            string foldoutViewKey)
        {
            if (valueType == typeof(Placeholder))
            {
                return;
            }

            if (IMGUIEditAttribute.TryOnGUI(position, label, valueType, value, beforeSet, setterOrNull,
                    labelGrayColor, inHorizontalLayout, allAttributes, targets, richTextTagProvider, foldoutViewKey))
            {
                return;
            }

            if (valueType == typeof(bool) || value is bool)
            {
                IMGUIEditBool.OnGUI(position, label, valueType, (bool)value, beforeSet,
                    setterOrNull, labelGrayColor, inHorizontalLayout, targets, richTextTagProvider, foldoutViewKey);
                return;
            }

            if (valueType == typeof(sbyte) || value is sbyte)
            {
                IMGUIEditSByte.OnGUI(position, label, valueType, (sbyte)value, beforeSet, setterOrNull,
                    labelGrayColor, inHorizontalLayout, targets, richTextTagProvider, foldoutViewKey);
                return;
            }

            if (valueType == typeof(byte) || value is byte)
            {
                IMGUIEditByte.OnGUI(position, label, valueType, (byte)value, beforeSet, setterOrNull, labelGrayColor,
                    inHorizontalLayout, targets, richTextTagProvider, foldoutViewKey);
                return;
            }

            if (valueType == typeof(short) || value is short)
            {
                IMGUIEditShort.OnGUI(position, label, valueType, (short)value, beforeSet, setterOrNull, labelGrayColor,
                    inHorizontalLayout, targets, richTextTagProvider, foldoutViewKey);
                return;
            }

            if (valueType == typeof(ushort) || value is ushort)
            {
                IMGUIEditUShort.OnGUI(position, label, valueType, (ushort)value, beforeSet, setterOrNull,
                    labelGrayColor, inHorizontalLayout, targets, richTextTagProvider, foldoutViewKey);
                return;
            }

            if (valueType == typeof(int) || value is int)
            {
                IMGUIEditInt.OnGUI(position, label, valueType, (int)value, beforeSet, setterOrNull, labelGrayColor,
                    inHorizontalLayout, targets, richTextTagProvider, foldoutViewKey);
                return;
            }

            if (valueType == typeof(uint) || value is uint)
            {
                IMGUIEditUInt.OnGUI(position, label, valueType, (uint)value, beforeSet, setterOrNull, labelGrayColor,
                    inHorizontalLayout, targets, richTextTagProvider, foldoutViewKey);
                return;
            }

            if (valueType == typeof(long) || value is long)
            {
                IMGUIEditLong.OnGUI(position, label, valueType, (long)value, beforeSet, setterOrNull, labelGrayColor,
                    inHorizontalLayout, targets, richTextTagProvider, foldoutViewKey);
                return;
            }

            if (valueType == typeof(ulong) || value is ulong)
            {
                IMGUIEditULong.OnGUI(position, label, valueType, (ulong)value, beforeSet, setterOrNull, labelGrayColor,
                    inHorizontalLayout, targets, richTextTagProvider, foldoutViewKey);
                return;
            }

            if (valueType == typeof(float) || value is float)
            {
                IMGUIEditFloat.OnGUI(position, label, valueType, (float)value, beforeSet, setterOrNull, labelGrayColor,
                    inHorizontalLayout, targets, richTextTagProvider, foldoutViewKey);
                return;
            }

            if (valueType == typeof(double) || value is double)
            {
                IMGUIEditDouble.OnGUI(position, label, valueType, (double)value, beforeSet, setterOrNull,
                    labelGrayColor, inHorizontalLayout, targets, richTextTagProvider, foldoutViewKey);
                return;
            }

            if (valueType == typeof(string) || value is string)
            {
                IMGUIEditString.OnGUI(position, label, valueType, (string)value, beforeSet, setterOrNull,
                    labelGrayColor, inHorizontalLayout, targets, richTextTagProvider, foldoutViewKey);
                return;
            }

            if (valueType == typeof(char) || value is char)
            {
                IMGUIEditChar.OnGUI(position, label, valueType, (char)value, beforeSet, setterOrNull, labelGrayColor,
                    inHorizontalLayout, targets, richTextTagProvider, foldoutViewKey);
                return;
            }

            if (valueType == typeof(Vector2) || value is Vector2)
            {
                IMGUIEditVector2.OnGUI(position, label, valueType, (Vector2)value, beforeSet, setterOrNull,
                    labelGrayColor, inHorizontalLayout, targets, richTextTagProvider, foldoutViewKey);
                return;
            }

            if (valueType == typeof(Vector3) || value is Vector3)
            {
                IMGUIEditVector3.OnGUI(position, label, valueType, (Vector3)value, beforeSet, setterOrNull,
                    labelGrayColor, inHorizontalLayout, targets, richTextTagProvider, foldoutViewKey);
                return;
            }

            if (valueType == typeof(Vector4) || value is Vector4)
            {
                IMGUIEditVector4.OnGUI(position, label, valueType, (Vector4)value, beforeSet, setterOrNull,
                    labelGrayColor, inHorizontalLayout, targets, richTextTagProvider, foldoutViewKey);
                return;
            }

            if (valueType == typeof(Vector2Int) || value is Vector2Int)
            {
                IMGUIEditVector2Int.OnGUI(position, label, valueType, (Vector2Int)value, beforeSet, setterOrNull,
                    labelGrayColor, inHorizontalLayout, targets, richTextTagProvider, foldoutViewKey);
                return;
            }

            if (valueType == typeof(Vector3Int) || value is Vector3Int)
            {
                IMGUIEditVector3Int.OnGUI(position, label, valueType, (Vector3Int)value, beforeSet, setterOrNull,
                    labelGrayColor, inHorizontalLayout, targets, richTextTagProvider, foldoutViewKey);
                return;
            }

            if (valueType == typeof(Quaternion) || value is Quaternion)
            {
                IMGUIEditQuaternion.OnGUI(position, label, valueType, (Quaternion)value, beforeSet, setterOrNull,
                    labelGrayColor, inHorizontalLayout, targets, richTextTagProvider, foldoutViewKey);
                return;
            }

            if (valueType == typeof(Color) || value is Color)
            {
                GUIContent content = new GUIContent(label);
                using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                {
                    Color result = IMGUIColor.DrawField(position, content, (Color)value, inHorizontalLayout,
                        labelGrayColor);
                    if (changed.changed && setterOrNull != null)
                    {
                        beforeSet?.Invoke(value);
                        setterOrNull(result);
                    }
                }
                return;
            }

            if (valueType == typeof(Bounds) || value is Bounds)
            {
                GUIContent content = new GUIContent(label);
                using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                {
                    Bounds result = IMGUIBounds.DrawField(position, content, (Bounds)value, inHorizontalLayout,
                        labelGrayColor);
                    if (changed.changed && setterOrNull != null)
                    {
                        beforeSet?.Invoke(value);
                        setterOrNull(result);
                    }
                }
                return;
            }

            if (valueType == typeof(Rect) || value is Rect)
            {
                GUIContent content = new GUIContent(label);
                using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                {
                    Rect result = IMGUIRect.DrawField(position, content, (Rect)value, inHorizontalLayout,
                        labelGrayColor);
                    if (changed.changed && setterOrNull != null)
                    {
                        beforeSet?.Invoke(value);
                        setterOrNull(result);
                    }
                }
                return;
            }

            if (valueType == typeof(RectInt) || value is RectInt)
            {
                GUIContent content = new GUIContent(label);
                using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                {
                    RectInt result = IMGUIRectInt.DrawField(position, content, (RectInt)value, inHorizontalLayout,
                        labelGrayColor);
                    if (changed.changed && setterOrNull != null)
                    {
                        beforeSet?.Invoke(value);
                        setterOrNull(result);
                    }
                }
                return;
            }

            if (valueType == typeof(BoundsInt) || value is BoundsInt)
            {
                GUIContent content = new GUIContent(label);
                using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                {
                    BoundsInt result = IMGUIBoundsInt.DrawField(position, content, (BoundsInt)value,
                        inHorizontalLayout, labelGrayColor);
                    if (changed.changed && setterOrNull != null)
                    {
                        beforeSet?.Invoke(value);
                        setterOrNull(result);
                    }
                }
                return;
            }

            if (valueType?.BaseType == typeof(Enum) || value is Enum)
            {
                IMGUIEditEnum.OnGUI(position, label, valueType, value, beforeSet, setterOrNull, labelGrayColor,
                    inHorizontalLayout, targets, richTextTagProvider, foldoutViewKey);
                return;
            }

            if (valueType != null && typeof(UnityEngine.Object).IsAssignableFrom(valueType) ||
                value is UnityEngine.Object)
            {
                IMGUIEditObject.OnGUI(position, label, valueType, (UnityEngine.Object)value, beforeSet,
                    setterOrNull, labelGrayColor, inHorizontalLayout, targets, richTextTagProvider, foldoutViewKey);
                return;
            }

            if (valueType != null && typeof(AnimationCurve).IsAssignableFrom(valueType) || value is AnimationCurve)
            {
                IMGUIEditAnimationCurve.OnGUI(position, label, valueType, (AnimationCurve)value, beforeSet,
                    setterOrNull, labelGrayColor, inHorizontalLayout, targets, richTextTagProvider, foldoutViewKey);
                return;
            }

            if (valueType == typeof(Hash128) || value is Hash128)
            {
                IMGUIEditHash128.OnGUI(position, label, valueType, (Hash128)value, beforeSet, setterOrNull,
                    labelGrayColor, inHorizontalLayout, targets, richTextTagProvider, foldoutViewKey);
                return;
            }

            if (valueType != null && typeof(Gradient).IsAssignableFrom(valueType) || value is Gradient)
            {
                IMGUIEditGradient.OnGUI(position, label, valueType, (Gradient)value, beforeSet, setterOrNull,
                    labelGrayColor, inHorizontalLayout, targets, richTextTagProvider, foldoutViewKey);
                return;
            }

            if (valueType == typeof(DateTime) || value is DateTime)
            {
                IMGUIEditDateTime.OnGUI(position, label, valueType, (DateTime)value, beforeSet, setterOrNull,
                    labelGrayColor, inHorizontalLayout, targets, richTextTagProvider, foldoutViewKey);
                return;
            }

            if (valueType == typeof(TimeSpan) || value is TimeSpan)
            {
                IMGUIEditTimeSpan.OnGUI(position, label, valueType, (TimeSpan)value, beforeSet, setterOrNull,
                    labelGrayColor, inHorizontalLayout, targets, richTextTagProvider, foldoutViewKey);
                return;
            }

            if (valueType == typeof(LayerMask) || value is LayerMask)
            {
                GUIContent content = new GUIContent(label);
                using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                {
                    LayerMask result = IMGUILayerMask.DrawField(position, content, (LayerMask)value,
                        inHorizontalLayout, labelGrayColor);
                    if (changed.changed && setterOrNull != null)
                    {
                        beforeSet?.Invoke(value);
                        setterOrNull(result);
                    }
                }
                return;
            }

            if (valueType == typeof(Guid) || value is Guid)
            {
                IMGUIEditGuid.OnGUI(position, label, valueType, (Guid)value, beforeSet, setterOrNull, labelGrayColor,
                    inHorizontalLayout, targets, richTextTagProvider, foldoutViewKey);
                return;
            }

            if (valueType == typeof(decimal) || value is decimal)
            {
                IMGUIEditDecimal.OnGUI(position, label, valueType, (decimal)value, beforeSet, setterOrNull,
                    labelGrayColor, inHorizontalLayout, targets, richTextTagProvider, foldoutViewKey);
                return;
            }

            if (valueType == typeof(Type) || value is Type)
            {
                IMGUIEditType.OnGUI(position, label, valueType, (Type)value, beforeSet, setterOrNull, labelGrayColor,
                    inHorizontalLayout, targets, richTextTagProvider, foldoutViewKey);
                return;
            }

            if (valueType?.BaseType == typeof(TypeInfo) || value is TypeInfo)
            {
                IMGUIEditType.OnGUI(position, label, valueType, (TypeInfo)value, beforeSet, setterOrNull,
                    labelGrayColor, inHorizontalLayout, targets, richTextTagProvider, foldoutViewKey);
                return;
            }

            if (IMGUIEditDictionary.TryOnGUI(position, label, valueType, value, beforeSet, setterOrNull,
                    labelGrayColor, inHorizontalLayout, allAttributes, targets, richTextTagProvider, foldoutViewKey))
            {
                return;
            }

            if (IMGUIEditList.TryOnGUI(position, label, valueType, value, beforeSet, setterOrNull,
                    labelGrayColor, inHorizontalLayout, allAttributes, targets, richTextTagProvider, foldoutViewKey))
            {
                return;
            }

            IMGUIGeneralType.OnGUI(position, label, valueType, value, beforeSet, setterOrNull, labelGrayColor,
                inHorizontalLayout, allAttributes, targets, richTextTagProvider, foldoutViewKey);
        }
    }
}

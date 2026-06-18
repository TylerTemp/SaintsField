using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils.IMGUIPlainDrawer;
using UnityEngine;

namespace SaintsField.Editor.Utils.IMGUIEditDrawer
{
    public static partial class IMGUIEdit
    {
        public static readonly Dictionary<string, bool> ViewKey = new Dictionary<string, bool>();

        public static float GetPropertyHeight(
            string label, Type valueType, object value, Action<object> beforeSet, Action<object> setterOrNull,
            bool labelGrayColor, bool inHorizontalLayout,
            IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets, IRichTextTagProvider richTextTagProvider,
            string foldoutViewKey)
        {
            if (valueType == typeof(Placeholder))
            {
                return 0f;
            }

            (bool hasAttributeHeight, float attributeHeight) = IMGUIEditAttribute.GetPropertyHeight(label, valueType,
                value, beforeSet, setterOrNull, labelGrayColor, inHorizontalLayout, allAttributes, targets,
                richTextTagProvider, foldoutViewKey);
            if (hasAttributeHeight)
            {
                return attributeHeight;
            }

            if (valueType == typeof(bool) || value is bool)
            {
                return IMGUIEditBool.GetPropertyHeight(label, valueType, (bool)value, beforeSet, setterOrNull,
                    labelGrayColor, inHorizontalLayout, targets, richTextTagProvider, foldoutViewKey);
            }

            if (valueType == typeof(sbyte) || value is sbyte)
            {
                return IMGUIEditSByte.GetPropertyHeight(label, valueType, (sbyte)value, beforeSet, setterOrNull,
                    labelGrayColor, inHorizontalLayout, targets, richTextTagProvider, foldoutViewKey);
            }

            if (valueType == typeof(byte) || value is byte)
            {
                return IMGUIEditByte.GetPropertyHeight(label, valueType, (byte)value, beforeSet, setterOrNull,
                    labelGrayColor, inHorizontalLayout, targets, richTextTagProvider, foldoutViewKey);
            }

            if (valueType == typeof(short) || value is short)
            {
                return IMGUIEditShort.GetPropertyHeight(label, valueType, (short)value, beforeSet, setterOrNull,
                    labelGrayColor, inHorizontalLayout, targets, richTextTagProvider, foldoutViewKey);
            }

            if (valueType == typeof(ushort) || value is ushort)
            {
                return IMGUIEditUShort.GetPropertyHeight(label, valueType, (ushort)value, beforeSet, setterOrNull,
                    labelGrayColor, inHorizontalLayout, targets, richTextTagProvider, foldoutViewKey);
            }

            if (valueType == typeof(int) || value is int)
            {
                return IMGUIEditInt.GetPropertyHeight(label, valueType, (int)value, beforeSet, setterOrNull,
                    labelGrayColor, inHorizontalLayout, targets, richTextTagProvider, foldoutViewKey);
            }

            if (valueType == typeof(uint) || value is uint)
            {
                return IMGUIEditUInt.GetPropertyHeight(label, valueType, (uint)value, beforeSet, setterOrNull,
                    labelGrayColor, inHorizontalLayout, targets, richTextTagProvider, foldoutViewKey);
            }

            if (valueType == typeof(long) || value is long)
            {
                return IMGUIEditLong.GetPropertyHeight(label, valueType, (long)value, beforeSet, setterOrNull,
                    labelGrayColor, inHorizontalLayout, targets, richTextTagProvider, foldoutViewKey);
            }

            if (valueType == typeof(ulong) || value is ulong)
            {
                return IMGUIEditULong.GetPropertyHeight(label, valueType, (ulong)value, beforeSet, setterOrNull,
                    labelGrayColor, inHorizontalLayout, targets, richTextTagProvider, foldoutViewKey);
            }

            if (valueType == typeof(float) || value is float)
            {
                return IMGUIEditFloat.GetPropertyHeight(label, valueType, (float)value, beforeSet, setterOrNull,
                    labelGrayColor, inHorizontalLayout, targets, richTextTagProvider, foldoutViewKey);
            }

            if (valueType == typeof(double) || value is double)
            {
                return IMGUIEditDouble.GetPropertyHeight(label, valueType, (double)value, beforeSet, setterOrNull,
                    labelGrayColor, inHorizontalLayout, targets, richTextTagProvider, foldoutViewKey);
            }

            if (valueType == typeof(string) || value is string)
            {
                return IMGUIEditString.GetPropertyHeight(label, valueType, (string)value, beforeSet, setterOrNull,
                    labelGrayColor, inHorizontalLayout, targets, richTextTagProvider, foldoutViewKey);
            }

            if (valueType == typeof(char) || value is char)
            {
                return IMGUIEditChar.GetPropertyHeight(label, valueType, (char)value, beforeSet, setterOrNull,
                    labelGrayColor, inHorizontalLayout, targets, richTextTagProvider, foldoutViewKey);
            }

            if (valueType == typeof(Vector2) || value is Vector2)
            {
                return IMGUIEditVector2.GetPropertyHeight(label, valueType, (Vector2)value, beforeSet, setterOrNull,
                    labelGrayColor, inHorizontalLayout, targets, richTextTagProvider, foldoutViewKey);
            }

            if (valueType == typeof(Vector3) || value is Vector3)
            {
                return IMGUIEditVector3.GetPropertyHeight(label, valueType, (Vector3)value, beforeSet, setterOrNull,
                    labelGrayColor, inHorizontalLayout, targets, richTextTagProvider, foldoutViewKey);
            }

            if (valueType == typeof(Vector4) || value is Vector4)
            {
                return IMGUIEditVector4.GetPropertyHeight(label, valueType, (Vector4)value, beforeSet, setterOrNull,
                    labelGrayColor, inHorizontalLayout, targets, richTextTagProvider, foldoutViewKey);
            }

            if (valueType == typeof(Vector2Int) || value is Vector2Int)
            {
                return IMGUIEditVector2Int.GetPropertyHeight(label, valueType, (Vector2Int)value, beforeSet,
                    setterOrNull, labelGrayColor, inHorizontalLayout, targets, richTextTagProvider, foldoutViewKey);
            }

            if (valueType == typeof(Vector3Int) || value is Vector3Int)
            {
                return IMGUIEditVector3Int.GetPropertyHeight(label, valueType, (Vector3Int)value, beforeSet,
                    setterOrNull, labelGrayColor, inHorizontalLayout, targets, richTextTagProvider, foldoutViewKey);
            }

            if (valueType == typeof(Quaternion) || value is Quaternion)
            {
                return IMGUIEditQuaternion.GetPropertyHeight(label, valueType, (Quaternion)value, beforeSet,
                    setterOrNull, labelGrayColor, inHorizontalLayout, targets, richTextTagProvider, foldoutViewKey);
            }

            if (valueType == typeof(Color) || value is Color)
            {
                _ = (Color)value;
                return IMGUIColor.GetHeight(inHorizontalLayout);
            }

            if (valueType == typeof(Bounds) || value is Bounds)
            {
                _ = (Bounds)value;
                return IMGUIBounds.GetHeight();
            }

            if (valueType == typeof(Rect) || value is Rect)
            {
                _ = (Rect)value;
                return IMGUIRect.GetHeight();
            }

            if (valueType == typeof(RectInt) || value is RectInt)
            {
                _ = (RectInt)value;
                return IMGUIRectInt.GetHeight();
            }

            if (valueType == typeof(BoundsInt) || value is BoundsInt)
            {
                _ = (BoundsInt)value;
                return IMGUIBoundsInt.GetHeight();
            }

            if (valueType?.BaseType == typeof(Enum) || value is Enum)
            {
                return IMGUIEditEnum.GetPropertyHeight(label, valueType, (Enum)value, beforeSet, setterOrNull,
                    labelGrayColor, inHorizontalLayout, targets, richTextTagProvider, foldoutViewKey);
            }

            if (valueType != null && typeof(UnityEngine.Object).IsAssignableFrom(valueType) ||
                value is UnityEngine.Object)
            {
                return IMGUIEditObject.GetPropertyHeight(label, valueType, (UnityEngine.Object)value, beforeSet,
                    setterOrNull, labelGrayColor, inHorizontalLayout, targets, richTextTagProvider, foldoutViewKey);
            }

            if (valueType != null && typeof(AnimationCurve).IsAssignableFrom(valueType) || value is AnimationCurve)
            {
                return IMGUIEditAnimationCurve.GetPropertyHeight(label, valueType, (AnimationCurve)value, beforeSet,
                    setterOrNull, labelGrayColor, inHorizontalLayout, targets, richTextTagProvider, foldoutViewKey);
            }

            if (valueType == typeof(Hash128) || value is Hash128)
            {
                return IMGUIEditHash128.GetPropertyHeight(label, valueType, (Hash128)value, beforeSet, setterOrNull,
                    labelGrayColor, inHorizontalLayout, targets, richTextTagProvider, foldoutViewKey);
            }

            if (valueType != null && typeof(Gradient).IsAssignableFrom(valueType) || value is Gradient)
            {
                return IMGUIEditGradient.GetPropertyHeight(label, valueType, (Gradient)value, beforeSet,
                    setterOrNull, labelGrayColor, inHorizontalLayout, targets, richTextTagProvider, foldoutViewKey);
            }

            if (valueType == typeof(DateTime) || value is DateTime)
            {
                return IMGUIEditDateTime.GetPropertyHeight(label, valueType, (DateTime)value, beforeSet,
                    setterOrNull, labelGrayColor, inHorizontalLayout, targets, richTextTagProvider, foldoutViewKey);
            }

            if (valueType == typeof(TimeSpan) || value is TimeSpan)
            {
                return IMGUIEditTimeSpan.GetPropertyHeight(label, valueType, (TimeSpan)value, beforeSet,
                    setterOrNull, labelGrayColor, inHorizontalLayout, targets, richTextTagProvider, foldoutViewKey);
            }

            if (valueType == typeof(LayerMask) || value is LayerMask)
            {
                _ = (LayerMask)value;
                return IMGUILayerMask.GetHeight(inHorizontalLayout);
            }

            if (valueType == typeof(Guid) || value is Guid)
            {
                return IMGUIEditGuid.GetPropertyHeight(label, valueType, (Guid)value, beforeSet, setterOrNull,
                    labelGrayColor, inHorizontalLayout, targets, richTextTagProvider, foldoutViewKey);
            }

            if (valueType == typeof(decimal) || value is decimal)
            {
                return IMGUIEditDecimal.GetPropertyHeight(label, valueType, (decimal)value, beforeSet, setterOrNull,
                    labelGrayColor, inHorizontalLayout, targets, richTextTagProvider, foldoutViewKey);
            }

            if (valueType == typeof(Type) || value is Type)
            {
                return IMGUIEditType.GetPropertyHeight(label, valueType, (Type)value, beforeSet, setterOrNull,
                    labelGrayColor, inHorizontalLayout, targets, richTextTagProvider, foldoutViewKey);
            }

            if (valueType?.BaseType == typeof(TypeInfo) || value is TypeInfo)
            {
                return IMGUIEditType.GetPropertyHeight(label, valueType, (TypeInfo)value, beforeSet, setterOrNull,
                    labelGrayColor, inHorizontalLayout, targets, richTextTagProvider, foldoutViewKey);
            }

            (bool hasDictionaryHeight, float dictionaryHeight) = IMGUIEditDictionary.GetPropertyHeight(label, valueType,
                value, beforeSet, setterOrNull, labelGrayColor, inHorizontalLayout, allAttributes, targets,
                richTextTagProvider, foldoutViewKey);
            if (hasDictionaryHeight)
            {
                return dictionaryHeight;
            }

            (bool hasListHeight, float listHeight) = IMGUIEditList.GetPropertyHeight(label, valueType,
                value, beforeSet, setterOrNull, labelGrayColor, inHorizontalLayout, allAttributes, targets,
                richTextTagProvider, foldoutViewKey);
            if (hasListHeight)
            {
                return listHeight;
            }

            return IMGUIGeneralType.GetPropertyHeight(
                label, valueType, value, beforeSet, setterOrNull, labelGrayColor, inHorizontalLayout, allAttributes,
                targets, richTextTagProvider, foldoutViewKey);
        }
    }
}

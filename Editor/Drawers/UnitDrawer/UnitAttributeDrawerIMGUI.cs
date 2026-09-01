using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using SaintsField.Editor.Units;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.UnitDrawer
{
    public partial class UnitAttributeDrawer
    {
        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            float width, int index, ISaintsAttribute saintsAttribute, FieldInfo info, bool hasLabelWidth,
            object parent) => EditorGUIUtility.singleLineHeight;

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent)
        {
            UnitAttribute unitAttribute = (UnitAttribute)saintsAttribute;
            if (!string.IsNullOrEmpty(GetImGuiError(property, unitAttribute, allAttributes, info)))
            {
                RawDefaultDrawer(position, property, allAttributes, label, info);
                DrawOverrideRichText(position, label, overrideRichTextChunks);
                return;
            }

            Type rawType = GetRawType(property, info);
            bool previousShowMixedValue = EditorGUI.showMixedValue;
            EditorGUI.showMixedValue = property.hasMultipleDifferentValues;
            try
            {
                if (rawType == typeof(sbyte))
                {
                    DrawSByte(position, property, label, unitAttribute);
                }
                else if (rawType == typeof(byte))
                {
                    DrawByte(position, property, label, unitAttribute);
                }
                else if (rawType == typeof(short))
                {
                    DrawShort(position, property, label, unitAttribute);
                }
                else if (rawType == typeof(ushort))
                {
                    DrawUShort(position, property, label, unitAttribute);
                }
                else if (rawType == typeof(uint))
                {
                    DrawUInt(position, property, label, unitAttribute);
                }
                else if (rawType == typeof(long))
                {
                    DrawLong(position, property, label, unitAttribute);
                }
                else if (rawType == typeof(ulong))
                {
                    DrawULong(position, property, label, unitAttribute);
                }
                else if (rawType == typeof(float))
                {
                    DrawFloat(position, property, label, unitAttribute);
                }
                else if (rawType == typeof(double))
                {
                    DrawDouble(position, property, label, unitAttribute);
                }
                else
                {
                    DrawInt(position, property, label, unitAttribute);
                }
            }
            finally
            {
                EditorGUI.showMixedValue = previousShowMixedValue;
            }

            DrawOverrideRichText(position, label, overrideRichTextChunks);
        }

        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute, int index,
            FieldInfo info, object parent)
        {
            UnitState state = GetState((UnitAttribute)saintsAttribute);
            if (!string.IsNullOrEmpty(state.Error))
            {
                return 0f;
            }

            string text = GetDisplayText(state.DisplayUnit);
            return Math.Max(36f, GUI.skin.button.CalcSize(new GUIContent(text)).x);
        }

        protected override bool DrawPostFieldImGui(Rect position, Rect fullRect, SerializedProperty property,
            GUIContent label, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, FieldInfo info, object parent)
        {
            UnitState state = GetState((UnitAttribute)saintsAttribute);
            if (!string.IsNullOrEmpty(state.Error))
            {
                return false;
            }

            if (GUI.Button(position, new GUIContent(GetDisplayText(state.DisplayUnit), state.DisplayUnit.Name)))
            {
                ShowUnitMenu(position, state);
            }

            return true;
        }

        protected virtual string GetImGuiError(SerializedProperty property, UnitAttribute unitAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes, FieldInfo info)
        {
            UnitState state = GetState(unitAttribute);
            if (!string.IsNullOrEmpty(state.Error))
            {
                return state.Error;
            }

            Type rawType = GetRawType(property, info);
            return IsSupportedNumberType(rawType)
                ? ""
                : $"Unit does not support {rawType?.Name ?? property.propertyType.ToString()} fields.";
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute,
            int index, FieldInfo info, object parent) =>
            !string.IsNullOrEmpty(GetImGuiError(property, (UnitAttribute)saintsAttribute, allAttributes, info));

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute,
            int index, FieldInfo info, object parent)
        {
            string error = GetImGuiError(property, (UnitAttribute)saintsAttribute, allAttributes, info);
            return string.IsNullOrEmpty(error) ? 0f : ImGuiHelpBox.GetHeight(error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent)
        {
            string error = GetImGuiError(property, (UnitAttribute)saintsAttribute, allAttributes, info);
            return string.IsNullOrEmpty(error)
                ? position
                : ImGuiHelpBox.Draw(position, error, MessageType.Error);
        }

        private static Type GetRawType(SerializedProperty property, FieldInfo info) =>
            SerializedUtils.PropertyPathIndex(property.propertyPath) >= 0
                ? ReflectUtils.GetElementType(info.FieldType)
                : info.FieldType;

        private static bool IsSupportedNumberType(Type type) =>
            type == typeof(sbyte) || type == typeof(byte) || type == typeof(short) || type == typeof(ushort) ||
            type == typeof(int) || type == typeof(uint) || type == typeof(long) || type == typeof(ulong) ||
            type == typeof(float) || type == typeof(double);

        private static string GetDisplayText(UnitInfo unitInfo) => string.IsNullOrEmpty(unitInfo.PrimarySymbol)
            ? unitInfo.Name
            : unitInfo.PrimarySymbol;

        private static void ShowUnitMenu(Rect position, UnitState state)
        {
            GenericMenu menu = new GenericMenu();
            foreach (UnitInfo unitInfo in UnitRegistry.GetAllUnitInfos(state.BaseUnit.Category))
            {
                UnitInfo captured = unitInfo;
                string label = string.IsNullOrEmpty(unitInfo.PrimarySymbol)
                    ? unitInfo.Name
                    : $"{unitInfo.Name} ({unitInfo.PrimarySymbol})";
                menu.AddItem(new GUIContent(label), ReferenceEquals(unitInfo, state.DisplayUnit),
                    () => state.SetDisplayUnit(captured));
            }

            menu.DropDown(position);
        }

        private static void DrawSByte(Rect position, SerializedProperty property, GUIContent label,
            UnitAttribute attribute)
        {
            (string error, sbyte display) = GetSByteValuePre((sbyte)property.intValue, attribute);
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError(error);
                return;
            }
            using EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope();
            int edited = EditorGUI.IntField(position, label, display);
            if (changed.changed)
            {
                (string postError, sbyte value) = GetSByteValuePost((sbyte)edited, attribute);
                if (string.IsNullOrEmpty(postError))
                {
                    property.intValue = value;
                    TriggerChangedIMGUI(property, value);
                }
            }
        }

        private static void DrawByte(Rect position, SerializedProperty property, GUIContent label,
            UnitAttribute attribute)
        {
            (string error, byte display) = GetByteValuePre((byte)property.intValue, attribute);
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError(error);
                return;
            }
            using EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope();
            int edited = EditorGUI.IntField(position, label, display);
            if (changed.changed)
            {
                (string postError, byte value) = GetByteValuePost((byte)edited, attribute);
                if (string.IsNullOrEmpty(postError))
                {
                    property.intValue = value;
                    TriggerChangedIMGUI(property, value);
                }
            }
        }

        private static void DrawShort(Rect position, SerializedProperty property, GUIContent label,
            UnitAttribute attribute)
        {
            (string error, short display) = GetShortValuePre((short)property.intValue, attribute);
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError(error);
                return;
            }
            using EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope();
            int edited = EditorGUI.IntField(position, label, display);
            if (changed.changed)
            {
                (string postError, short value) = GetShortValuePost((short)edited, attribute);
                if (string.IsNullOrEmpty(postError))
                {
                    property.intValue = value;
                    TriggerChangedIMGUI(property, value);
                }
            }
        }

        private static void DrawUShort(Rect position, SerializedProperty property, GUIContent label,
            UnitAttribute attribute)
        {
            (string error, ushort display) = GetUShortValuePre((ushort)property.intValue, attribute);
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError(error);
                return;
            }
            using EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope();
            int edited = EditorGUI.IntField(position, label, display);
            if (changed.changed)
            {
                (string postError, ushort value) = GetUShortValuePost((ushort)edited, attribute);
                if (string.IsNullOrEmpty(postError))
                {
                    property.intValue = value;
                    TriggerChangedIMGUI(property, value);
                }
            }
        }

        private static void DrawInt(Rect position, SerializedProperty property, GUIContent label,
            UnitAttribute attribute)
        {
            (string error, int display) = GetIntValuePre(property.intValue, attribute);
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError(error);
                return;
            }
            using EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope();
            int edited = EditorGUI.IntField(position, label, display);
            if (changed.changed)
            {
                (string postError, int value) = GetIntValuePost(edited, attribute);
                if (string.IsNullOrEmpty(postError))
                {
                    property.intValue = value;
                    TriggerChangedIMGUI(property, value);
                }
            }
        }

        private static void DrawUInt(Rect position, SerializedProperty property, GUIContent label,
            UnitAttribute attribute)
        {
            (string error, uint display) = GetUIntValuePre(property.uintValue, attribute);
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError(error);
                return;
            }
            using EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope();
            long edited = EditorGUI.LongField(position, label, display);
            if (changed.changed)
            {
                uint clipped = (uint)Math.Max(0L, Math.Min(uint.MaxValue, edited));
                (string postError, uint value) = GetUIntValuePost(clipped, attribute);
                if (string.IsNullOrEmpty(postError))
                {
                    property.uintValue = value;
                    TriggerChangedIMGUI(property, value);
                }
            }
        }

        private static void DrawLong(Rect position, SerializedProperty property, GUIContent label,
            UnitAttribute attribute)
        {
            (string error, long display) = GetLongValuePre(property.longValue, attribute);
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError(error);
                return;
            }
            using EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope();
            long edited = EditorGUI.LongField(position, label, display);
            if (changed.changed)
            {
                (string postError, long value) = GetLongValuePost(edited, attribute);
                if (string.IsNullOrEmpty(postError))
                {
                    property.longValue = value;
                    TriggerChangedIMGUI(property, value);
                }
            }
        }

        private static void DrawULong(Rect position, SerializedProperty property, GUIContent label,
            UnitAttribute attribute)
        {
            (string error, ulong display) = GetULongValuePre(property.ulongValue, attribute);
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError(error);
                return;
            }
            using EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope();
            string edited = EditorGUI.TextField(position, label, display.ToString(CultureInfo.InvariantCulture));
            if (changed.changed)
            {
                (bool success, ulong parsed) = ParseULong(edited);
                if (!success)
                {
                    return;
                }
                (string postError, ulong value) = GetULongValuePost(parsed, attribute);
                if (string.IsNullOrEmpty(postError))
                {
                    property.ulongValue = value;
                    TriggerChangedIMGUI(property, value);
                }
            }
        }

        private static void DrawFloat(Rect position, SerializedProperty property, GUIContent label,
            UnitAttribute attribute)
        {
            (string error, float display) = GetFloatValuePre(property.floatValue, attribute);
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError(error);
                return;
            }
            using EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope();
            float edited = EditorGUI.FloatField(position, label, display);
            if (changed.changed)
            {
                (string postError, float value) = GetFloatValuePost(edited, attribute);
                if (string.IsNullOrEmpty(postError))
                {
                    property.floatValue = value;
                    TriggerChangedIMGUI(property, value);
                }
            }
        }

        private static void DrawDouble(Rect position, SerializedProperty property, GUIContent label,
            UnitAttribute attribute)
        {
            (string error, double display) = GetDoubleValuePre(property.doubleValue, attribute);
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError(error);
                return;
            }
            using EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope();
            double edited = EditorGUI.DoubleField(position, label, display);
            if (changed.changed)
            {
                (string postError, double value) = GetDoubleValuePost(edited, attribute);
                if (string.IsNullOrEmpty(postError))
                {
                    property.doubleValue = value;
                    TriggerChangedIMGUI(property, value);
                }
            }
        }

        private static (bool success, ulong result) ParseULong(string value)
        {
            try
            {
                return (true, System.Convert.ToUInt64(value, CultureInfo.InvariantCulture));
            }
            catch (Exception)
            {
                return (false, 0UL);
            }
        }
    }
}

using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.ArraySizeDrawer
{
    public partial class ArraySizeAttributeDrawer
    {
        private sealed class InfoIMGUI
        {
            public string Error = "";
            public bool Dynamic = true;
            public int Min;
            public int Max;
        }

        private static readonly Dictionary<string, InfoIMGUI> InfoCacheIMGUI = new Dictionary<string, InfoIMGUI>();

        private static InfoIMGUI EnsureKey(SerializedProperty property)
        {
            string key = SerializedUtils.GetUniqueId(property);
            if (InfoCacheIMGUI.TryGetValue(key, out InfoIMGUI cache))
            {
                return cache;
            }

            InfoCacheIMGUI[key] = cache = new InfoIMGUI();
            NoLongerInspectingWatch(property.serializedObject.targetObject, key, () => InfoCacheIMGUI.Remove(key));
            return cache;
        }

        protected override bool WillDrawAbove(SerializedProperty property, ISaintsAttribute saintsAttribute,
            FieldInfo info, object parent) => true;

        protected override float GetAboveExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info, object parent) => 0f;

        protected override Rect DrawAboveImGui(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index,
            FieldInfo info, object parent)
        {
            UpdateArraySize(property, (ArraySizeAttribute)saintsAttribute, info, parent, true);
            return position;
        }

        private static void UpdateArraySize(SerializedProperty property, ArraySizeAttribute arraySizeAttribute,
            MemberInfo info, object parent, bool applyChange)
        {
            InfoIMGUI cache = EnsureKey(property);

            object actualParent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
            if (actualParent == null)
            {
                Debug.LogWarning($"{property.propertyPath} parent disposed unexpectedly");
                cache.Error = "";
                return;
            }

            (SerializedProperty arrProp, int _, string error) = Util.GetArrayProperty(property, info, actualParent);
            cache.Error = error;

            if (cache.Error != "")
            {
                return;
            }

            if (cache.Dynamic)
            {
                (string callbackError, bool dynamic, int min, int max) =
                    GetMinMax(arraySizeAttribute, property, info, actualParent);
                cache.Dynamic = dynamic;
                if (callbackError != "")
                {
                    cache.Error = callbackError;
                    return;
                }

                cache.Min = min;
                cache.Max = max;
            }

            if (!applyChange)
            {
                return;
            }

            bool changed = false;
            int curSize = arrProp.arraySize;

            if (cache.Min >= 0 && curSize < cache.Min)
            {
                arrProp.arraySize = cache.Min;
                changed = true;
            }

            if (cache.Max >= 0 && curSize > cache.Max)
            {
                arrProp.arraySize = cache.Max;
                changed = true;
            }

            if (changed)
            {
                arrProp.serializedObject.ApplyModifiedProperties();
            }
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent)
        {
            UpdateArraySize(property, (ArraySizeAttribute)saintsAttribute, info, parent, false);
            return EnsureKey(property).Error != "";
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            IReadOnlyList<PropertyAttribute> allAttributes,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            string error = EnsureKey(property).Error;
            return error == "" ? 0 : ImGuiHelpBox.GetHeight(error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent)
        {
            string error = EnsureKey(property).Error;
            return error == ""
                ? position
                : ImGuiHelpBox.Draw(position, error, MessageType.Error);
        }
    }
}

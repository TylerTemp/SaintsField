using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace SaintsField.Editor.Drawers.ValidateInputDrawer
{
    public partial class ValidateInputAttributeDrawer
    {
        private sealed class InfoIMGUI
        {
            public string Error = "";
            public bool HasChecked;
            public bool CheckNow = true;
            public UnityAction OnEditorChanged;
        }

        private static readonly Dictionary<string, InfoIMGUI> InfoCacheIMGUI = new Dictionary<string, InfoIMGUI>();

        private static InfoIMGUI EnsureKey(SerializedProperty property, int index)
        {
            string key = $"{SerializedUtils.GetUniqueId(property)}_{index}";
            if (InfoCacheIMGUI.TryGetValue(key, out InfoIMGUI cache))
            {
                return cache;
            }

            cache = new InfoIMGUI();
            InfoCacheIMGUI[key] = cache;

            cache.OnEditorChanged = () => cache.CheckNow = true;
            SaintsEditorApplicationChanged.OnAnyEvent.AddListener(cache.OnEditorChanged);

            NoLongerInspectingWatch(property.serializedObject.targetObject, key, () =>
            {
                InfoCacheIMGUI.Remove(key);
                SaintsEditorApplicationChanged.OnAnyEvent.RemoveListener(cache.OnEditorChanged);
            });

            return cache;
        }

        private static void CallValidate(InfoIMGUI cache, SerializedProperty property, ValidateInputAttribute attribute,
            MemberInfo info)
        {
            object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
            if (parent == null)
            {
                Debug.LogWarning($"{property.propertyPath} parent disposed unexpectedly.");
                cache.Error = "";
                cache.HasChecked = true;
                cache.CheckNow = false;
                return;
            }

            SaintsContext.SerializedProperty = property;
            cache.Error = CallValidateMethod(attribute.Callback, property.displayName, property, info, parent);
            cache.HasChecked = true;
            cache.CheckNow = false;
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent)
        {
            InfoIMGUI cache = EnsureKey(property, index);
            if (!cache.HasChecked || cache.CheckNow)
            {
                CallValidate(cache, property, (ValidateInputAttribute)saintsAttribute, info);
            }

            return cache.Error != "";
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            IReadOnlyList<PropertyAttribute> allAttributes,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            string error = EnsureKey(property, index).Error;
            return error == "" ? 0 : ImGuiHelpBox.GetHeight(error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent)
        {
            string error = EnsureKey(property, index).Error;
            return error == "" ? position : ImGuiHelpBox.Draw(position, error, MessageType.Error);
        }

        protected override void OnPropertyEndImGui(Rect labelFieldRect, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int saintsIndex, FieldInfo info, object parent)
        {
            if (!SaintsFieldConfigUtil.GetValidateInputLoopCheckUIToolkit())
            {
                return;
            }

            CallValidate(EnsureKey(property, saintsIndex), property, (ValidateInputAttribute)saintsAttribute, info);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.CustomPicker.RequireTypeDrawer
{
    public partial class RequireTypeAttributeDrawer
    {
        private static Texture2D _pickIcon;

        private sealed class InfoIMGUI
        {
            public string Error = "";
            public bool HasCorrectValue;
            public object CorrectValue;
            public object PreviousValue;
        }

        private static readonly Dictionary<string, InfoIMGUI> InfoCacheIMGUI = new Dictionary<string, InfoIMGUI>();

        private static InfoIMGUI EnsureInfo(SerializedProperty property)
        {
            string key = SerializedUtils.GetUniqueId(property);
            if (InfoCacheIMGUI.TryGetValue(key, out InfoIMGUI infoCache))
            {
                return infoCache;
            }

            InfoCacheIMGUI[key] = infoCache = new InfoIMGUI();
            NoLongerInspectingWatch(property.serializedObject.targetObject, key, () => InfoCacheIMGUI.Remove(key));
            return infoCache;
        }

        protected override float DrawPreLabelImGui(Rect position, SerializedProperty property,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            if(property.propertyType ==  SerializedPropertyType.ObjectReference)
            {
                EnsureInfo(property).PreviousValue = property.objectReferenceValue;
            }
            return -1;
        }

        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            RequireTypeAttribute requireTypeAttribute = (RequireTypeAttribute)saintsAttribute;
            return requireTypeAttribute.CustomPicker ? 20 : 0;
        }

        private GUIStyle _imGuiButtonStyle;

        protected override bool DrawPostFieldImGui(Rect position, Rect fullRect, SerializedProperty property,
            GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent)
        {
            RequireTypeAttribute requireTypeAttribute = (RequireTypeAttribute)saintsAttribute;
            IReadOnlyList<Type> requiredTypes = requireTypeAttribute.RequiredTypes;

            bool customPicker = requireTypeAttribute.CustomPicker;
            if (customPicker)
            {
                if (_imGuiButtonStyle == null)
                {
                    _imGuiButtonStyle = new GUIStyle(GUI.skin.button)
                    {
                        padding = new RectOffset(0, 0, 0, 0),
                    };
                }

                _pickIcon ??= Util.LoadResource<Texture2D>("d_pick");
                if (GUI.Button(position, new GUIContent(_pickIcon), _imGuiButtonStyle))
                {
                    OpenSelectorWindowIMGUI(property, requireTypeAttribute, info,
                        newValue => TriggerChangedIMGUI(property, newValue), parent);
                }
            }

            InfoIMGUI cacheInfo = EnsureInfo(property);
            cacheInfo.Error = "";

            Object curValue = GetCurFieldValue(property, requireTypeAttribute);
            IReadOnlyList<string> missingTypeNames = curValue == null
                ? Array.Empty<string>()
                : GetMissingTypeNames(curValue, requiredTypes);

            if (missingTypeNames.Count == 0)
            {
                cacheInfo.HasCorrectValue = true;
                if (TryGetCurrentSerializedValue(property, info, parent, out object currentValue))
                {
                    cacheInfo.CorrectValue = currentValue;
                }
            }
            else
            {
                string errorMessage = $"{curValue} has no component{(missingTypeNames.Count > 1 ? "s" : "")} {string.Join(", ", missingTypeNames)}.";
                if (requireTypeAttribute.FreeSign || !cacheInfo.HasCorrectValue)
                {
                    cacheInfo.Error = errorMessage;
                }
                else
                {
                    RestorePreviousValue(property, info, parent, cacheInfo.CorrectValue);
                    property.serializedObject.ApplyModifiedProperties();
                    TriggerChangedIMGUI(property, GetPreviousValue(cacheInfo.CorrectValue));
                    Debug.LogWarning($"{errorMessage} Change reverted to {(cacheInfo.CorrectValue == null ? "null" : cacheInfo.CorrectValue.ToString())}.");
                }
            }

            return customPicker;
        }

        private static bool TryGetCurrentSerializedValue(SerializedProperty property, FieldInfo info, object parent,
            out object value)
        {
            (string error, int _, object currentValue) = Util.GetValue(property, info, parent);
            if (error != "")
            {
                value = null;
                return false;
            }

            value = currentValue;
            return true;
        }

        protected virtual Object GetCurFieldValue(SerializedProperty property, RequireTypeAttribute _) => property.objectReferenceValue;

        protected virtual void RestorePreviousValue(SerializedProperty property, FieldInfo info, object parent, object previousValue)
        {
            property.objectReferenceValue = (Object)previousValue;
            ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, previousValue);
        }

        protected virtual object GetPreviousValue(object previousValue) => previousValue;

        private static IEnumerable<Object> GetQualifiedInterfaces(IReadOnlyList<Object> toCheckTargets,
            IReadOnlyList<Type> interfaceTypes)
        {
            foreach (Object target in toCheckTargets)
            {
                if(interfaceTypes.All(each => each.IsAssignableFrom(target.GetType())))
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_REQUIRE_TYPE
                    Debug.Log($"GetQualifiedInterfaces: {target}");
#endif
                    yield return target;
                }
            }
        }

        private static IEnumerable<Object> GetQualifiedComponent(IReadOnlyList<Object> toCheckTargets,
            IReadOnlyList<Type> normalTypes)
        {
            foreach (Object fieldResult in toCheckTargets)
            {
                switch (fieldResult)
                {
                    case GameObject go:
                    {
                        bool incapable = false;
                        List<Type> toCheckComponents = new List<Type>();
                        foreach (Type normalType in normalTypes)
                        {
                            if (typeof(ScriptableObject).IsAssignableFrom(normalType))
                            {
                                incapable = true;
                                break;
                            }

                            if (!typeof(GameObject).IsAssignableFrom(normalType))
                            {
                                continue;
                            }

                            if (!typeof(Component).IsAssignableFrom(normalType))
                            {
                                incapable = true;
                                break;
                            }

                            toCheckComponents.Add(normalType);
                        }

                        if (incapable)
                        {
                            continue;
                        }

                        if(toCheckComponents.All(requiredComp => go.GetComponent(requiredComp) != null))
                        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_REQUIRE_TYPE
                            Debug.Log($"IsQualifiedGo: {go}");
#endif
                            yield return go;
                        }
                    }
                        break;
                    case ScriptableObject so:
                    {
                        if(normalTypes.All(each => each.IsAssignableFrom(so.GetType())))
                        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_REQUIRE_TYPE
                            Debug.Log($"IsQualifiedSo: {so}");
#endif
                            yield return so;
                        }
                    }
                        break;
                    case Component comp:
                    {
                        bool incapable = false;
                        foreach (Type normalType in normalTypes)
                        {
                            if (typeof(GameObject).IsAssignableFrom(normalType))
                            {
                                continue;
                            }

                            if (typeof(ScriptableObject).IsAssignableFrom(normalType))
                            {
                                incapable = true;
                                break;
                            }

                            if (comp.GetComponent(normalType) == null)
                            {
                                incapable = true;
                                break;
                            }
                        }

                        if (incapable)
                        {
                            continue;
                        }

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_REQUIRE_TYPE
                        Debug.Log($"IsQualifiedComp: {comp}");
#endif
                        yield return comp;
                    }
                        break;
                }
            }
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent) => EnsureInfo(property).Error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            IReadOnlyList<PropertyAttribute> allAttributes,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            string error = EnsureInfo(property).Error;
            return error == "" ? 0 : ImGuiHelpBox.GetHeight(error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent) => ImGuiHelpBox.Draw(position, EnsureInfo(property).Error, MessageType.Error);
    }
}

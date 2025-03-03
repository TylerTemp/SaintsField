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
        #region IMGUI

        // ReSharper disable once InconsistentNaming
        protected string _error { private get; set; } = "";
        protected bool ImGuiFirstChecked { get; private set; }

        private Object _previousValue;

        protected override float DrawPreLabelImGui(Rect position, SerializedProperty property,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            _previousValue = property.objectReferenceValue;
            return base.DrawPreLabelImGui(position, property, saintsAttribute, info, parent);
        }

        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, OnGUIPayload onGuiPayload, FieldInfo info, object parent)
        {
            RequireTypeAttribute requireTypeAttribute = (RequireTypeAttribute)saintsAttribute;
            return requireTypeAttribute.CustomPicker ? 20 : 0;
        }

        private GUIStyle _imGuiButtonStyle;

        protected override bool DrawPostFieldImGui(Rect position, Rect fullRect, SerializedProperty property,
            GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            RequireTypeAttribute requireTypeAttribute = (RequireTypeAttribute)saintsAttribute;
            IReadOnlyList<Type> requiredTypes = requireTypeAttribute.RequiredTypes;

            bool customPicker = requireTypeAttribute.CustomPicker;
            if(customPicker)
            {
                // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
                if (_imGuiButtonStyle == null)
                {
                    _imGuiButtonStyle = new GUIStyle(GUI.skin.button)
                    {
                        // margin = new RectOffset(0, 0, 0, 0),
                        padding = new RectOffset(0, 0, 0, 0),
                    };
                }

                if (GUI.Button(position, "●", _imGuiButtonStyle))
                {
                    OpenSelectorWindow(property, requireTypeAttribute, info, onGUIPayload.SetValue, parent);
                }
            }

            if (!ImGuiFirstChecked || onGUIPayload.changed)
            {
                // Debug.Log($"onGUIPayload.changed={onGUIPayload.changed}/_imGuiFirstChecked={_imGuiFirstChecked}");
                _error = "";
                // bool isFirstCheck = !_imGuiFirstChecked;
                // Debug.Log($"_imGuiFirstChecked={_imGuiFirstChecked}/freeSign={fieldInterfaceAttribute.FreeSign}");


                Object curValue = GetCurFieldValue(property, requireTypeAttribute);
                if (curValue is null)
                {
                    return customPicker;
                }

                IReadOnlyList<string> missingTypeNames = GetMissingTypeNames(curValue, requiredTypes);

                // Debug.Log($"missingTypeNames={string.Join(",", missingTypeNames)}, _imGuiFirstChecked={_imGuiFirstChecked}");

                if (missingTypeNames.Count > 0)  // if has errors
                {
                    string errorMessage = $"{curValue} has no component{(missingTypeNames.Count > 1? "s": "")} {string.Join(", ", missingTypeNames)}.";
                    // freeSign will always give error information
                    // but if you never passed the first check, then sign as you want and it'll always just show error
                    if (!ImGuiFirstChecked || requireTypeAttribute.FreeSign)
                    {
                        // Debug.Log($"isFirstCheck={isFirstCheck}/freeSign={fieldInterfaceAttribute.FreeSign}");
                        _error = errorMessage;
                    }
                    else  // it's not freeSign, and you've already got a correct answer. So revert to the old value.
                    {
                        // property.objectReferenceValue = _previousValue;
                        RestorePreviousValue(property, info, parent);
                        onGUIPayload.SetValue(GetPreviousValue());
                        Debug.LogWarning($"{errorMessage} Change reverted to {(_previousValue==null? "null": _previousValue.ToString())}.");
                    }
                }
                else
                {
                    ImGuiFirstChecked = true;
                }
            }

            return customPicker;
        }

        protected virtual Object GetCurFieldValue(SerializedProperty property, RequireTypeAttribute _) => property.objectReferenceValue;

        protected virtual void RestorePreviousValue(SerializedProperty property, FieldInfo info, object parent)
        {
            property.objectReferenceValue = _previousValue;
            ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, _previousValue);
        }

        protected virtual object GetPreviousValue() => _previousValue;


        private static IEnumerable<Object> GetQualifiedInterfaces(IReadOnlyList<Object> toCheckTargets,
            IReadOnlyList<Type> interfaceTypes)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (Object target in toCheckTargets)
            {
                // Debug.Log($"{target} -> {string.Join(",", interfaceTypes)}");
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
                            // ScriptableObject can not be on a GameObject
                            if (typeof(ScriptableObject).IsAssignableFrom(normalType))
                            {
                                incapable = true;
                                break;
                            }

                            if (!typeof(GameObject).IsAssignableFrom(normalType))
                            {
                                continue;  // skip GameObject
                            }

                            if (!typeof(Component).IsAssignableFrom(normalType))  // only Component can be on a gameObject
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

//                         foreach (Component comp in toCheckComponents.Select(eachCompType => go.GetComponent(eachCompType)).Where(each => each != null))
//                         {
// #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_REQUIRE_TYPE
//                             Debug.Log($"GetQualifiedComp: {comp}");
// #endif
//                             yield return comp;
//                         }
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

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent) => _error == "" ? 0 : ImGuiHelpBox.GetHeight(_error, EditorGUIUtility.currentViewWidth, MessageType.Error);

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            OnGUIPayload onGuiPayload, FieldInfo info, object parent) => ImGuiHelpBox.Draw(position, _error, MessageType.Error);
        #endregion
    }
}

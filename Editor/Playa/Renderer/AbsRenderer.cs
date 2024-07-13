using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Condition;
using SaintsField.Editor.Drawers;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;
#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Playa.Renderer
{
    public abstract class AbsRenderer: ISaintsRenderer
    {
        // ReSharper disable InconsistentNaming
        public readonly SaintsFieldWithInfo FieldWithInfo;
        // protected readonly SerializedObject SerializedObject;
        // ReSharper enable InconsistentNaming

        protected struct PreCheckResult
        {
            public bool IsShown;
            public bool IsDisabled;
            public int ArraySize;  // NOTE: -1=No Limit, 0=0, 1=More Than 0
            public bool HasRichLabel;
            public string RichLabelXml;
        }

        protected AbsRenderer(SaintsFieldWithInfo fieldWithInfo)
        {
            FieldWithInfo = fieldWithInfo;
            // SerializedObject = serializedObject;
        }

        private enum PreCheckInternalType
        {
            Show,
            Hide,
            Disable,
            Enable,
        }

        private class PreCheckInternalInfo
        {
            public PreCheckInternalType Type;
            public IReadOnlyList<ConditionInfo> ConditionInfos;
            public EMode EditorMode;
            public object Target;

            // TODO: handle errors
            public IReadOnlyList<string> errors;
            public IReadOnlyList<bool> boolResults;
        }

        protected static PreCheckResult GetPreCheckResult(SaintsFieldWithInfo fieldWithInfo)
        {
            List<PreCheckInternalInfo> preCheckInternalInfos = new List<PreCheckInternalInfo>();
            int arraySize = -1;
            foreach (IPlayaAttribute playaAttribute in fieldWithInfo.PlayaAttributes)
            {
                switch (playaAttribute)
                {
                    case PlayaHideIfAttribute hideIfAttribute:
                        preCheckInternalInfos.Add(new PreCheckInternalInfo
                        {
                            Type = PreCheckInternalType.Hide,
                            ConditionInfos = hideIfAttribute.ConditionInfos,
                            EditorMode = hideIfAttribute.EditorMode,
                            Target = fieldWithInfo.Target,
                        });
                        break;
                    case PlayaShowIfAttribute showIfAttribute:
                        preCheckInternalInfos.Add(new PreCheckInternalInfo
                        {
                            Type = PreCheckInternalType.Show,
                            ConditionInfos = showIfAttribute.ConditionInfos,
                            EditorMode = showIfAttribute.EditorMode,
                            Target = fieldWithInfo.Target,
                        });
                        break;
                    case PlayaEnableIfAttribute enableIfAttribute:
                        preCheckInternalInfos.Add(new PreCheckInternalInfo
                        {
                            Type = PreCheckInternalType.Enable,
                            ConditionInfos = enableIfAttribute.ConditionInfos,
                            EditorMode = enableIfAttribute.EditorMode,
                            Target = fieldWithInfo.Target,
                        });
                        break;
                    case PlayaDisableIfAttribute disableIfAttribute:
                        preCheckInternalInfos.Add(new PreCheckInternalInfo
                        {
                            Type = PreCheckInternalType.Disable,
                            ConditionInfos = disableIfAttribute.ConditionInfos,
                            EditorMode = disableIfAttribute.EditorMode,
                            Target = fieldWithInfo.Target,
                        });
                        break;
                    case IPlayaArraySizeAttribute arraySizeAttribute:
                        arraySize = fieldWithInfo.SerializedProperty.isArray
                            ? GetArraySize(arraySizeAttribute, fieldWithInfo.SerializedProperty, fieldWithInfo.FieldInfo, fieldWithInfo.Target)
                            : -1;
                        break;
                }
            }

            foreach (PreCheckInternalInfo preCheckInternalInfo in preCheckInternalInfos)
            {
                FillResult(preCheckInternalInfo);
            }

            // no show attribute: show
            // any show attribute is true: show; otherwise: not-show
            bool hasShow = false;
            bool show = true;
            // no hide attribute: show
            // any hide attribute is true: hide; otherwise: not-hide
            bool hasHide = false;
            bool hide = false;
            // no disable attribute: not-disable
            // any disable attribute is true: disable; otherwise: not-disable
            bool disable = false;
            // no enable attribute: enable
            // any enable attribute is true: enable; otherwise: not-enable
            bool enable = true;

            foreach (PreCheckInternalInfo preCheckInternalInfo in preCheckInternalInfos.Where(each => each.errors.Count == 0))
            {
                switch (preCheckInternalInfo.Type)
                {
                    case PreCheckInternalType.Show:
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_SHOW_HIDE
                        Debug.Log(
                            $"show, count={preCheckInternalInfo.boolResults.Count}, values={string.Join(",", preCheckInternalInfo.boolResults)}");
#endif
                        hasShow = true;
                        if (!preCheckInternalInfo.boolResults.All(each => each))
                        {
                            show = false;
                        }
                        break;
                    case PreCheckInternalType.Hide:
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_SHOW_HIDE
                        Debug.Log(
                            $"hide, count={preCheckInternalInfo.boolResults.Count}, values={string.Join(",", preCheckInternalInfo.boolResults)}");
#endif
                        hasHide = true;
                        if (preCheckInternalInfo.boolResults.Count == 0 ||
                            preCheckInternalInfo.boolResults.Any(each => each))
                        {
                            hide = true;
                        }
                        break;
                    case PreCheckInternalType.Disable:
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_DISABLE_ENABLE
                        Debug.Log(
                            $"disable, count={preCheckInternalInfo.boolResults.Count}, values={string.Join(",", preCheckInternalInfo.boolResults)}");
#endif
                        if (preCheckInternalInfo.boolResults.Count == 0 || preCheckInternalInfo.boolResults.All(each => each))
                        {
                            disable = true;
                        }
                        break;
                    case PreCheckInternalType.Enable:
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_DISABLE_ENABLE
                        Debug.Log(
                            $"enable, count={preCheckInternalInfo.boolResults.Count}, values={string.Join(",", preCheckInternalInfo.boolResults)}");
#endif
                        if (preCheckInternalInfo.boolResults.Count != 0 && !preCheckInternalInfo.boolResults.Any(each => each))
                        {
                            enable = false;
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(preCheckInternalInfo.Type), preCheckInternalInfo.Type, null);
                }
            }
            bool showIfResult = true;
            if (hasShow)
            {
                showIfResult = show;
            }

            if (hasHide)
            {
                // ReSharper disable once SimplifyConditionalTernaryExpression
                showIfResult = (hasShow? show: true) && !hide;
            }

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_SHOW_HIDE
            Debug.Log(
                $"showIfResult={showIfResult} (hasShow={hasShow}, show={show}, hide={hide})");
#endif
            bool disableIfResult = disable || !enable;
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_DISABLE_ENABLE
            Debug.Log(
                $"disableIfResult={disableIfResult} (disable={disable}, enable={enable})");
#endif


            PlayaRichLabelAttribute richLabelAttribute = fieldWithInfo.PlayaAttributes.OfType<PlayaRichLabelAttribute>().FirstOrDefault();
            bool hasRichLabel = richLabelAttribute != null;

            string richLabelXml = "";
            if (hasRichLabel)
            {
                richLabelXml = richLabelAttribute.IsCallback ? ParseRichLabelXml(fieldWithInfo, richLabelAttribute.RichTextXml) : richLabelAttribute.RichTextXml;
            }

            return new PreCheckResult
            {
                IsDisabled = disableIfResult,
                IsShown = showIfResult,
                ArraySize = arraySize,

                HasRichLabel = hasRichLabel,
                RichLabelXml = richLabelXml,
            };
        }

        private static int GetArraySize(IPlayaArraySizeAttribute genArraySizeAttribute, SerializedProperty property, FieldInfo info, object parent)
        {
            switch (genArraySizeAttribute)
            {
                case PlayaArraySizeAttribute playaArraySizeAttribute:
                    return playaArraySizeAttribute.Size;
                case ArraySizeAttribute arraySizeAttribute:
                    return arraySizeAttribute.Size;
                case GetComponentAttribute getComponentAttribute:
                    return GetComponentAttributeDrawer.HelperGetArraySize(property, getComponentAttribute, info);
                case GetComponentInChildrenAttribute getComponentInChildrenAttribute:
                    return GetComponentInChildrenAttributeDrawer.HelperGetArraySize(property, getComponentInChildrenAttribute, info);
                case GetComponentInParentsAttribute getComponentInParentsAttribute:
                    return GetComponentInParentsAttributeDrawer.HelperGetArraySize(property, getComponentInParentsAttribute, info);
                case GetComponentInSceneAttribute getComponentInSceneAttribute:
                    return GetComponentInSceneAttributeDrawer.HelperGetArraySize(getComponentInSceneAttribute, info);
                case GetComponentByPathAttribute getComponentByPathAttribute:
                    return GetComponentByPathAttributeDrawer.HelperGetArraySize(property, getComponentByPathAttribute, info);
                case GetPrefabWithComponentAttribute getPrefabWithComponentAttribute:
                    return GetPrefabWithComponentAttributeDrawer.HelperGetArraySize(getPrefabWithComponentAttribute, info);
                default:
                    return -1;
            }
        }

        private static void FillResult(PreCheckInternalInfo preCheckInternalInfo)
        {
            bool editorModeIsTrue = Util.ConditionEditModeChecker(preCheckInternalInfo.EditorMode);
            if (!editorModeIsTrue)
            {
                preCheckInternalInfo.errors = Array.Empty<string>();
                preCheckInternalInfo.boolResults = new[]{false};
                return;
            }

            (IReadOnlyList<string> errors, IReadOnlyList<bool> boolResults) = Util.ConditionChecker(preCheckInternalInfo.ConditionInfos, null, null, preCheckInternalInfo.Target);

            // List<bool> callbackTruly = new List<bool>();
            // List<string> errors = new List<string>();

            // foreach (string callback in preCheckInternalInfo.Callbacks)
            // {
            //     (string error, bool isTruly) = Util.GetTruly(preCheckInternalInfo.Target, callback);
            //     if (error != "")
            //     {
            //         errors.Add(error);
            //     }
            //     callbackTruly.Add(isTruly);
            // }

            if (errors.Count > 0)
            {
                preCheckInternalInfo.errors = errors;
                preCheckInternalInfo.boolResults = Array.Empty<bool>();
                return;
            }

            preCheckInternalInfo.errors = Array.Empty<string>();
            preCheckInternalInfo.boolResults = boolResults;
        }

        private static string ParseRichLabelXml(SaintsFieldWithInfo fieldWithInfo, string richTextXml)
        {
            object target = fieldWithInfo.Target;

            List<Type> types = ReflectUtils.GetSelfAndBaseTypes(target);
            types.Reverse();
            foreach (Type eachType in types)
            {
                (ReflectUtils.GetPropType getPropType, object fieldOrMethodInfo) =
                    ReflectUtils.GetProp(eachType, richTextXml);
                switch (getPropType)
                {
                    case ReflectUtils.GetPropType.Field:
                    {
                        object result = ((FieldInfo)fieldOrMethodInfo).GetValue(target);
                        return result == null ? string.Empty : result.ToString();
                    }

                    case ReflectUtils.GetPropType.Property:
                    {
                        object result = ((PropertyInfo)fieldOrMethodInfo).GetValue(target);
                        return result == null ? string.Empty : result.ToString();
                    }
                    case ReflectUtils.GetPropType.Method:
                    {
                        MethodInfo methodInfo = (MethodInfo)fieldOrMethodInfo;

                        object curValue;
                        string fallbackName;

                        switch (fieldWithInfo.RenderType)
                        {
                            case SaintsRenderType.SerializedField:
                                // this can not be an list element because Editor component do not obtain it
                                curValue = fieldWithInfo.FieldInfo.GetValue(target);
                                // Debug.Log($"ser curValue={curValue}/{fieldWithInfo.FieldInfo.Name}/type={curValue.GetType()}");
                                fallbackName = ObjectNames.NicifyVariableName(fieldWithInfo.FieldInfo.Name);
                                break;
                            case SaintsRenderType.NonSerializedField:
                                curValue = fieldWithInfo.FieldInfo.GetValue(target);
                                fallbackName = ObjectNames.NicifyVariableName(fieldWithInfo.FieldInfo.Name);
                                break;
                            case SaintsRenderType.NativeProperty:
                                curValue = fieldWithInfo.PropertyInfo.GetValue(target);
                                fallbackName = ObjectNames.NicifyVariableName(fieldWithInfo.PropertyInfo.Name);
                                break;
                            case SaintsRenderType.Method:  // not work for method atm
                            default:
                                throw new ArgumentOutOfRangeException(nameof(fieldWithInfo.RenderType), fieldWithInfo.RenderType, null);
                        }

                        object[] passParams = ReflectUtils.MethodParamsFill(methodInfo.GetParameters(), new[]{curValue});

                        // Debug.Log($"passParams={passParams[0]==null}, length={passParams.Length}, curValue==null={curValue==null}");

                        try
                        {
                            return (string)methodInfo.Invoke(
                                target,
                                passParams
                            );
                        }
                        catch (TargetInvocationException e)
                        {
                            Debug.LogException(e);
                            Debug.Assert(e.InnerException != null);
                            return fallbackName;
                        }
                        catch (Exception e)
                        {
                            // _error = e.Message;
                            Debug.LogException(e);
                            return fallbackName;
                        }
                    }
                    case ReflectUtils.GetPropType.NotFound:
                        continue;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(getPropType), getPropType, null);
                }
            }

            switch (fieldWithInfo.RenderType)
            {
                case SaintsRenderType.SerializedField:
                    return ObjectNames.NicifyVariableName(fieldWithInfo.FieldInfo.Name);
                case SaintsRenderType.NonSerializedField:
                    return ObjectNames.NicifyVariableName(fieldWithInfo.FieldInfo.Name);
                case SaintsRenderType.NativeProperty:
                    return ObjectNames.NicifyVariableName(fieldWithInfo.PropertyInfo.Name);
                case SaintsRenderType.Method:  // not work for method atm
                default:
                    throw new ArgumentOutOfRangeException(nameof(fieldWithInfo.RenderType), fieldWithInfo.RenderType, null);
            }
        }

        public abstract void OnDestroy();

#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
        public abstract VisualElement CreateVisualElement();

        protected static PreCheckResult UIToolkitOnUpdate(SaintsFieldWithInfo fieldWithInfo, VisualElement result, bool checkDisable)
        {
            PreCheckResult preCheckResult = GetPreCheckResult(fieldWithInfo);
            if(checkDisable && result.enabledSelf != !preCheckResult.IsDisabled)
            {
                result.SetEnabled(!preCheckResult.IsDisabled);
            }

            bool isShown = result.style.display != DisplayStyle.None;

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_PLAYA_IS_SHOWN
            Debug.Log($"{fieldWithInfo} {result.name} isShown={isShown}, preCheckIsShown={preCheckResult.IsShown}");
#endif

            if(isShown != preCheckResult.IsShown)
            {
                result.style.display = preCheckResult.IsShown ? DisplayStyle.Flex : DisplayStyle.None;
            }

            return preCheckResult;
        }
#endif
        public abstract void Render();
        public abstract float GetHeight();

        public abstract void RenderPosition(Rect position);

        // NA: NaughtyEditorGUI
        protected static object FieldLayout(object value, string label, Type type=null, bool disabled=true)
        {
            using (new EditorGUI.DisabledScope(disabled))
            {
                if (type == null && value == null)
                {
                    Rect rt = GUILayoutUtility.GetRect(new GUIContent(label), EditorStyles.label);
                    EditorGUI.DrawRect(new Rect(rt)
                    {
                        x = rt.x + EditorGUIUtility.labelWidth,
                        width = rt.width - EditorGUIUtility.labelWidth,
                    }, Color.yellow * new Color(1, 1,1, 0.2f));
                    EditorGUI.LabelField(rt, label, "null", EditorStyles.label);
                    return null;
                }

                // bool isDrawn = true;
                Type valueType = type ?? value.GetType();

                if (valueType == typeof(bool))
                {
                    return EditorGUILayout.Toggle(label, (bool)value);
                }

                if (valueType == typeof(short))
                {
                    return EditorGUILayout.IntField(label, (short)value);
                }
                if (valueType == typeof(ushort))
                {
                    return EditorGUILayout.IntField(label, (ushort)value);
                }
                if (valueType == typeof(int))
                {
                    return EditorGUILayout.IntField(label, (int)value);
                }
                if (valueType == typeof(uint))
                {
                    return EditorGUILayout.LongField(label, (uint)value);
                }
                if (valueType == typeof(long))
                {
                    return EditorGUILayout.LongField(label, (long)value);
                }
                if (valueType == typeof(ulong))
                {
                    return EditorGUILayout.TextField(label, ((ulong)value).ToString());
                }
                if (valueType == typeof(float))
                {
                    return EditorGUILayout.FloatField(label, (float)value);
                }
                if (valueType == typeof(double))
                {
                    return EditorGUILayout.DoubleField(label, (double)value);
                }
                if (valueType == typeof(string))
                {
                    return EditorGUILayout.TextField(label, (string)value);
                }
                if (valueType == typeof(Vector2))
                {
                    return EditorGUILayout.Vector2Field(label, (Vector2)value);
                }
                if (valueType == typeof(Vector3))
                {
                    return EditorGUILayout.Vector3Field(label, (Vector3)value);
                }
                if (valueType == typeof(Vector4))
                {
                    return EditorGUILayout.Vector4Field(label, (Vector4)value);
                }
                if (valueType == typeof(Vector2Int))
                {
                    return EditorGUILayout.Vector2IntField(label, (Vector2Int)value);
                }
                if (valueType == typeof(Vector3Int))
                {
                    return EditorGUILayout.Vector3IntField(label, (Vector3Int)value);
                }
                if (valueType == typeof(Color))
                {
                    return EditorGUILayout.ColorField(label, (Color)value);
                }
                if (valueType == typeof(Bounds))
                {
                    return EditorGUILayout.BoundsField(label, (Bounds)value);
                }
                if (valueType == typeof(Rect))
                {
                    return EditorGUILayout.RectField(label, (Rect)value);
                }
                if (valueType == typeof(RectInt))
                {
                    return EditorGUILayout.RectIntField(label, (RectInt)value);
                }
                if (typeof(UnityEngine.Object).IsAssignableFrom(valueType))
                {
                    return EditorGUILayout.ObjectField(label, (UnityEngine.Object)value, valueType, true);
                }
                if (valueType.BaseType == typeof(Enum))
                {
                    return EditorGUILayout.EnumPopup(label, (Enum)value);
                }
                if (valueType.BaseType == typeof(TypeInfo))
                {
                    return EditorGUILayout.TextField(label, value.ToString());
                }
                if (value is IEnumerable enumerableValue)
                {
                    (object value, int index)[] valueIndexed = enumerableValue.Cast<object>().WithIndex().ToArray();

                    // using(new EditorGUILayout.VerticalScope(GUI.skin.box))
                    // {
                    Rect labelRect = EditorGUILayout.GetControlRect();
                    EditorGUI.LabelField(labelRect, label);

                    float numWidth = Mathf.Max(30,
                        EditorStyles.textField.CalcSize(new GUIContent($"{valueIndexed.Length}")).x);

                    Rect numRect = new Rect(labelRect)
                    {
                        width = numWidth,
                        x = labelRect.x + labelRect.width - numWidth,
                    };

                    EditorGUI.IntField(numRect, valueIndexed.Length);
                    using (new EditorGUI.IndentLevelScope())
                    {
                        List<object> listResult = new List<object>();
                        foreach ((object item, int index) in valueIndexed)
                        {
                            object itemValue = FieldLayout(item, $"Element {index}", item.GetType(), disabled);
                            listResult.Add(itemValue);
                        }

                        return listResult;
                    }
                    // }
                }
                EditorGUILayout.HelpBox($"Type not supported: {valueType}", MessageType.Warning);
                return null;

                // return isDrawn;
            }
        }

        protected static float FieldHeight(object value, string label)
        {
            if (value == null)
            {
                return EditorGUIUtility.singleLineHeight;
            }
            Type valueType = value.GetType();

            if (valueType == typeof(bool)
                || valueType == typeof(short)
                || valueType == typeof(ushort)
                || valueType == typeof(int)
                || valueType == typeof(uint)
                || valueType == typeof(long)
                || valueType == typeof(ulong)
                || valueType == typeof(float)
                || valueType == typeof(double)
                || valueType == typeof(string)
                || valueType == typeof(Vector2)
                || valueType == typeof(Vector3)
                || valueType == typeof(Vector4)
                || valueType == typeof(Vector2Int)
                || valueType == typeof(Vector3Int)
                || valueType == typeof(Color)
                || valueType == typeof(Bounds)
                || valueType == typeof(Rect)
                || valueType == typeof(RectInt)
                || typeof(UnityEngine.Object).IsAssignableFrom(valueType)
                || valueType.BaseType == typeof(Enum)
                || valueType.BaseType == typeof(TypeInfo))
            {
                return EditorGUIUtility.singleLineHeight;
            }

            if (value is IEnumerable enumerableValue)
            {
                return EditorGUIUtility.singleLineHeight + enumerableValue.Cast<object>().Select((each, index) => FieldHeight(each, $"Element {index}")).Sum();
            }

            return ImGuiHelpBox.GetHeight($"Type not supported: {valueType}", EditorGUIUtility.currentViewWidth, MessageType.Warning);
        }

        protected static object FieldPosition(Rect position, object value, string label, Type type=null, bool disabled=true)
        {
            using (new EditorGUI.DisabledScope(disabled))
            {
                if (type == null && value == null)
                {
                    Rect rt = position;
                    EditorGUI.DrawRect(new Rect(rt)
                    {
                        x = rt.x + EditorGUIUtility.labelWidth,
                        width = rt.width - EditorGUIUtility.labelWidth,
                    }, Color.yellow * new Color(1, 1,1, 0.2f));
                    EditorGUI.LabelField(rt, label, "null", EditorStyles.label);
                    return null;
                }

                // bool isDrawn = true;
                Type valueType = type ?? value.GetType();

                if (valueType == typeof(bool))
                {
                    return EditorGUI.Toggle(position, label, (bool)value);
                }

                if (valueType == typeof(short))
                {
                    return EditorGUI.IntField(position, label, (short)value);
                }
                if (valueType == typeof(ushort))
                {
                    return EditorGUI.IntField(position, label, (ushort)value);
                }
                if (valueType == typeof(int))
                {
                    return EditorGUI.IntField(position, label, (int)value);
                }
                if (valueType == typeof(uint))
                {
                    return EditorGUI.LongField(position, label, (uint)value);
                }
                if (valueType == typeof(long))
                {
                    return EditorGUI.LongField(position, label, (long)value);
                }
                if (valueType == typeof(ulong))
                {
                    return EditorGUI.TextField(position, label, ((ulong)value).ToString());
                }
                if (valueType == typeof(float))
                {
                    return EditorGUI.FloatField(position, label, (float)value);
                }
                if (valueType == typeof(double))
                {
                    return EditorGUI.DoubleField(position, label, (double)value);
                }
                if (valueType == typeof(string))
                {
                    return EditorGUI.TextField(position, label, (string)value);
                }
                if (valueType == typeof(Vector2))
                {
                    return EditorGUI.Vector2Field(position, label, (Vector2)value);
                }
                if (valueType == typeof(Vector3))
                {
                    return EditorGUI.Vector3Field(position, label, (Vector3)value);
                }
                if (valueType == typeof(Vector4))
                {
                    return EditorGUI.Vector4Field(position, label, (Vector4)value);
                }
                if (valueType == typeof(Vector2Int))
                {
                    return EditorGUI.Vector2IntField(position, label, (Vector2Int)value);
                }
                if (valueType == typeof(Vector3Int))
                {
                    return EditorGUI.Vector3IntField(position, label, (Vector3Int)value);
                }
                if (valueType == typeof(Color))
                {
                    return EditorGUI.ColorField(position, label, (Color)value);
                }
                if (valueType == typeof(Bounds))
                {
                    return EditorGUI.BoundsField(position, label, (Bounds)value);
                }
                if (valueType == typeof(Rect))
                {
                    return EditorGUI.RectField(position, label, (Rect)value);
                }
                if (valueType == typeof(RectInt))
                {
                    return EditorGUI.RectIntField(position, label, (RectInt)value);
                }
                if (typeof(UnityEngine.Object).IsAssignableFrom(valueType))
                {
                    return EditorGUI.ObjectField(position, label, (UnityEngine.Object)value, valueType, true);
                }
                if (valueType.BaseType == typeof(Enum))
                {
                    return EditorGUI.EnumPopup(position, label, (Enum)value);
                }
                if (valueType.BaseType == typeof(TypeInfo))
                {
                    return EditorGUI.TextField(position, label, value.ToString());
                }
                if (value is IEnumerable enumerableValue)
                {
                    (object value, int index)[] valueIndexed = enumerableValue.Cast<object>().WithIndex().ToArray();

                    (Rect labelRect, Rect listRect) = RectUtils.SplitHeightRect(position, EditorGUIUtility.singleLineHeight);
                    EditorGUI.LabelField(labelRect, label);

                    float numWidth = Mathf.Max(30,
                        EditorStyles.textField.CalcSize(new GUIContent($"{valueIndexed.Length}")).x);

                    Rect numRect = new Rect(labelRect)
                    {
                        width = numWidth,
                        x = labelRect.x + labelRect.width - numWidth,
                    };

                    EditorGUI.IntField(numRect, valueIndexed.Length);

                    GUI.Box(listRect, GUIContent.none);

                    float lisAccY = 0;
                    using(new EditorGUI.IndentLevelScope())
                    {
                        List<object> results = new List<object>();
                        foreach ((object item, int index) in valueIndexed)
                        {
                            string thisLabel = $"Element {index}";
                            float thisHeight = FieldHeight(item, thisLabel);
                            Rect thisRect = new Rect(listRect)
                            {
                                y = listRect.y + lisAccY,
                                height = thisHeight,
                            };
                            lisAccY += thisHeight;

                            results.Add(FieldPosition(thisRect, item, thisLabel));
                        }

                        return results;
                    }

                }
                EditorGUI.HelpBox(position, $"Type not supported: {valueType}", MessageType.Warning);
                return null;

                // return isDrawn;
            }
        }

#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
        private static StyleSheet nullUss;

        protected static VisualElement UIToolkitLayout(object value, string label, Type type=null)
        {
            if (type == null && value == null)
            {
                TextField textField = new TextField(label)
                {
                    value = "null",
                    // style =
                    // {
                    //     backgroundColor = Color.yellow * new Color(1, 1, 1, 0.2f),
                    // },
                    pickingMode = PickingMode.Ignore,
                };

                if(nullUss == null)
                {
                    nullUss = Util.LoadResource<StyleSheet>("UIToolkit/UnityTextInputElementWarning.uss");
                }
                textField.styleSheets.Add(nullUss);

                textField.SetEnabled(false);
                return textField;
            }

            VisualElement visualElement;
            Type valueType = type ?? value.GetType();

            if (valueType == typeof(bool))
            {
                visualElement = new Toggle(label)
                {
                    value = (bool)value,
                };
            }
            else if (valueType == typeof(short))
            {
                // EditorGUILayout.IntField(label, (short)value);
                visualElement = new IntegerField(label)
                {
                    value = (short)value,
                };
            }
            else if (valueType == typeof(ushort))
            {
                // EditorGUILayout.IntField(label, (ushort)value);
                visualElement = new IntegerField(label)
                {
                    value = (ushort)value,
                };
            }
            else if (valueType == typeof(int))
            {
                // EditorGUILayout.IntField(label, (int)value);
                visualElement = new IntegerField(label)
                {
                    value = (int)value,
                };
            }
            else if (valueType == typeof(uint))
            {
                // EditorGUILayout.LongField(label, (uint)value);
                visualElement = new LongField(label)
                {
                    value = (uint)value,
                };
            }
            else if (valueType == typeof(long))
            {
                // EditorGUILayout.LongField(label, (long)value);
                visualElement = new LongField(label)
                {
                    value = (long)value,
                };
            }
            else if (valueType == typeof(ulong))
            {
                // EditorGUILayout.TextField(label, ((ulong)value).ToString());
                visualElement = new TextField(label)
                {
                    value = ((ulong)value).ToString(),
                };
            }
            else if (valueType == typeof(float))
            {
                // EditorGUILayout.FloatField(label, (float)value);
                visualElement = new FloatField(label)
                {
                    value = (float)value,
                };
            }
            else if (valueType == typeof(double))
            {
                // EditorGUILayout.DoubleField(label, (double)value);
                visualElement = new DoubleField(label)
                {
                    value = (double)value,
                };
            }
            else if (valueType == typeof(string))
            {
                // EditorGUILayout.TextField(label, (string)value);
                visualElement = new TextField(label)
                {
                    value = (string)value,
                };
            }
            else if (valueType == typeof(Vector2))
            {
                // EditorGUILayout.Vector2Field(label, (Vector2)value);
                visualElement = new Vector2Field(label)
                {
                    value = (Vector2)value,
                };
            }
            else if (valueType == typeof(Vector3))
            {
                // EditorGUILayout.Vector3Field(label, (Vector3)value);
                visualElement = new Vector3Field(label)
                {
                    value = (Vector3)value,
                };
            }
            else if (valueType == typeof(Vector4))
            {
                // EditorGUILayout.Vector4Field(label, (Vector4)value);
                visualElement = new Vector4Field(label)
                {
                    value = (Vector4)value,
                };
            }
            else if (valueType == typeof(Vector2Int))
            {
                // EditorGUILayout.Vector2IntField(label, (Vector2Int)value);
                visualElement = new Vector2IntField(label)
                {
                    value = (Vector2Int)value,
                };
            }
            else if (valueType == typeof(Vector3Int))
            {
                // EditorGUILayout.Vector3IntField(label, (Vector3Int)value);
                visualElement = new Vector3IntField(label)
                {
                    value = (Vector3Int)value,
                };
            }
            else if (valueType == typeof(Color))
            {
                // EditorGUILayout.ColorField(label, (Color)value);
                visualElement = new ColorField(label)
                {
                    value = (Color)value,
                };
            }
            else if (valueType == typeof(Bounds))
            {
                // EditorGUILayout.BoundsField(label, (Bounds)value);
                visualElement = new BoundsField(label)
                {
                    value = (Bounds)value,
                };
            }
            else if (valueType == typeof(Rect))
            {
                // EditorGUILayout.RectField(label, (Rect)value);
                visualElement = new RectField(label)
                {
                    value = (Rect)value,
                };
            }
            else if (valueType == typeof(RectInt))
            {
                // EditorGUILayout.RectIntField(label, (RectInt)value);
                visualElement = new RectIntField(label)
                {
                    value = (RectInt)value,
                };
            }
            else if (typeof(UnityEngine.Object).IsAssignableFrom(valueType))
            {
                // EditorGUILayout.ObjectField(label, (UnityEngine.Object)value, valueType, true);
                visualElement = new ObjectField(label)
                {
                    value = (UnityEngine.Object)value,
                    objectType = valueType,
                };
            }
            else if (valueType.BaseType == typeof(Enum))
            {
                visualElement = new EnumField((Enum)value)
                {
                    label = label,
                    value = (Enum)value,
                };
            }
            else if (valueType.BaseType == typeof(TypeInfo))
            {
                // EditorGUILayout.TextField(label, value.ToString());
                visualElement = new TextField(label)
                {
                    value = value.ToString(),
                };
            }
            else if (value is IEnumerable enumerableValue)
            {
                // List<object> values = enumerableValue.Cast<object>().ToList();
                // Debug.Log($"!!!!!!!!!{value}/{valueType}/{valueType.IsArray}/{valueType.BaseType}");
                // return new ListView(((IEnumerable<object>)enumerableValue).ToList());
                VisualElement root = new VisualElement();

                Foldout foldout = new Foldout
                {
                    text = label,
                };

                // this is sooooo buggy.
                // ListView listView = new ListView(
                //     values,
                //     -1f,
                //     () => new VisualElement(),
                //     (element, index) => element.Add(UIToolkitLayout(values[index], $"Element {index}")))
                // {
                //     showBorder = true,
                //     showBoundCollectionSize  = true,
                // };
                VisualElement listView = new VisualElement
                {
                    style =
                    {
                        backgroundColor = new Color(64f/255, 64f/255, 64f/255, 1f),

                        borderTopWidth = 1,
                        borderLeftWidth = 1,
                        borderRightWidth = 1,
                        borderBottomWidth = 1,
                        borderTopLeftRadius = 3,
                        borderTopRightRadius = 3,
                        borderBottomLeftRadius = 3,
                        borderBottomRightRadius = 3,
                        borderLeftColor = EColor.MidnightAsh.GetColor(),
                        borderRightColor = EColor.MidnightAsh.GetColor(),
                        borderTopColor = EColor.MidnightAsh.GetColor(),
                        borderBottomColor = EColor.MidnightAsh.GetColor(),

                        paddingTop = 2,
                        paddingBottom = 2,
                        paddingLeft = 2,
                        paddingRight = 2,
                    },
                };

                foreach ((object item, int index) in enumerableValue.Cast<object>().WithIndex())
                {
                    VisualElement child = UIToolkitLayout(item, $"Element {index}");
                    listView.Add(child);
                }

                listView.SetEnabled(false);

                foldout.RegisterValueChangedCallback(evt =>
                {
                    listView.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None;
                });

                root.Add(foldout);
                root.Add(listView);

                return root;
            }
            else
            {
// #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_SHOW_IN_INSPECTOR
//                 Debug.Log($"IEnumerable={value is IEnumerable}");
// #endif
                // isDrawn = false;
                visualElement = new HelpBox($"Unable to draw type {valueType}", HelpBoxMessageType.Error);
            }

            visualElement.SetEnabled(false);
            visualElement.AddToClassList("unity-base-field__aligned");
            return visualElement;
        }
#endif
    }
}

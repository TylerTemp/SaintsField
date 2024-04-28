using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        protected readonly SerializedObject SerializedObject;
        // ReSharper enable InconsistentNaming

        protected struct PreCheckResult
        {
            public bool IsShown;
            public bool IsDisabled;
            public int ArraySize;
            public bool HasRichLabel;
            public string RichLabelXml;
        }

        protected AbsRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo)
        {
            FieldWithInfo = fieldWithInfo;
            SerializedObject = serializedObject;
        }

        private enum PreCheckInternalType
        {
            ShowHide,  // main
            DisableEnable,  // main
        }

        private class PreCheckInternalInfo
        {
            public PreCheckInternalType Type;
            public bool Reverse;
            public string[] Callbacks;
            public EMode EditorMode;
            public bool SafeResult;
            public bool EmptyResult;
            public object Target;

            public bool result;
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
                            Type = PreCheckInternalType.ShowHide,
                            Reverse = true,
                            Callbacks = hideIfAttribute.Callbacks,
                            EditorMode = hideIfAttribute.EditorMode,
                            SafeResult = true,
                            EmptyResult = false,
                            Target = fieldWithInfo.Target,
                        });
                        break;
                    case PlayaShowIfAttribute showIfAttribute:
                        preCheckInternalInfos.Add(new PreCheckInternalInfo
                        {
                            Type = PreCheckInternalType.ShowHide,
                            Reverse = false,
                            Callbacks = showIfAttribute.Callbacks,
                            EditorMode = showIfAttribute.EditorMode,
                            SafeResult = true,
                            EmptyResult = true,
                            Target = fieldWithInfo.Target,
                        });
                        break;
                    case PlayaEnableIfAttribute enableIfAttribute:
                        preCheckInternalInfos.Add(new PreCheckInternalInfo
                        {
                            Type = PreCheckInternalType.DisableEnable,
                            Reverse = true,
                            Callbacks = enableIfAttribute.Callbacks,
                            EditorMode = enableIfAttribute.EditorMode,
                            SafeResult = false,
                            EmptyResult = false,
                            Target = fieldWithInfo.Target,
                        });
                        break;
                    case PlayaDisableIfAttribute disableIfAttribute:
                        preCheckInternalInfos.Add(new PreCheckInternalInfo
                        {
                            Type = PreCheckInternalType.DisableEnable,
                            Reverse = false,
                            Callbacks = disableIfAttribute.Callbacks,
                            EditorMode = disableIfAttribute.EditorMode,
                            SafeResult = false,
                            EmptyResult = true,
                            Target = fieldWithInfo.Target,
                        });
                        break;
                    case PlayaArraySizeAttribute arraySizeAttribute:
                        arraySize = arraySizeAttribute.Size;
                        break;
                }
            }

            foreach (PreCheckInternalInfo preCheckInternalInfo in preCheckInternalInfos)
            {
                FillResult(preCheckInternalInfo);
            }

            // showIf
            IReadOnlyList<bool> showIfResults = preCheckInternalInfos
                .Where(each => each.Type == PreCheckInternalType.ShowHide)
                .Select(each => each.result)
                .ToList();
            bool showIfResult = showIfResults.Count == 0 || showIfResults.Any(each => each);

            // disableIf
            IReadOnlyList<bool> disableIfResults = preCheckInternalInfos
                .Where(each => each.Type == PreCheckInternalType.DisableEnable)
                .Select(each => each.result)
                .ToList();
            bool disableIfResult = disableIfResults.Count != 0 && disableIfResults.Any(each => each);
            // Debug.Log($"disableIfResult={disableIfResult}/{disableIfResults.Count != 0}/{disableIfResults.Any(each => each)}/results={string.Join(",", disableIfResults)}");

            PlayaRichLabelAttribute richLabelAttribute = fieldWithInfo.PlayaAttributes.OfType<PlayaRichLabelAttribute>().FirstOrDefault();
            // Debug.Log($"richLabelAttribute={richLabelAttribute}");
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

        private static void FillResult(PreCheckInternalInfo preCheckInternalInfo)
        {
            EMode editorMode = preCheckInternalInfo.EditorMode;
            bool editorRequiresEdit = editorMode.HasFlag(EMode.Edit);
            bool editorRequiresPlay = editorMode.HasFlag(EMode.Play);

            bool editorModeIsTrue = (
                !editorRequiresEdit || !EditorApplication.isPlaying
            ) && (
                !editorRequiresPlay || EditorApplication.isPlaying
            );

            string[] bys = preCheckInternalInfo.Callbacks;
            if (bys.Length == 0 && editorRequiresEdit && editorRequiresPlay)
            {
                preCheckInternalInfo.result = preCheckInternalInfo.EmptyResult;
                // Debug.Log($"return {preCheckInternalInfo.SafeResult}");
                return;
            }

            List<bool> callbackTruly = new List<bool>();
            if(!(editorRequiresEdit && editorRequiresPlay))
            {
                callbackTruly.Add(editorModeIsTrue);
            }

            List<string> errors = new List<string>();

            // Type targetType = preCheckInternalInfo.Target.GetType();
            foreach (string callback in bys)
            {
                (string error, bool isTruly) = Util.GetTruly(preCheckInternalInfo.Target, callback);
                if (error != "")
                {
                    errors.Add(error);
                }
                callbackTruly.Add(isTruly);
            }

            if (errors.Count > 0)
            {
                // return (string.Join("\n\n", errors), false);
                preCheckInternalInfo.result = preCheckInternalInfo.SafeResult;
                return;
            }

            if (callbackTruly.Count == 0)
            {
                preCheckInternalInfo.result = preCheckInternalInfo.EmptyResult;
                return;
            }

            if (preCheckInternalInfo.Reverse)  // !or
            {
                preCheckInternalInfo.result = !callbackTruly.Any(each => each);
            }
            else  // and
            {
                preCheckInternalInfo.result = callbackTruly.All(each => each);
            }
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
        protected static void FieldLayout(object value, string label)
        {
            using (new EditorGUI.DisabledScope(true))
            {
                if (value == null)
                {
                    Rect rt = GUILayoutUtility.GetRect(new GUIContent(label), EditorStyles.label);
                    EditorGUI.DrawRect(new Rect(rt)
                    {
                        x = rt.x + EditorGUIUtility.labelWidth,
                        width = rt.width - EditorGUIUtility.labelWidth,
                    }, Color.yellow * new Color(1, 1,1, 0.2f));
                    EditorGUI.LabelField(rt, label, "null", EditorStyles.label);
                    return;
                }

                // bool isDrawn = true;
                Type valueType = value.GetType();

                if (valueType == typeof(bool))
                {
                    EditorGUILayout.Toggle(label, (bool)value);
                }
                else if (valueType == typeof(short))
                {
                    EditorGUILayout.IntField(label, (short)value);
                }
                else if (valueType == typeof(ushort))
                {
                    EditorGUILayout.IntField(label, (ushort)value);
                }
                else if (valueType == typeof(int))
                {
                    EditorGUILayout.IntField(label, (int)value);
                }
                else if (valueType == typeof(uint))
                {
                    EditorGUILayout.LongField(label, (uint)value);
                }
                else if (valueType == typeof(long))
                {
                    EditorGUILayout.LongField(label, (long)value);
                }
                else if (valueType == typeof(ulong))
                {
                    EditorGUILayout.TextField(label, ((ulong)value).ToString());
                }
                else if (valueType == typeof(float))
                {
                    EditorGUILayout.FloatField(label, (float)value);
                }
                else if (valueType == typeof(double))
                {
                    EditorGUILayout.DoubleField(label, (double)value);
                }
                else if (valueType == typeof(string))
                {
                    EditorGUILayout.TextField(label, (string)value);
                }
                else if (valueType == typeof(Vector2))
                {
                    EditorGUILayout.Vector2Field(label, (Vector2)value);
                }
                else if (valueType == typeof(Vector3))
                {
                    EditorGUILayout.Vector3Field(label, (Vector3)value);
                }
                else if (valueType == typeof(Vector4))
                {
                    EditorGUILayout.Vector4Field(label, (Vector4)value);
                }
                else if (valueType == typeof(Vector2Int))
                {
                    EditorGUILayout.Vector2IntField(label, (Vector2Int)value);
                }
                else if (valueType == typeof(Vector3Int))
                {
                    EditorGUILayout.Vector3IntField(label, (Vector3Int)value);
                }
                else if (valueType == typeof(Color))
                {
                    EditorGUILayout.ColorField(label, (Color)value);
                }
                else if (valueType == typeof(Bounds))
                {
                    EditorGUILayout.BoundsField(label, (Bounds)value);
                }
                else if (valueType == typeof(Rect))
                {
                    EditorGUILayout.RectField(label, (Rect)value);
                }
                else if (valueType == typeof(RectInt))
                {
                    EditorGUILayout.RectIntField(label, (RectInt)value);
                }
                else if (typeof(UnityEngine.Object).IsAssignableFrom(valueType))
                {
                    EditorGUILayout.ObjectField(label, (UnityEngine.Object)value, valueType, true);
                }
                else if (valueType.BaseType == typeof(Enum))
                {
                    EditorGUILayout.EnumPopup(label, (Enum)value);
                }
                else if (valueType.BaseType == typeof(TypeInfo))
                {
                    EditorGUILayout.TextField(label, value.ToString());
                }
                else
                {
                    EditorGUILayout.HelpBox($"Type not supported: {valueType}", MessageType.Warning);
                }

                // return isDrawn;
            }
        }

        protected static void FieldPosition(Rect position, object value, string label)
        {
            using (new EditorGUI.DisabledScope(true))
            {
                if (value == null)
                {
                    Rect rt = position;
                    EditorGUI.DrawRect(new Rect(rt)
                    {
                        x = rt.x + EditorGUIUtility.labelWidth,
                        width = rt.width - EditorGUIUtility.labelWidth,
                    }, Color.yellow * new Color(1, 1,1, 0.2f));
                    EditorGUI.LabelField(rt, label, "null", EditorStyles.label);
                    return;
                }

                // bool isDrawn = true;
                Type valueType = value.GetType();

                if (valueType == typeof(bool))
                {
                    EditorGUI.Toggle(position, label, (bool)value);
                }
                else if (valueType == typeof(short))
                {
                    EditorGUI.IntField(position, label, (short)value);
                }
                else if (valueType == typeof(ushort))
                {
                    EditorGUI.IntField(position, label, (ushort)value);
                }
                else if (valueType == typeof(int))
                {
                    EditorGUI.IntField(position, label, (int)value);
                }
                else if (valueType == typeof(uint))
                {
                    EditorGUI.LongField(position, label, (uint)value);
                }
                else if (valueType == typeof(long))
                {
                    EditorGUI.LongField(position, label, (long)value);
                }
                else if (valueType == typeof(ulong))
                {
                    EditorGUI.TextField(position, label, ((ulong)value).ToString());
                }
                else if (valueType == typeof(float))
                {
                    EditorGUI.FloatField(position, label, (float)value);
                }
                else if (valueType == typeof(double))
                {
                    EditorGUI.DoubleField(position, label, (double)value);
                }
                else if (valueType == typeof(string))
                {
                    EditorGUI.TextField(position, label, (string)value);
                }
                else if (valueType == typeof(Vector2))
                {
                    EditorGUI.Vector2Field(position, label, (Vector2)value);
                }
                else if (valueType == typeof(Vector3))
                {
                    EditorGUI.Vector3Field(position, label, (Vector3)value);
                }
                else if (valueType == typeof(Vector4))
                {
                    EditorGUI.Vector4Field(position, label, (Vector4)value);
                }
                else if (valueType == typeof(Vector2Int))
                {
                    EditorGUI.Vector2IntField(position, label, (Vector2Int)value);
                }
                else if (valueType == typeof(Vector3Int))
                {
                    EditorGUI.Vector3IntField(position, label, (Vector3Int)value);
                }
                else if (valueType == typeof(Color))
                {
                    EditorGUI.ColorField(position, label, (Color)value);
                }
                else if (valueType == typeof(Bounds))
                {
                    EditorGUI.BoundsField(position, label, (Bounds)value);
                }
                else if (valueType == typeof(Rect))
                {
                    EditorGUI.RectField(position, label, (Rect)value);
                }
                else if (valueType == typeof(RectInt))
                {
                    EditorGUI.RectIntField(position, label, (RectInt)value);
                }
                else if (typeof(UnityEngine.Object).IsAssignableFrom(valueType))
                {
                    EditorGUI.ObjectField(position, label, (UnityEngine.Object)value, valueType, true);
                }
                else if (valueType.BaseType == typeof(Enum))
                {
                    EditorGUI.EnumPopup(position, label, (Enum)value);
                }
                else if (valueType.BaseType == typeof(System.Reflection.TypeInfo))
                {
                    EditorGUI.TextField(position, label, value.ToString());
                }
                else
                {
                    EditorGUI.HelpBox(position, $"Type not supported: {valueType}", MessageType.Warning);
                }

                // return isDrawn;
            }
        }

#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
        private static StyleSheet nullUss;

        protected static VisualElement UIToolkitLayout(object value, string label)
        {
            if (value == null)
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
            Type valueType = value.GetType();

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
            else
            {
                // isDrawn = false;
                visualElement = new HelpBox($"Unable to draw type {valueType}", HelpBoxMessageType.Error);
            }

            visualElement.SetEnabled(false);
            return visualElement;
        }
#endif
    }
}

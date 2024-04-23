using System;
using System.Collections.Generic;
using System.Linq;
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
        // ReSharper disable once MemberCanBePrivate.Global
        protected readonly bool TryFixUIToolkit;
        // ReSharper enable InconsistentNaming

        protected struct PreCheckResult
        {
            public bool IsShown;
            public bool IsDisabled;
            public int ArraySize;
        }

        protected AbsRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo, bool tryFixUIToolkit=false)
        {
            FieldWithInfo = fieldWithInfo;
            SerializedObject = serializedObject;
            TryFixUIToolkit = tryFixUIToolkit;
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
            return new PreCheckResult
            {
                IsDisabled = disableIfResult,
                IsShown = showIfResult,
                ArraySize = arraySize,
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
                else if (valueType.BaseType == typeof(System.Reflection.TypeInfo))
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
            // TODO: need a way to monitor if the value changed, for auto-property.
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
                // EditorGUILayout.EnumPopup(label, (Enum)value);
                visualElement = new EnumField()
                {
                    value = (Enum)value,
                };
            }
            else if (valueType.BaseType == typeof(System.Reflection.TypeInfo))
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

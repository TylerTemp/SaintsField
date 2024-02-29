using System;
using SaintsField.Editor.Utils;
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

        protected AbsRenderer(UnityEditor.Editor editor, SaintsFieldWithInfo fieldWithInfo, bool tryFixUIToolkit=false)
        {
            FieldWithInfo = fieldWithInfo;
            SerializedObject = editor.serializedObject;
            TryFixUIToolkit = tryFixUIToolkit;
        }

#if UNITY_2022_2_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
        public abstract VisualElement CreateVisualElement();
#endif
        public abstract void Render();

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

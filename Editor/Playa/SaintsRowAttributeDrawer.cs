using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using UnityEngine.UIElements;
using SaintsField.Playa;
using SaintsField.Editor.Playa.Renderer;
#endif

namespace SaintsField.Editor.Playa
{
    [CustomPropertyDrawer(typeof(SaintsRowAttribute))]
    public class SaintsRowAttributeDrawer: PropertyDrawer, IDOTweenPlayRecorder
    {
        private static (string error, int arrayIndex, object parent, object current) GetTargets(FieldInfo fieldInfo, SerializedProperty property)
        {
            object parentValue = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;

            (string error, int index, object value) = Util.GetValue(property, fieldInfo, parentValue);
            if (error != "")
            {
                return (error, -1, parentValue, null);
            }

            return ("", index, parentValue, value);
        }

        public static IEnumerable<(string name, SerializedProperty property)> GetSerializableFieldInfo(SerializedProperty property)
        {
            HashSet<string> alreadySend = new HashSet<string>();
            SerializedProperty it = property.Copy();
            // or Next, also, the bool argument specifies whether to enter on children or not
            while (it.NextVisible(true))
            {
                // ReSharper disable once InvertIf
                if (alreadySend.Add(it.name))
                {
                    SerializedProperty relProperty = property.FindPropertyRelative(it.name);
                    // Debug.Log($"prop={it.name}/relProp={relProperty}");
                    if(relProperty != null)
                    {
                        yield return (it.name, relProperty);
                    }
                }
            }
        }

        #region IMGUI

        // private IReadOnlyList<ISaintsRenderer> _imGuiRenderers;
        private readonly Dictionary<int, IReadOnlyList<ISaintsRenderer>> _imGuiRenderers = new Dictionary<int, IReadOnlyList<ISaintsRenderer>>();
        private float _filedWidthCache = -1;

        private IEnumerable<ISaintsRenderer> ImGuiEnsureRenderers(SerializedProperty property)
        {
#if UNITY_2021_3_OR_NEWER
            if(property.propertyType == SerializedPropertyType.ManagedReference && property.managedReferenceValue == null)
            {
                return Array.Empty<ISaintsRenderer>();
            }
#endif

            // string key = $"{property.serializedObject.targetObject.GetInstanceID()}:{property.propertyPath}";
            // if (GlobalCache.TryGetValue(key, out IReadOnlyList<ISaintsRenderer> result))
            // {
            //     return result;
            // }
            //
            // (object _, object current) = GetTargets(fieldInfo, property);
            // Dictionary<string, SerializedProperty> serializedFieldNames = GetSerializableFieldInfo(property).ToDictionary(each => each.name, each => each.property);
            // return GlobalCache[key] = SaintsEditor.GetRenderers(false, serializedFieldNames, property.serializedObject, current);
            int arrayIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);

            if(_imGuiRenderers.TryGetValue(arrayIndex, out IReadOnlyList<ISaintsRenderer> result))
            {
                return result;
            }

            // Debug.Log($"create new for {property.propertyPath}");
            (string error, int index, object _, object current) = GetTargets(fieldInfo, property);
            if(error == "" )
            {
                Dictionary<string, SerializedProperty> serializedFieldNames = GetSerializableFieldInfo(property)
                    .ToDictionary(each => each.name, each => each.property);
                return _imGuiRenderers[index] =
                    SaintsEditor.GetRenderers(serializedFieldNames, property.serializedObject, current);
            }

            Debug.LogWarning(error);
            return Array.Empty<ISaintsRenderer>();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
#if UNITY_2021_3_OR_NEWER
            if(property.propertyType == SerializedPropertyType.ManagedReference && property.managedReferenceValue == null)
            {
                return EditorGUIUtility.singleLineHeight;
            }
#endif

            float fullWidth = _filedWidthCache <= 0
                ? EditorGUIUtility.currentViewWidth - EditorGUI.indentLevel * 15
                : _filedWidthCache;

            SaintsRowAttribute saintsRowAttribute = (SaintsRowAttribute) attribute;
            float baseLineHeight = saintsRowAttribute.Inline ? 0 : SaintsPropertyDrawer.SingleLineHeight;
            float fieldHeight = 0f;
            // ReSharper disable once InvertIf
            if(property.isExpanded || saintsRowAttribute.Inline)
            {
                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (ISaintsRenderer saintsRenderer in ImGuiEnsureRenderers(property))
                {
                    fieldHeight += saintsRenderer.GetHeightIMGUI(fullWidth);
                }
            }

            return baseLineHeight + fieldHeight;
        }

        // private bool _testToggle;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {

            if(position.width - 1 > Mathf.Epsilon)
            {
                _filedWidthCache = position.width;
            }
            // Debug.Log(property.propertyPath);
            SaintsRowAttribute saintsRowAttribute = (SaintsRowAttribute) attribute;
            using (new EditorGUI.PropertyScope(position, label, property))
            {
                if(!saintsRowAttribute.Inline)
                {
                    // Debug.Log($"render foldout {position.x}, {position.y}");
                    property.isExpanded = EditorGUI.Foldout(new Rect(position)
                    {
                        height = SaintsPropertyDrawer.SingleLineHeight,
                    }, property.isExpanded, label, true);
                }

                if (!saintsRowAttribute.Inline && !property.isExpanded)
                {
                    return;
                }

                Rect leftRect = saintsRowAttribute.Inline
                    ? new Rect(position)
                    : new Rect(position)
                    {
                        x = position.x + SaintsPropertyDrawer.IndentWidth,
                        y = position.y + SaintsPropertyDrawer.SingleLineHeight,
                        height = position.height - SaintsPropertyDrawer.SingleLineHeight,
                        width = position.width - SaintsPropertyDrawer.IndentWidth,
                    };

                float yAcc = leftRect.y;

                foreach (ISaintsRenderer saintsRenderer in ImGuiEnsureRenderers(property))
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTSROW
                    Debug.Log($"saintsRow: {saintsRenderer}");
#endif
                    float height = saintsRenderer.GetHeightIMGUI(position.width);
                    Rect rect = new Rect(leftRect)
                    {
                        y = yAcc,
                        height = height,
                    };
                    saintsRenderer.RenderPosition(rect);
                    yAcc += height;
                }
            }

            // Debug.Log($"Done {property.propertyPath}");
        }

        #endregion

        #region UI Toolkit
#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            object value;
            if (property.propertyType == SerializedPropertyType.ManagedReference)
            {
                value = property.managedReferenceValue;
                if (value == null)
                {
                    return new VisualElement();
                }
            }
            else
            {
                object parentValue = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
                (string error, int _, object getValue) = Util.GetValue(property, fieldInfo, parentValue);
                if (error != "")
                {
                    return new HelpBox(error, HelpBoxMessageType.Error);
                }

                value = getValue;
            }
            Dictionary<string, SerializedProperty> serializedFieldNames = GetSerializableFieldInfo(property)
                .ToDictionary(each => each.name, each => each.property);

            SaintsRowAttribute saintsRowAttribute = (SaintsRowAttribute) attribute;

            IReadOnlyList<ISaintsRenderer> renderer = SaintsEditor.GetRenderers(serializedFieldNames, property.serializedObject, value);

            // VisualElement bodyElement = SaintsEditor.CreateVisualElement(renderer);
            VisualElement bodyElement = new VisualElement();
            foreach (ISaintsRenderer saintsRenderer in renderer)
            {
                bodyElement.Add(saintsRenderer.CreateVisualElement());
            }

#if DOTWEEN && !SAINTSFIELD_DOTWEEN_DISABLED
            bodyElement.RegisterCallback<AttachToPanelEvent>(_ => SaintsEditor.AddInstance(this));
            bodyElement.RegisterCallback<DetachFromPanelEvent>(_ => SaintsEditor.RemoveInstance(this));
#endif

            if (saintsRowAttribute.Inline)
            {
                return bodyElement;
            }

            bodyElement.style.paddingLeft = SaintsPropertyDrawer.IndentWidth;

            Foldout toggle = new Foldout
            {
                text = property.displayName,
                value = true,
            };
            toggle.RegisterValueChangedCallback(evt => bodyElement.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None);

            VisualElement root = new VisualElement();
            root.Add(toggle);
            root.Add(bodyElement);
            return root;
        }
#endif
        #endregion
    }
}

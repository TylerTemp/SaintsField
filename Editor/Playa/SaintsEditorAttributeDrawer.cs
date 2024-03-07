using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;
#if UNITY_2021_3_OR_NEWER
using UnityEngine.UIElements;
#endif
// #if SAINTSFIELD_DOTWEEN
// using DG.DOTweenEditor;
// #endif

namespace SaintsField.Editor.Playa
{
    [CustomPropertyDrawer(typeof(SaintsEditorAttribute))]
    public class SaintsEditorAttributeDrawer: PropertyDrawer, IDOTweenPlayRecorder
    {
        public static (object parent, object current) GetTargets(FieldInfo fieldInfo, SerializedProperty property)
        {
            object parentValue = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
            object rawValue = fieldInfo.GetValue(parentValue);
            int arrayIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);

            object value = arrayIndex == -1 ? rawValue : SerializedUtils.GetValueAtIndex(rawValue, arrayIndex);
            return (parentValue, value);
        }

        private static IEnumerable<(string name, SerializedProperty property)> GetSerializableFieldInfo(SerializedProperty property)
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

        private IReadOnlyList<ISaintsRenderer> _imGuiRenderers;

        private IEnumerable<ISaintsRenderer> ImGuiEnsureRenderers(SerializedProperty property)
        {
            if (_imGuiRenderers != null)
            {
                return _imGuiRenderers;
            }
            (object _, object current) = GetTargets(fieldInfo, property);
            Dictionary<string, SerializedProperty> serializedFieldNames = GetSerializableFieldInfo(property).ToDictionary(each => each.name, each => each.property);
            return _imGuiRenderers = SaintsEditor.GetRenderers(false, serializedFieldNames, property.serializedObject, current);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SaintsEditorAttribute saintsEditorAttribute = (SaintsEditorAttribute) attribute;
            float baseLineHeight = saintsEditorAttribute.Inline ? 0 : SaintsPropertyDrawer.SingleLineHeight;
            float fieldHeight = 0f;
            if(property.isExpanded)
            {
                foreach (ISaintsRenderer saintsRenderer in ImGuiEnsureRenderers(property))
                {
                    fieldHeight += saintsRenderer.GetHeight();
                }
            }

            return baseLineHeight + fieldHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // SaintsEditorAttribute saintsEditorAttribute = (SaintsEditorAttribute) attribute;
            using (new EditorGUI.PropertyScope(position, label, property))
            {
                property.isExpanded = EditorGUI.Foldout(new Rect(position)
                {
                    height = SaintsPropertyDrawer.SingleLineHeight,
                }, property.isExpanded, label, true);

                if (!property.isExpanded)
                {
                    return;
                }

                Rect leftRect = new Rect(position)
                {
                    x = position.x + SaintsPropertyDrawer.IndentWidth,
                    y = position.y + SaintsPropertyDrawer.SingleLineHeight,
                    height = position.height - SaintsPropertyDrawer.SingleLineHeight,
                };

                float yAcc = leftRect.y;

                foreach (ISaintsRenderer saintsRenderer in ImGuiEnsureRenderers(property))
                {
                    float height = saintsRenderer.GetHeight();
                    Rect rect = new Rect(leftRect)
                    {
                        y = yAcc,
                        height = height,
                    };
                    saintsRenderer.RenderPosition(rect);
                    yAcc += height;
                }
            }
        }

        #endregion

#if UNITY_2022_2_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
        #region UI Toolkit
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            object parentValue = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
            object rawValue = fieldInfo.GetValue(parentValue);
            int arrayIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);

            object value = arrayIndex == -1 ? rawValue : SerializedUtils.GetValueAtIndex(rawValue, arrayIndex);

            Dictionary<string, SerializedProperty> serializedFieldNames = GetSerializableFieldInfo(property).ToDictionary(each => each.name, each => each.property);

            SaintsEditorAttribute saintsEditorAttribute = (SaintsEditorAttribute) attribute;

            IReadOnlyList<ISaintsRenderer> renderer = SaintsEditor.GetRenderers(true, serializedFieldNames, property.serializedObject, value);

            VisualElement bodyElement = SaintsEditor.CreateVisualElement(renderer);

#if SAINTSFIELD_DOTWEEN
            bodyElement.RegisterCallback<AttachToPanelEvent>(_ => SaintsEditor.AddInstance(this));
            bodyElement.RegisterCallback<DetachFromPanelEvent>(_ => SaintsEditor.RemoveInstance(this));
#endif

            if (saintsEditorAttribute.Inline)
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
        #endregion
#endif
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Drawers.ExpandableDrawer;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.AnimatorDrawers.AnimatorStateDrawer
{
    public partial class AnimatorStateAttributeDrawer
    {
        protected override bool UseCreateFieldIMGUI => true;

        protected override float DrawPreLabelImGui(Rect position, SerializedProperty property,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            if (property.propertyType == SerializedPropertyType.String)
            {
                return -1;
            }

            bool curExpanded = property.isExpanded;
            // ReSharper disable once ConvertToUsingDeclaration
            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                using (new GUIEnabledScoop(true))
                {
                    bool newExpanded = EditorGUI.Foldout(position, curExpanded,
                        new GUIContent(new string(' ', property.displayName.Length)), true);
                    if (changed.changed)
                    {
                        property.isExpanded = newExpanded;
                    }
                }
            }

            return 13;
        }

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            float width,
            ISaintsAttribute saintsAttribute, FieldInfo info, bool hasLabelWidth, object parent)
        {
            return EditorStyles.popup.CalcHeight(new GUIContent("M"), EditorGUIUtility.currentViewWidth);
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes, OnGUIPayload onGUIPayload,
            FieldInfo info, object parent)
        {
            MetaInfo metaInfo = GetMetaInfo(property, (saintsAttribute as AnimatorStateAttribute)?.AnimFieldName, info, parent);
            _errorMsg = metaInfo.Error;

            if (_errorMsg != "")
            {
                RenderErrorFallback(position, label, property);
                return;
            }

            GUIContent[] optionContents = metaInfo.AnimatorStates
                .Select(each => new GUIContent(FormatStateLabel(each, " > "))).ToArray();

            int curIndex = property.propertyType == SerializedPropertyType.String
                ? Util.ListIndexOfAction(metaInfo.AnimatorStates,
                    eachInfo => eachInfo.state.name == property.stringValue)
                : Util.ListIndexOfAction(metaInfo.AnimatorStates,
                    eachStateInfo => EqualAnimatorState(eachStateInfo, property));

            // Debug.Log($"curIndex={curIndex}");

            if (!_onEnableChecked) // check whether external source changed, to avoid caching an old value
            {
                _onEnableChecked = true;
                if (curIndex != -1 && property.propertyType != SerializedPropertyType.String)
                {
                    // if some attribute changed, we need to update them
                    // var curSelected = metaInfo.AnimatorStates[curIndex];
                    if (SetPropValue(property, metaInfo.AnimatorStates[curIndex]))
                    {
                        // Debug.Log($"IMGUI init changed");
                        // ReSharper disable once RedundantCast
                        onGUIPayload.SetValue(property.propertyType == SerializedPropertyType.String
                            ? (object)metaInfo.AnimatorStates[curIndex].state.name
                            : metaInfo.AnimatorStates[curIndex]);
                        if (ExpandableIMGUIScoop.IsInScoop)
                        {
                            property.serializedObject.ApplyModifiedProperties();
                        }
                    }
                }
            }

            // (Rect popupRect, Rect popupLeftRect) = RectUtils.SplitHeightRect(position, GetLabelFieldHeight(property, label, saintsAttribute));
            // ReSharper disable once ConvertToUsingDeclaration
            using (EditorGUI.ChangeCheckScope popupChanged = new EditorGUI.ChangeCheckScope())
            {
                int newIndex = EditorGUI.Popup(
                    position,
                    label,
                    curIndex,
                    optionContents.Concat(new[]
                    {
                        GUIContent.none,
                        new GUIContent($"Edit {metaInfo.RuntimeAnimatorController.name}..."),
                    }).ToArray(),
                    EditorStyles.popup);

                // ReSharper disable once InvertIf
                if (popupChanged.changed)
                {
                    if (newIndex >= optionContents.Length)
                    {
                        // Selection.activeObject = metaInfo.Animator.runtimeAnimatorController;
                        // EditorApplication.ExecuteMenuItem("Window/Animation/Animator");
                        OpenAnimator(metaInfo.RuntimeAnimatorController);
                    }
                    else
                    {
                        SetPropValue(property, metaInfo.AnimatorStates[newIndex]);
                        // ReSharper disable once RedundantCast
                        onGUIPayload.SetValue(property.propertyType == SerializedPropertyType.String
                            ? (object)metaInfo.AnimatorStates[newIndex].state.name
                            : metaInfo.AnimatorStates[newIndex]);
                        if (ExpandableIMGUIScoop.IsInScoop)
                        {
                            property.serializedObject.ApplyModifiedProperties();
                        }
                    }
                }
            }
            // RenderSubRow(popupLeftRect, property);
        }

        private static bool EqualAnimatorState(AnimatorStateChanged eachStateInfo, SerializedProperty property)
        {
            bool layerIndexEqual = FindPropertyRelative(property, "layerIndex")?.intValue == eachStateInfo.layerIndex;
            bool stateNameEqual = FindPropertyRelative(property, "stateName")?.stringValue == eachStateInfo.state.name;
            bool stateNameHashEqual =
                FindPropertyRelative(property, "stateNameHash")?.intValue == eachStateInfo.state.nameHash;

            if (!layerIndexEqual || !stateNameEqual || !stateNameHashEqual)
            {
                return false;
            }

            SerializedProperty subStateMachineNameChainProp =
                FindPropertyRelative(property, "subStateMachineNameChain");
            if (subStateMachineNameChainProp == null)
            {
                return true;
            }

            int arraySize = subStateMachineNameChainProp.arraySize;
            IReadOnlyList<string> eachChain = eachStateInfo.subStateMachineNameChain;
            if (arraySize != eachChain.Count)
            {
                return false;
            }

            for (int arrayIndex = 0; arrayIndex < arraySize; arrayIndex++)
            {
                string thisSubName = subStateMachineNameChainProp.GetArrayElementAtIndex(arrayIndex).stringValue;
                if (thisSubName != eachStateInfo.subStateMachineNameChain[arrayIndex])
                {
                    return false;
                }
            }

            return true;
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent)
        {
            return _errorMsg != "" || property.isExpanded;
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label,
            float width,
            IReadOnlyList<PropertyAttribute> allAttributes,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            float errorHeight = _errorMsg == "" ? 0 : ImGuiHelpBox.GetHeight(_errorMsg, width, MessageType.Error);

            if (!property.isExpanded)
            {
                return errorHeight;
            }

            int rowCount;
            if (property.propertyType == SerializedPropertyType.String)
            {
                rowCount = 0;
            }
            else
            {
                // must have: layerIndex; must one of: stateNameHash/stateName, + optionals
                rowCount = 1 + new[]
                {
                    "stateNameHash",
                    "stateName",
                    "stateSpeed",
                    "stateTag",
                    "animationClip",
                    "subStateMachineNameChain",
                }.Count(each => FindPropertyRelative(property, each) != null);
            }

            float subRowHeight = EditorGUIUtility.singleLineHeight * rowCount;
            return errorHeight + subRowHeight;
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property,
            GUIContent label, ISaintsAttribute saintsAttribute, int index1,
            IReadOnlyList<PropertyAttribute> allAttributes, OnGUIPayload onGuiPayload, FieldInfo info, object parent)
        {
            // Debug.Log(_targetIsString);
            if (property.propertyType == SerializedPropertyType.String || !property.isExpanded)
            {
                return _errorMsg == "" ? position : ImGuiHelpBox.Draw(position, _errorMsg, MessageType.Error);
            }

            IReadOnlyList<SerializedProperty> renders = new[]
                {
                    "layerIndex",
                    "stateName",
                    "stateNameHash",
                    "stateSpeed",
                    "stateTag",
                    "animationClip",
                    // "subStateMachineNameChain",
                }
                .Select(each => FindPropertyRelative(property, each))
                .Where(each => each != null)
                .ToArray();

            SerializedProperty subStateMachineNameChainProp =
                FindPropertyRelative(property, "subStateMachineNameChain");

            // Rect leftRectForError = position;
            int willRenderCount = renders.Count + (subStateMachineNameChainProp == null ? 0 : 1);
            Rect willRenderRect = new Rect(position)
            {
                height = EditorGUIUtility.singleLineHeight * willRenderCount,
            };

            EditorGUI.DrawRect(willRenderRect, EColor.EditorEmphasized.GetColor());

            using (new EditorGUI.DisabledScope(true))
            {
                Rect indentedRect = new Rect(position)
                {
                    x = position.x + IndentWidth,
                    width = position.width - IndentWidth,
                };

                foreach ((SerializedProperty prop, int index) in renders.WithIndex())
                {
                    // bool isLast = subStateMachineNameChainProp == null && index == renders.Count - 1;
                    EditorGUI.PropertyField(new Rect(indentedRect)
                    {
                        y = indentedRect.y + EditorGUIUtility.singleLineHeight * index,
                        height = EditorGUIUtility.singleLineHeight,
                    }, prop, new GUIContent(ObjectNames.NicifyVariableName(prop.displayName)));
                    // useRect.y += EditorGUIUtility.singleLineHeight;
                }

                if (subStateMachineNameChainProp != null)
                {
                    string subStateStr = subStateMachineNameChainProp.arraySize == 0
                        ? ""
                        : string.Join(" > ", Enumerable
                            .Range(0, subStateMachineNameChainProp.arraySize)
                            .Select(each => subStateMachineNameChainProp.GetArrayElementAtIndex(each).stringValue)
                        );

                    Rect subStateRect = new Rect(indentedRect)
                    {
                        y = indentedRect.y + EditorGUIUtility.singleLineHeight * renders.Count,
                        height = EditorGUIUtility.singleLineHeight,
                    };
                    EditorGUI.TextField(subStateRect, ObjectNames.NicifyVariableName("subStateMachineNameChain"),
                        subStateStr);
                }
            }

            Rect leftRectForError = new Rect(position)
            {
                y = position.y + EditorGUIUtility.singleLineHeight *
                    (renders.Count + (subStateMachineNameChainProp == null ? 0 : 1)),
            };
            return _errorMsg == ""
                ? leftRectForError
                : ImGuiHelpBox.Draw(leftRectForError, _errorMsg, MessageType.Error);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.AnimatorStateDrawer
{
    public partial class AnimatorStateAttributeDrawer
    {
        private sealed class InfoIMGUI
        {
            public string Error = "";
            public RuntimeAnimatorController RuntimeAnimatorController;
            public IReadOnlyList<AnimatorStateChanged> AnimatorStates = Array.Empty<AnimatorStateChanged>();
            public int CurrentIndex = -1;
            public bool InitializedSelection;
        }

        private static readonly Dictionary<string, InfoIMGUI> InfoCacheIMGUI = new Dictionary<string, InfoIMGUI>();
        private readonly RichTextDrawer _richTextDrawer = new RichTextDrawer();

        private static InfoIMGUI EnsureKey(SerializedProperty property, AnimatorStateAttribute animatorStateAttribute,
            FieldInfo info, object parent)
        {
            string key = SerializedUtils.GetUniqueId(property);
            if (InfoCacheIMGUI.TryGetValue(key, out InfoIMGUI cache))
            {
                RefreshCache(cache, property, animatorStateAttribute?.AnimFieldName, info, parent);
                return cache;
            }

            cache = new InfoIMGUI();
            InfoCacheIMGUI[key] = cache;
            NoLongerInspectingWatch(property.serializedObject.targetObject, key, () => InfoCacheIMGUI.Remove(key));
            RefreshCache(cache, property, animatorStateAttribute?.AnimFieldName, info, parent);
            return cache;
        }

        private static void RefreshCache(InfoIMGUI cache, SerializedProperty property, string animFieldName,
            FieldInfo info, object parent)
        {
            if (property.propertyType is not (SerializedPropertyType.String or SerializedPropertyType.Generic))
            {
                cache.Error = $"Expect string/AnimatorState, get {property.propertyType}";
                cache.RuntimeAnimatorController = null;
                cache.AnimatorStates = Array.Empty<AnimatorStateChanged>();
                cache.CurrentIndex = -1;
                cache.InitializedSelection = false;
                return;
            }

            MetaInfo metaInfo = GetMetaInfo(property, animFieldName, info, parent);
            cache.Error = metaInfo.Error;
            cache.RuntimeAnimatorController = metaInfo.RuntimeAnimatorController;
            cache.AnimatorStates = metaInfo.AnimatorStates ?? Array.Empty<AnimatorStateChanged>();
            if (metaInfo.Error != "")
            {
                cache.CurrentIndex = -1;
            }
            else if (property.propertyType == SerializedPropertyType.String)
            {
                cache.CurrentIndex = Util.ListIndexOfAction(cache.AnimatorStates,
                    eachInfo => eachInfo.state.name == property.stringValue);
            }
            else
            {
                cache.CurrentIndex = Util.ListIndexOfAction(cache.AnimatorStates,
                    eachStateInfo => EqualAnimatorState(eachStateInfo, property));
            }
        }

        protected override bool UseCreateFieldIMGUI => true;

        protected override float DrawPreLabelImGui(Rect position, SerializedProperty property,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            if (property.propertyType != SerializedPropertyType.Generic)
            {
                return -1;
            }

            Rect foldoutRect = new Rect(position)
            {
                width = 12f,
            };

            bool newExpanded = GUI.Toggle(foldoutRect, property.isExpanded, GUIContent.none, EditorStyles.foldout);
            if (newExpanded != property.isExpanded)
            {
                property.isExpanded = newExpanded;
            }

            return 14f;
        }

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            float width,
            int index,
            ISaintsAttribute saintsAttribute, FieldInfo info, bool hasLabelWidth, object parent)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent)
        {
            AnimatorStateAttribute animatorStateAttribute = saintsAttribute as AnimatorStateAttribute;
            InfoIMGUI cache = EnsureKey(property, animatorStateAttribute, info, parent);

            if (cache.Error != "")
            {
                RenderErrorFallback(position, label, property);
                DrawOverrideRichText(position, label, overrideRichTextChunks);
                return;
            }

            if (!cache.InitializedSelection)
            {
                cache.InitializedSelection = true;
                if (cache.CurrentIndex != -1 && property.propertyType == SerializedPropertyType.Generic)
                {
                    if (SetPropValue(property, cache.AnimatorStates[cache.CurrentIndex]))
                    {
                        property.serializedObject.ApplyModifiedProperties();
                        TriggerChangedIMGUI(property, cache.AnimatorStates[cache.CurrentIndex]);
                        RefreshCache(cache, property, animatorStateAttribute?.AnimFieldName, info, parent);
                    }
                }
            }

            Rect lineRect = new Rect(position)
            {
                height = EditorGUIUtility.singleLineHeight,
            };

            Rect labelRect = new Rect();
            if (property.propertyType == SerializedPropertyType.Generic)
            {
                lineRect.xMin += 14f;
            }

            Rect fieldRect;
            if (label.text == "")
            {
                fieldRect = lineRect;
            }
            else
            {
                labelRect = new Rect(lineRect)
                {
                    width = Mathf.Min(EditorGUIUtility.labelWidth, lineRect.width),
                };
                fieldRect = new Rect(lineRect)
                {
                    xMin = labelRect.xMax,
                };
            }

            if (label.text != "")
            {
                if (property.propertyType == SerializedPropertyType.Generic &&
                    GUI.Button(labelRect, label, EditorStyles.label))
                {
                    property.isExpanded = !property.isExpanded;
                }
                else
                {
                    EditorGUI.LabelField(labelRect, label);
                }
                DrawOverrideRichText(labelRect, label, overrideRichTextChunks);
            }

            GUI.SetNextControlName(FieldControlName);
            if (GUI.Button(fieldRect, GUIContent.none, EditorStyles.popup))
            {
                PopupWindow.Show(fieldRect, new SaintsTreeDropdownIMGUI(
                    GetDropdownMetaInfo(cache),
                    Mathf.Max(fieldRect.width, 220f),
                    320f,
                    false,
                    (curItem, _) =>
                    {
                        AnimatorStateChanged newState = (AnimatorStateChanged)curItem;
                        if (newState == null)
                        {
                            AnimatorStateUtil.OpenAnimator(cache.RuntimeAnimatorController);
                            return null;
                        }

                        object changedValue = property.propertyType == SerializedPropertyType.String
                            ? (object)newState.state.name
                            : newState;

                        SetPropValue(property, newState);
                        property.serializedObject.ApplyModifiedProperties();
                        TriggerChangedIMGUI(property, changedValue);
                        RefreshCache(cache, property, animatorStateAttribute?.AnimFieldName, info, parent);
                        return null;
                    }));
            }

            Rect drawRect = new Rect(fieldRect)
            {
                xMin = fieldRect.xMin + 6f,
                xMax = fieldRect.xMax - 18f,
            };
            _richTextDrawer.DrawChunks(drawRect, GetDisplayChunks(property, cache));
        }

        private static AdvancedDropdownMetaInfo GetDropdownMetaInfo(InfoIMGUI cache)
        {
            AdvancedDropdownList<AnimatorStateChanged> dropdown =
                new AdvancedDropdownList<AnimatorStateChanged>();

            foreach (AnimatorStateChanged animatorState in cache.AnimatorStates)
            {
                dropdown.Add(GetTreeItemLabel(animatorState), animatorState);
            }

            if (cache.RuntimeAnimatorController != null)
            {
                if (cache.AnimatorStates.Count > 0)
                {
                    dropdown.AddSeparator();
                }

                dropdown.Add($"Edit {cache.RuntimeAnimatorController.name}...", null);
            }

            dropdown.SelfCompact();

            return new AdvancedDropdownMetaInfo
            {
                CurDisplay = cache.CurrentIndex >= 0 ? FormatStateLabel(cache.AnimatorStates[cache.CurrentIndex], "/") : "-",
                CurValues = cache.CurrentIndex >= 0 ? new object[] { cache.AnimatorStates[cache.CurrentIndex] } : Array.Empty<object>(),
                DropdownListValue = dropdown,
                SelectStacks = Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>(),
            };
        }

        private static string GetTreeItemLabel(AnimatorStateChanged animatorStateInfo)
        {
            string preText = animatorStateInfo.subStateMachineNameChain.Count == 0
                ? ""
                : $"{string.Join('/', animatorStateInfo.subStateMachineNameChain)}/";
            string clipText;
            string iconText = "";
            if (animatorStateInfo.animationClip == null)
            {
                clipText = "";
            }
            else
            {
                clipText = $" <color=gray>({animatorStateInfo.animationClip.name})</color>";
                iconText = "<icon=d_AnimationClip Icon/>";
            }

            return preText + iconText + animatorStateInfo.state.name + clipText + ": " + animatorStateInfo.layer.name;
        }

        private IEnumerable<RichTextDrawer.RichTextChunk> GetDisplayChunks(SerializedProperty property, InfoIMGUI cache)
        {
            if (cache.CurrentIndex >= 0)
            {
                AnimatorStateChanged animatorState = cache.AnimatorStates[cache.CurrentIndex];
                List<RichTextDrawer.RichTextChunk> chunks = new List<RichTextDrawer.RichTextChunk>();
                if (animatorState.animationClip != null)
                {
                    chunks.Add(new RichTextDrawer.RichTextChunk("<icon=d_AnimationClip Icon/>", true, "d_AnimationClip Icon"));
                }

                string content =
                    $"{animatorState.state.name}" +
                    $"<color=#{ColorUtility.ToHtmlStringRGB(EColor.Gray.GetColor())}>" +
                    (animatorState.animationClip == null ? "" : $" ({animatorState.animationClip.name})") +
                    ": " +
                    animatorState.layer.name +
                    (animatorState.subStateMachineNameChain.Count == 0 ? "" : $"/{string.Join('/', animatorState.subStateMachineNameChain)}") +
                    "</color>";
                chunks.Add(new RichTextDrawer.RichTextChunk(content, false, content));
                return chunks;
            }

            if (property.propertyType == SerializedPropertyType.String)
            {
                string wrongLabel = property.stringValue == ""
                    ? ""
                    : $"<color=red>?</color> {property.stringValue}";
                return RichTextDrawer.ParseRichXmlWithProvider(wrongLabel, new RichTextDrawer.EmptyRichTextTagProvider());
            }

            string stateName = FindPropertyRelative(property, "stateName")?.stringValue;
            string fallback = string.IsNullOrEmpty(stateName) ? "<color=red>?</color>" : $"<color=red>?</color> {stateName}";
            return RichTextDrawer.ParseRichXmlWithProvider(fallback, new RichTextDrawer.EmptyRichTextTagProvider());
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
            return EnsureKey(property, saintsAttribute as AnimatorStateAttribute ?? new AnimatorStateAttribute(), info, parent).Error != ""
                   || property.isExpanded;
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label,
            float width,
            IReadOnlyList<PropertyAttribute> allAttributes,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            string error = EnsureKey(property, saintsAttribute as AnimatorStateAttribute ?? new AnimatorStateAttribute(), info, parent).Error;
            float errorHeight = error == "" ? 0 : ImGuiHelpBox.GetHeight(error, width, MessageType.Error);

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
            IReadOnlyList<PropertyAttribute> allAttributes, FieldInfo info, object parent)
        {
            string error = EnsureKey(property, saintsAttribute as AnimatorStateAttribute ?? new AnimatorStateAttribute(), info, parent).Error;
            // Debug.Log(_targetIsString);
            if (property.propertyType == SerializedPropertyType.String || !property.isExpanded)
            {
                return error == "" ? position : ImGuiHelpBox.Draw(position, error, MessageType.Error);
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
            return error == ""
                ? leftRectForError
                : ImGuiHelpBox.Draw(leftRectForError, error, MessageType.Error);
        }
    }
}

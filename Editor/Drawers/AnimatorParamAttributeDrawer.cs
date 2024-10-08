using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
#if UNITY_2021_3_OR_NEWER
using UnityEngine.UIElements;
#endif
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(AnimatorParamAttribute))]
    public class AnimatorParamAttributeDrawer : SaintsPropertyDrawer
    {
        // private const string InvalidAnimatorControllerWarningMessage = "Target animator controller is null";
        private string _error = "";

        private struct MetaInfo
        {
            // ReSharper disable InconsistentNaming
            public string Error;
            public Animator Animator;
            public IReadOnlyList<AnimatorControllerParameter> AnimatorParameters;
            // ReSharper enable InconsistentNaming
        }

        private static MetaInfo GetMetaInfo(SerializedProperty property, ISaintsAttribute saintsAttribute, FieldInfo fieldInfo, object parent)
        {
            AnimatorParamAttribute animatorParamAttribute = (AnimatorParamAttribute)saintsAttribute;

            (string error, Animator animatorController) = AnimatorUtils.GetAnimator(animatorParamAttribute.AnimatorName, property, fieldInfo, parent);
            if (error != "")
            {
                return new MetaInfo
                {
                    Error = error,
                    AnimatorParameters = Array.Empty<AnimatorControllerParameter>(),
                };
            }

            List<AnimatorControllerParameter> animatorParameters = new List<AnimatorControllerParameter>();

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (AnimatorControllerParameter parameter in animatorController.parameters)
            {
                if (animatorParamAttribute.AnimatorParamType == null ||
                    parameter.type == animatorParamAttribute.AnimatorParamType)
                {
                    animatorParameters.Add(parameter);
                }
            }

            return new MetaInfo
            {
                Error = "",
                Animator = animatorController,
                AnimatorParameters = animatorParameters,
            };
        }

        private static void OpenAnimator(Object animatorController)
        {
            Selection.activeObject = animatorController;
            EditorApplication.ExecuteMenuItem("Window/Animation/Animator");
        }

        #region IMGUI

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, FieldInfo info, bool hasLabelWidth, object parent)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            MetaInfo metaInfo = GetMetaInfo(property, saintsAttribute, info, parent);
            if (metaInfo.Error != "")
            {
                _error = metaInfo.Error;
                DefaultDrawer(position, property, label, info);
                return;
            }

            _error = "";

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    DrawPropertyForInt(position, property, label, metaInfo, onGUIPayload);
                    break;
                case SerializedPropertyType.String:
                    DrawPropertyForString(position, property, label, metaInfo, onGUIPayload);
                    break;
                default:
                    _error = $"Invalid property type: expect integer or string, get {property.propertyType}";
                    DefaultDrawer(position, property, label, info);
                    break;
            }
        }

        private static void DrawPropertyForInt(Rect position, SerializedProperty property, GUIContent label, MetaInfo metaInfo, OnGUIPayload onGUIPayload)
        {
            int paramNameHash = property.intValue;
            int index = -1;

            foreach ((AnimatorControllerParameter value, int eachIndex)  in metaInfo.AnimatorParameters.WithIndex())
            {
                if (value.nameHash == paramNameHash)
                {
                    index = eachIndex;
                    break;
                }
            }

            IEnumerable<string> displayOptions = GetDisplayOptions(metaInfo.AnimatorParameters);

            GUIContent[] contents = displayOptions
                .Select(each => new GUIContent(each))
                .Concat(new[]
                {
                    GUIContent.none,
                    new GUIContent($"Edit {metaInfo.Animator.runtimeAnimatorController.name}...")
                })
                .ToArray();

            // ReSharper disable once ConvertToUsingDeclaration
            using(EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                int newIndex = EditorGUI.Popup(position, label, index, contents);
                // ReSharper disable once InvertIf
                if(changed.changed)
                {
                    if(newIndex < metaInfo.AnimatorParameters.Count)
                    {
                        int newValue = metaInfo.AnimatorParameters[newIndex].nameHash;
                        property.intValue = newValue;
                        onGUIPayload.SetValue(newValue);
                        if(ExpandableIMGUIScoop.IsInScoop)
                        {
                            property.serializedObject.ApplyModifiedProperties();
                        }
                    }
                    else
                    {
                        OpenAnimator(metaInfo.Animator.runtimeAnimatorController);
                    }
                }
            }
        }

        private static void DrawPropertyForString(Rect position, SerializedProperty property, GUIContent label, MetaInfo metaInfo, OnGUIPayload onGUIPayload)
        {
            string paramName = property.stringValue;
            int index = metaInfo.AnimatorParameters
                .Select((value, valueIndex) => new {value, index=valueIndex})
                .FirstOrDefault(each => each.value.name == paramName)?.index ?? -1;

            IEnumerable<string> displayOptions = GetDisplayOptions(metaInfo.AnimatorParameters);

            // ReSharper disable once ConvertToUsingDeclaration
            using(EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                GUIContent[] contents = displayOptions
                    .Select(each => new GUIContent(each))
                    .Concat(new[]
                    {
                        GUIContent.none,
                        new GUIContent($"Edit {metaInfo.Animator.runtimeAnimatorController.name}..."),
                    })
                    .ToArray();

                int newIndex = EditorGUI.Popup(position, label, index, contents);
                // ReSharper disable once InvertIf
                if(changed.changed)
                {
                    if(newIndex < metaInfo.AnimatorParameters.Count)
                    {
                        string newValue = metaInfo.AnimatorParameters[newIndex].name;
                        property.stringValue = newValue;
                        onGUIPayload.SetValue(newValue);
                        if(ExpandableIMGUIScoop.IsInScoop)
                        {
                            property.serializedObject.ApplyModifiedProperties();
                        }
                    }
                    else
                    {
                        OpenAnimator(metaInfo.Animator.runtimeAnimatorController);
                    }
                }
            }
        }

        // ReSharper disable once ReturnTypeCanBeEnumerable.Local
        private static IReadOnlyList<string> GetDisplayOptions(IEnumerable<AnimatorControllerParameter> animatorParams) => animatorParams.Select(each => $"{each.name} [{each.type}]").ToList();

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent) => _error == "" ? 0 : ImGuiHelpBox.GetHeight(_error, EditorGUIUtility.currentViewWidth, MessageType.Error);

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent) => ImGuiHelpBox.Draw(position, _error, MessageType.Error);

        #endregion

#if UNITY_2021_3_OR_NEWER

        #region UI ToolKit

        // private MetaInfo _curMetaInfo = new MetaInfo
        // {
        //     Error = "",
        //     AnimatorParameters = new List<AnimatorControllerParameter>(),
        // };

        private static string NameDropdownField(SerializedProperty property) => $"{property.propertyPath}__AnimatorParam_DropdownField";
        private static string NameHelpBox(SerializedProperty property) => $"{property.propertyPath}__AnimatorParam_HelpBox";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, VisualElement container, FieldInfo info, object parent)
        {
            MetaInfo metaInfo = GetMetaInfo(property, saintsAttribute, info, parent);

            UIToolkitUtils.DropdownButtonField dropdownButton = UIToolkitUtils.MakeDropdownButtonUIToolkit(property.displayName);
            dropdownButton.name = NameDropdownField(property);
            dropdownButton.userData = metaInfo;

            dropdownButton.AddToClassList(ClassAllowDisable);
            return dropdownButton;
//             DropdownField dropdownField = new DropdownField(property.displayName)
//             {
//                 style =
//                 {
//                     flexGrow = 1,
//                 },
//                 userData = curMetaInfo,
//                 name = NameDropdownField(property),
//                 choices = curMetaInfo.AnimatorParameters.Select(GetParameterLabel).ToList(),
//             };
//             dropdownField.AddToClassList("unity-base-field__aligned");
//
//             Func<AnimatorControllerParameter, bool> predicate = property.propertyType == SerializedPropertyType.String
//                 ? p => ParamNameEquals(p, property)
//                 : p => ParamHashEquals(p, property);
//             int curSelected = Util.ListIndexOfAction(curMetaInfo.AnimatorParameters, predicate);
//             // ReSharper disable once InvertIf
//             if (curSelected >= 0)
//             {
//                 AnimatorControllerParameter curTarget = curMetaInfo.AnimatorParameters[curSelected];
//                 dropdownField.SetValueWithoutNotify(GetParameterLabel(curTarget));
//                 // _dropdownField.value = GetParameterLabel(curTarget);
// #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_ANIMATOR_PARAM_DRAW_PROCESS
//                 Debug.Log($"AnimatorParam init {property.propertyPath} found {GetParameterLabel(curTarget)} among {string.Join(", ", dropdownField.choices)}: {dropdownField.index}/{dropdownField.value}");
// #endif
//             }
//             else
//             {
// #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_ANIMATOR_PARAM_DRAW_PROCESS
//                 Debug.Log($"AnimatorParam init {property.propertyPath} found nothing {curSelected} among {string.Join(", ", dropdownField.choices)}: {dropdownField.index}/{dropdownField.value}");
// #endif
//                 if(curMetaInfo.AnimatorParameters.Count > 0)
//                 {
//                     dropdownField.index = 0;
//                 }
//             }
//
//             dropdownField.AddToClassList(ClassAllowDisable);
//
//             return dropdownField;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
        {
            HelpBox helpBoxElement = new HelpBox("", HelpBoxMessageType.Error)
            {
                style =
                {
                    display = DisplayStyle.None,
                    flexGrow = 1,
                    flexShrink = 0,
                },
                name = NameHelpBox(property),
            };

            helpBoxElement.AddToClassList(ClassAllowDisable);
            return helpBoxElement;
        }

        private static bool ParamNameEquals(AnimatorControllerParameter param, SerializedProperty prop) => param.name == prop.stringValue;
        private static bool ParamHashEquals(AnimatorControllerParameter param, SerializedProperty prop) => param.nameHash == prop.intValue;

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            UIToolkitUtils.DropdownButtonField dropdownField = container.Q<UIToolkitUtils.DropdownButtonField>(NameDropdownField(property));
            dropdownField.ButtonElement.clicked += () => ShowDropdown(property, saintsAttribute, container, info, parent, onValueChangedCallback);
            // DropdownField dropdownField = container.Q<DropdownField>(NameDropdownField(property));
            // MetaInfo metaInfo = (MetaInfo)dropdownField.userData;
            // // ReSharper disable once InvertIf
            // if (metaInfo.Error != "")
            // {
            //     HelpBox helpBoxElement = container.Q<HelpBox>(NameHelpBox(property));
            //     helpBoxElement.style.display = metaInfo.Error == "" ? DisplayStyle.None : DisplayStyle.Flex;
            //     helpBoxElement.text = metaInfo.Error;
            // }
            //
            // dropdownField.RegisterValueChangedCallback(v =>
            // {
            //     MetaInfo nowMetaInfo = (MetaInfo)((VisualElement)v.target).userData;
            //     AnimatorControllerParameter selectedState = nowMetaInfo.AnimatorParameters[dropdownField.index];
            //     if(property.propertyType == SerializedPropertyType.String)
            //     {
            //         property.stringValue = selectedState.name;
            //     }
            //     else
            //     {
            //         property.intValue = selectedState.nameHash;
            //     }
            //     property.serializedObject.ApplyModifiedProperties();
            //     onValueChangedCallback.Invoke(selectedState);
            // });
        }

        private static void ShowDropdown(SerializedProperty property, ISaintsAttribute saintsAttribute, VisualElement container, FieldInfo info, object parent, Action<object> onValueChangedCallback)
        {
            MetaInfo metaInfo = GetMetaInfo(property, saintsAttribute, info, parent);

            UIToolkitUtils.DropdownButtonField dropdownField = container.Q<UIToolkitUtils.DropdownButtonField>(NameDropdownField(property));

            bool isString = property.propertyType == SerializedPropertyType.String;
            int selectedIndex = isString
                ? Util.ListIndexOfAction(metaInfo.AnimatorParameters, eachName => ParamNameEquals(eachName, property))
                : Util.ListIndexOfAction(metaInfo.AnimatorParameters, eachHash => ParamHashEquals(eachHash, property));

            GenericDropdownMenu genericDropdownMenu = new GenericDropdownMenu();
            foreach ((AnimatorControllerParameter value, int index) in metaInfo.AnimatorParameters.WithIndex())
            {
                AnimatorControllerParameter curItem = value;
                string curName = GetParameterLabel(curItem);

                genericDropdownMenu.AddItem(curName, index == selectedIndex, () =>
                {
                    if (isString)
                    {
                        property.stringValue = curItem.name;
                        property.serializedObject.ApplyModifiedProperties();
                        onValueChangedCallback(curItem.name);
                    }
                    else
                    {
                        property.intValue = curItem.nameHash;
                        property.serializedObject.ApplyModifiedProperties();
                        onValueChangedCallback(curItem.nameHash);
                    }
                    dropdownField.ButtonLabelElement.text = curName;
                });
            }

            if (metaInfo.Animator != null)
            {
                if (metaInfo.AnimatorParameters.Count > 0)
                {
                    genericDropdownMenu.AddSeparator("");
                }
                genericDropdownMenu.AddItem($"Edit {metaInfo.Animator.runtimeAnimatorController.name}...", false, () => OpenAnimator(metaInfo.Animator.runtimeAnimatorController));
            }

            genericDropdownMenu.DropDown(dropdownField.ButtonElement.worldBound, dropdownField.ButtonElement, true);
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info)
        {
            object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
            MetaInfo metaInfo = GetMetaInfo(property, saintsAttribute, info, parent);

            UIToolkitUtils.DropdownButtonField dropdownField = container.Q<UIToolkitUtils.DropdownButtonField>(NameDropdownField(property));
            HelpBox helpBoxElement = container.Q<HelpBox>(NameHelpBox(property));
            if (helpBoxElement.text != metaInfo.Error)
            {
                helpBoxElement.text = metaInfo.Error;
                helpBoxElement.style.display = metaInfo.Error == "" ? DisplayStyle.None : DisplayStyle.Flex;
            }

            string label;
            if (property.propertyType == SerializedPropertyType.String)
            {
                if (string.IsNullOrEmpty(property.stringValue))
                {
                    label = "-";
                }
                else
                {
                    label = $"{property.stringValue} [?]";
                    foreach (AnimatorControllerParameter animatorControllerParameter in metaInfo.AnimatorParameters)
                    {
                        // ReSharper disable once InvertIf
                        if (ParamNameEquals(animatorControllerParameter, property))
                        {
                            label = GetParameterLabel(animatorControllerParameter);
                            break;
                        }
                    }
                }
            }
            else
            {
                if (property.intValue == 0)
                {
                    label = "-";
                }
                else
                {
                    label = $"{property.intValue} [?]";
                    foreach (AnimatorControllerParameter animatorControllerParameter in metaInfo.AnimatorParameters)
                    {
                        // ReSharper disable once InvertIf
                        if (ParamHashEquals(animatorControllerParameter, property))
                        {
                            label = GetParameterLabel(animatorControllerParameter);
                            break;
                        }
                    }
                }
            }

            if (dropdownField.ButtonLabelElement.text != label)
            {
                dropdownField.ButtonLabelElement.text = label;
            }
        }

        protected override void ChangeFieldLabelToUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, string labelOrNull,
            IReadOnlyList<RichTextDrawer.RichTextChunk> richTextChunks, bool tried, RichTextDrawer richTextDrawer)
        {
            UIToolkitUtils.DropdownButtonField dropdownField = container.Q<UIToolkitUtils.DropdownButtonField>(NameDropdownField(property));
            UIToolkitUtils.SetLabel(dropdownField.ButtonLabelElement, richTextChunks, richTextDrawer);
        }

        private static string GetParameterLabel(AnimatorControllerParameter each) => $"{each.name} [{each.type}]";

        #endregion

#endif
    }
}

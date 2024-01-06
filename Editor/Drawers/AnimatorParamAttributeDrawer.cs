using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
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
            public string Error;
            public IReadOnlyList<AnimatorControllerParameter> AnimatorParameters;
        }

        private static MetaInfo GetMetaInfo(SerializedProperty property, ISaintsAttribute saintsAttribute)
        {
            AnimatorParamAttribute animatorParamAttribute = (AnimatorParamAttribute)saintsAttribute;

            Animator animatorController;
            if (animatorParamAttribute.AnimatorName == null)
            {
                Object targetObj = property.serializedObject.targetObject;
                // animatorController = (Animator)animProp.objectReferenceValue;
                switch (targetObj)
                {
                    case GameObject go:
                        animatorController = go.GetComponent<Animator>();
                        break;
                    case Component component:
                        animatorController = component.GetComponent<Animator>();
                        break;
                    default:
                        string error = $"Animator controller not found in {targetObj}. Try specific a name instead.";
                        return new MetaInfo
                        {
                            Error = error,
                            AnimatorParameters = Array.Empty<AnimatorControllerParameter>(),
                        };
                }
            }
            else
            {

                SerializedObject targetSer = property.serializedObject;
                SerializedProperty animProp = targetSer.FindProperty(animatorParamAttribute.AnimatorName) ??
                                              SerializedUtils.FindPropertyByAutoPropertyName(targetSer,
                                                  animatorParamAttribute.AnimatorName);

                bool invalidAnimatorController = animProp == null;

                if (invalidAnimatorController)
                {
                    string error = $"Animator controller `{animatorParamAttribute.AnimatorName}` is null";
                    return new MetaInfo
                    {
                        Error = error,
                        AnimatorParameters = Array.Empty<AnimatorControllerParameter>(),
                    };
                }

                animatorController = (Animator)animProp.objectReferenceValue;
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
                AnimatorParameters = animatorParameters,
            };
        }

        #region IMGUI

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, bool hasLabelWidth)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, object parent)
        {
            MetaInfo metaInfo = GetMetaInfo(property, saintsAttribute);
            if (metaInfo.Error != "")
            {
                _error = metaInfo.Error;
                DefaultDrawer(position, property, label);
                return;
            }

            _error = "";

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    DrawPropertyForInt(position, property, label, metaInfo.AnimatorParameters);
                    break;
                case SerializedPropertyType.String:
                    DrawPropertyForString(position, property, label, metaInfo.AnimatorParameters);
                    break;
                default:
                    _error = $"Invalid property type: expect integer or string, get {property.propertyType}";
                    DefaultDrawer(position, property, label);
                    break;
            }
        }

        private static void DrawPropertyForInt(Rect position, SerializedProperty property, GUIContent label, IReadOnlyList<AnimatorControllerParameter> animatorParameters)
        {
            int paramNameHash = property.intValue;
            int index = 0;

            for (int i = 0; i < animatorParameters.Count; i++)
            {
                // ReSharper disable once InvertIf
                if (paramNameHash == animatorParameters[i].nameHash)
                {
                    index = i + 1; // +1 because the first option is reserved for (None)
                    break;
                }
            }

            IEnumerable<string> displayOptions = GetDisplayOptions(animatorParameters);

            // ReSharper disable once ConvertToUsingDeclaration
            using(EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                int newIndex = EditorGUI.Popup(position, label, index, displayOptions.Select(each => new GUIContent(each)).ToArray());
                // ReSharper disable once InvertIf
                if(changed.changed)
                {
                    int newValue = animatorParameters[newIndex].nameHash;

                    if (property.intValue != newValue)
                    {
                        property.intValue = newValue;
                    }
                }
            }
        }

        private static void DrawPropertyForString(Rect position, SerializedProperty property, GUIContent label, IReadOnlyList<AnimatorControllerParameter> animatorParameters)
        {
            string paramName = property.stringValue;
            int index = animatorParameters
                .Select((value, valueIndex) => new {value, index=valueIndex})
                .FirstOrDefault(each => each.value.name == paramName)?.index ?? -1;
            // int index = 0;
            //
            // for (int i = 0; i < animatorParameters.Count; i++)
            // {
            //     // ReSharper disable once InvertIf
            //     if (paramName.Equals(animatorParameters[i].name, System.StringComparison.Ordinal))
            //     {
            //         index = i + 1; // +1 because the first option is reserved for (None)
            //         break;
            //     }
            // }

            IEnumerable<string> displayOptions = GetDisplayOptions(animatorParameters);

            // ReSharper disable once ConvertToUsingDeclaration
            using(EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                int newIndex = EditorGUI.Popup(position, label, index, displayOptions.Select(each => new GUIContent(each)).ToArray());
                // ReSharper disable once InvertIf
                if(changed.changed)
                {
                    // string newValue = newIndex == 0 ? null : animatorParameters[newIndex - 1].name;
                    string newValue = animatorParameters[newIndex].name;

                    if (!property.stringValue.Equals(newValue, StringComparison.Ordinal))
                    {
                        property.stringValue = newValue;
                    }
                }
            }
        }

        // ReSharper disable once ReturnTypeCanBeEnumerable.Local
        private static IReadOnlyList<string> GetDisplayOptions(IEnumerable<AnimatorControllerParameter> animatorParams) => animatorParams.Select(each => $"{each.name} [{each.type}]").ToList();

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute) => _error == "" ? 0 : ImGuiHelpBox.GetHeight(_error, EditorGUIUtility.currentViewWidth, MessageType.Error);

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => ImGuiHelpBox.Draw(position, _error, MessageType.Error);

        #endregion

        #region UI ToolKit

        // private MetaInfo _curMetaInfo = new MetaInfo
        // {
        //     Error = "",
        //     AnimatorParameters = new List<AnimatorControllerParameter>(),
        // };

        private static string ClassDropdownField(SerializedProperty property) => $"{property.propertyPath}:DropdownField";
        private static string ClassHelpBox(SerializedProperty property) => $"{property.propertyPath}:HelpBox";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property, ISaintsAttribute saintsAttribute, object parent,
            Action<object> onChange)
        {
            var curMetaInfo = GetMetaInfo(property, saintsAttribute);

            DropdownField dropdownField = new DropdownField(property.displayName)
            {
                style =
                {
                    flexGrow = 1,
                },
                userData = curMetaInfo,
            };

            dropdownField.AddToClassList(ClassDropdownField(property));

            dropdownField.RegisterValueChangedCallback(v =>
            {
                MetaInfo nowMetaInfo = (MetaInfo)((VisualElement)v.target).userData;
                AnimatorControllerParameter selectedState = nowMetaInfo.AnimatorParameters[dropdownField.index];
                if(property.propertyType == SerializedPropertyType.String)
                {
                    property.stringValue = selectedState.name;
                }
                else
                {
                    property.intValue = selectedState.nameHash;
                }
                property.serializedObject.ApplyModifiedProperties();
                onChange?.Invoke(selectedState);
            });

            dropdownField.choices = curMetaInfo.AnimatorParameters.Select(GetParameterLabel).ToList();

            Func<AnimatorControllerParameter, bool> predicate = property.propertyType == SerializedPropertyType.String
                ? p => ParamNameEquals(p, property)
                : p => ParamHashEquals(p, property);
            int curSelected = Util.ListIndexOfAction(curMetaInfo.AnimatorParameters, predicate);
            // ReSharper disable once InvertIf
            if (curSelected >= 0)
            {
                AnimatorControllerParameter curTarget = curMetaInfo.AnimatorParameters[curSelected];
                dropdownField.SetValueWithoutNotify(GetParameterLabel(curTarget));
                // _dropdownField.value = GetParameterLabel(curTarget);
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_ANIMATOR_PARAM_DRAW_PROCESS
                Debug.Log($"AnimatorParam init {property.propertyPath} found {GetParameterLabel(curTarget)} among {string.Join(", ", dropdownField.choices)}: {dropdownField.index}/{dropdownField.value}");
#endif
            }
            else
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_ANIMATOR_PARAM_DRAW_PROCESS
                Debug.Log($"AnimatorParam init {property.propertyPath} found nothing {curSelected} among {string.Join(", ", dropdownField.choices)}: {dropdownField.index}/{dropdownField.value}");
#endif
                if(curMetaInfo.AnimatorParameters.Count > 0)
                {
                    dropdownField.index = 0;
                }
            }


            return dropdownField;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute)
        {
            HelpBox helpBoxElement = new HelpBox("", HelpBoxMessageType.Error)
            {
                style =
                {
                    display = DisplayStyle.None,
                },
            };

            helpBoxElement.AddToClassList(ClassHelpBox(property));
            return helpBoxElement;
        }

        private static bool ParamNameEquals(AnimatorControllerParameter param, SerializedProperty prop) => param.name == prop.stringValue;
        private static bool ParamHashEquals(AnimatorControllerParameter param, SerializedProperty prop) => param.nameHash == prop.intValue;

        protected override void OnAwakeUiToolKit(SerializedProperty property, ISaintsAttribute saintsAttribute, VisualElement containerElement)
        {
            DropdownField dropdownField = containerElement.Query<DropdownField>(className: ClassDropdownField(property)).First();
            MetaInfo metaInfo = (MetaInfo)dropdownField.userData;
            // ReSharper disable once InvertIf
            if (metaInfo.Error != "")
            {
                HelpBox helpBoxElement = containerElement.Query<HelpBox>(className: ClassHelpBox(property)).First();
                helpBoxElement.style.display = metaInfo.Error == "" ? DisplayStyle.None : DisplayStyle.Flex;
                helpBoxElement.text = metaInfo.Error;
            }
        }

        protected override void OnUpdateUiToolKit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            VisualElement containerElement)
        {
            MetaInfo metaInfo = GetMetaInfo(property, saintsAttribute);

            DropdownField dropdownField = containerElement.Query<DropdownField>(className: ClassDropdownField(property)).First();

            MetaInfo curMetaInfo = (MetaInfo) dropdownField.userData;
            dropdownField.userData = metaInfo;

            bool errorEqual = metaInfo.Error == curMetaInfo.Error;
            bool seqEqual = metaInfo.AnimatorParameters.SequenceEqual(curMetaInfo.AnimatorParameters);

            if(!errorEqual)
            {
                HelpBox helpBoxElement = containerElement.Query<HelpBox>(className: ClassHelpBox(property)).First();
                helpBoxElement.style.display = metaInfo.Error == "" ? DisplayStyle.None : DisplayStyle.Flex;
                helpBoxElement.text = metaInfo.Error;
            }

            // ReSharper disable once InvertIf
            if (!seqEqual)
            {
                dropdownField.choices = metaInfo.AnimatorParameters.Select(GetParameterLabel).ToList();
                // Debug.Log($"Update get {metaInfo.AnimatorParameters.Count} parameters: {string.Join(", ", metaInfo.AnimatorParameters.Select(GetParameterLabel))}");
                if(metaInfo.AnimatorParameters.Count > 0 && dropdownField.index < 0)
                {
                    // Debug.Log($"Set {property.propertyPath} to 0");
                    dropdownField.index = 0;
                }
            }
        }

        private static string GetParameterLabel(AnimatorControllerParameter each) => $"{each.name} [{each.type}]";

        #endregion
    }
}

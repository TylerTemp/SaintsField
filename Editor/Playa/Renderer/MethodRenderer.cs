using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;
#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Playa.Renderer
{
    public class MethodRenderer: AbsRenderer
    {
        public MethodRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo, bool tryFixUIToolkit=false) : base(fieldWithInfo)
        {
            Debug.Assert(FieldWithInfo.MethodInfo.GetParameters().All(p => p.IsOptional), $"{FieldWithInfo.MethodInfo.Name} has non-optional parameters");
        }

#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
        public override VisualElement CreateVisualElement()
        {
            object target = FieldWithInfo.Target;
            MethodInfo methodInfo = FieldWithInfo.MethodInfo;
            Debug.Assert(methodInfo.GetParameters().All(p => p.IsOptional));

            ButtonAttribute buttonAttribute = null;
            List<IPlayaMethodBindAttribute> methodBindAttributes = new List<IPlayaMethodBindAttribute>();

            foreach (IPlayaAttribute playaAttribute in FieldWithInfo.PlayaAttributes)
            {
                if(playaAttribute is ButtonAttribute button)
                {
                    buttonAttribute = button;
                }
                else if(playaAttribute is IPlayaMethodBindAttribute methodBindAttribute)
                {
                    methodBindAttributes.Add(methodBindAttribute);
                }
            }

            foreach (IPlayaMethodBindAttribute playaMethodBindAttribute in methodBindAttributes)
            {
                CheckMethodBind(playaMethodBindAttribute, FieldWithInfo);
            }

            if (buttonAttribute == null)
            {
                return null;
            }

            string buttonText = string.IsNullOrEmpty(buttonAttribute.Label) ? ObjectNames.NicifyVariableName(methodInfo.Name) : buttonAttribute.Label;
            object[] defaultParams = methodInfo.GetParameters().Select(p => p.DefaultValue).ToArray();

            Button result = new Button(() => methodInfo.Invoke(target, defaultParams))
            {
                text = buttonText,
                enableRichText = true,
                style =
                {
                    flexGrow = 1,
                },
            };
            if (FieldWithInfo.PlayaAttributes.Count(each => each is PlayaShowIfAttribute || each is PlayaHideIfAttribute || each is PlayaEnableIfAttribute ||
                                                            each is PlayaDisableIfAttribute) > 0)
            {
                result.RegisterCallback<AttachToPanelEvent>(_ => result.schedule.Execute(() => UIToolkitOnUpdate(FieldWithInfo, result, true)).Every(100));
            }

            return result;
        }

        private static void CheckMethodBind(IPlayaMethodBindAttribute playaMethodBindAttribute, SaintsFieldWithInfo fieldWithInfo)
        {
            MethodBind methodBind = playaMethodBindAttribute.MethodBind;
            string buttonTarget = playaMethodBindAttribute.ButtonTarget;
            object value = playaMethodBindAttribute.Value;

            UnityEngine.UI.Button uiButton = GetButton(buttonTarget, fieldWithInfo.Target);
            if (uiButton == null)
            {
                return;
            }

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (int eventIndex in Enumerable.Range(0, uiButton.onClick.GetPersistentEventCount()))
            {
                UnityEngine.Object persistentTarget = uiButton.onClick.GetPersistentTarget(eventIndex);
                string persistentMethodName = uiButton.onClick.GetPersistentMethodName(eventIndex);
                if (ReferenceEquals(persistentTarget, fieldWithInfo.Target) && persistentMethodName == fieldWithInfo.MethodInfo.Name)
                {
                    // _error = $"`{funcName}` already added to `{uiButton}`";
                    // already there
                    // Debug.Log($"`{funcName}` already added to `{uiButton}`");
                    return;
                }
            }
        }

        private static UnityEngine.UI.Button GetButton(string by, object target)
        {
            if (by == null)
            {
                return TryFindTarget(target);
            }

            List<Type> types = ReflectUtils.GetSelfAndBaseTypes(target);
            types.Reverse();

            foreach (Type type in types)
            {
                (ReflectUtils.GetPropType getPropType, object fieldOrMethodInfo) = ReflectUtils.GetProp(type, by);

                object genResult;
                switch (getPropType)
                {
                    case ReflectUtils.GetPropType.NotFound:
                        continue;

                    case ReflectUtils.GetPropType.Property:
                        genResult = ((PropertyInfo)fieldOrMethodInfo).GetValue(target);
                        break;
                    case ReflectUtils.GetPropType.Field:
                        genResult = ((FieldInfo)fieldOrMethodInfo).GetValue(target);
                        break;
                    case ReflectUtils.GetPropType.Method:
                    {
                        MethodInfo methodInfo = (MethodInfo)fieldOrMethodInfo;

                        try
                        {
                            genResult = methodInfo.Invoke(target, Array.Empty<object>());
                        }
                        catch (TargetInvocationException e)
                        {
                            Debug.LogException(e);
                            Debug.Assert(e.InnerException != null);
                            continue;
                        }
                        catch (Exception e)
                        {
                            // _error = e.Message;
                            Debug.LogException(e);
                            continue;
                        }

                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException(nameof(getPropType), getPropType, null);
                }

                // Debug.Log($"GetOf {genResult}/{genResult?.GetType()}/{genResult==null}");
                UnityEngine.UI.Button buttonResult = TryFindTarget(genResult);
                if (buttonResult != null)
                {
                    return buttonResult;
                }

            }

            return null;
        }

        private static UnityEngine.UI.Button TryFindTarget(object target)
        {
            switch (target)
            {
                case UnityEngine.UI.Button button:
                    return button;
                case GameObject gameObject:
                    return gameObject.GetComponent<UnityEngine.UI.Button>();
                case Component component:
                    return component.GetComponent<UnityEngine.UI.Button>();
                default:
                    return null;
            }
        }
#endif
        public override void OnDestroy()
        {
        }

        public override void Render()
        {
            object target = FieldWithInfo.Target;
            MethodInfo methodInfo = FieldWithInfo.MethodInfo;

            ButtonAttribute[] buttonAttributes = methodInfo.GetCustomAttributes<ButtonAttribute>(true).ToArray();
            if (buttonAttributes.Length == 0)
            {
                return;
            }

            ButtonAttribute buttonAttribute = buttonAttributes[0];

            PreCheckResult preCheckResult = GetPreCheckResult(FieldWithInfo);
            if (!preCheckResult.IsShown)
            {
                return;
            }

            using (new EditorGUI.DisabledScope(preCheckResult.IsDisabled))
            {
                string buttonText = string.IsNullOrEmpty(buttonAttribute.Label)
                    ? ObjectNames.NicifyVariableName(methodInfo.Name)
                    : buttonAttribute.Label;

                if (GUILayout.Button(buttonText, new GUIStyle(GUI.skin.button) { richText = true },
                        GUILayout.ExpandWidth(true)))
                {
                    object[] defaultParams = methodInfo.GetParameters().Select(p => p.DefaultValue).ToArray();
                    methodInfo.Invoke(target, defaultParams);
                }
            }
        }

        public override float GetHeight()
        {
            MethodInfo methodInfo = FieldWithInfo.MethodInfo;
            if(methodInfo.GetCustomAttribute<ButtonAttribute>(true) == null)
            {
                return 0;
            }

            PreCheckResult preCheckResult = GetPreCheckResult(FieldWithInfo);
            if (!preCheckResult.IsShown)
            {
                return 0;
            }

            return SaintsPropertyDrawer.SingleLineHeight;
        }

        public override void RenderPosition(Rect position)
        {
            object target = FieldWithInfo.Target;
            MethodInfo methodInfo = FieldWithInfo.MethodInfo;

            ButtonAttribute[] buttonAttributes = methodInfo.GetCustomAttributes<ButtonAttribute>(true).ToArray();
            if (buttonAttributes.Length == 0)
            {
                return;
            }

            PreCheckResult preCheckResult = GetPreCheckResult(FieldWithInfo);
            if (!preCheckResult.IsShown)
            {
                return;
            }

            using (new EditorGUI.DisabledScope(preCheckResult.IsDisabled))
            {

                ButtonAttribute buttonAttribute = buttonAttributes[0];

                string buttonText = string.IsNullOrEmpty(buttonAttribute.Label)
                    ? ObjectNames.NicifyVariableName(methodInfo.Name)
                    : buttonAttribute.Label;

                if (GUI.Button(position, buttonText, new GUIStyle(GUI.skin.button) { richText = true }))
                {
                    object[] defaultParams = methodInfo.GetParameters().Select(p => p.DefaultValue).ToArray();
                    methodInfo.Invoke(target, defaultParams);
                }
            }
        }
    }
}

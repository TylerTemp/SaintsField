#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.UIToolkitElements
{
    public class MethodParametersPanel: VisualElement, INotifyValueChanged<object[]>
    {
        private object[] _parameterValues;

        public MethodParametersPanel(MethodInfo methodInfo, bool inAnyHorizontalLayout, IReadOnlyList<object> targets, IRichTextTagProvider richTextTagProvider, string viewKey)
        {
            ParameterInfo[] parameters = methodInfo.GetParameters();
            _parameterValues = new object[parameters.Length];
            foreach ((ParameterInfo parameterInfo, int index) in parameters.WithIndex())
            {
                VisualElement paraContainer = new VisualElement();

                hierarchy.Add(paraContainer);

                Type paraType = parameterInfo.ParameterType;
                object paraValue;
                if(parameterInfo.HasDefaultValue)
                {
                    paraValue = parameterInfo.DefaultValue;
                }
                else
                {
                    paraValue = paraType.IsValueType ? Activator.CreateInstance(paraType) : null;
                }
                _parameterValues[index] = paraValue;

                Attribute[] attributes = parameterInfo.GetCustomAttributes().ToArray();
                bool paraValueChanged = true;
                paraContainer.schedule.Execute(() =>
                {
                    if (!paraValueChanged)
                    {
                        return;
                    }
                    // Debug.Log($"para value changed: {parameterInfo.Name}={value[index]}, {paraContainer.Children().FirstOrDefault()}");
                    VisualElement r = UIToolkitEdit.UIToolkitValueEdit(
                        paraContainer.Children().FirstOrDefault(),
                        ObjectNames.NicifyVariableName(parameterInfo.Name),
                        paraType,
                        value[index],
                        null,
                        newValue =>
                        {
                            _parameterValues[index] = newValue;
                            paraValueChanged = true;
                            value = _parameterValues.ToArray();
                            // Debug.Log($"param {index} set to {newValue}");
                        },
                        false,
                        inAnyHorizontalLayout,
                        attributes,
                        targets,
                        richTextTagProvider,
                        $"{viewKey}[{parameterInfo.Name}]"
                    ).result;
                    // ReSharper disable once InvertIf
                    if (r != null)
                    {
                        paraContainer.Clear();
                        paraContainer.Add(r);
                        paraContainer.schedule.Execute(() => UIToolkitUtils.CheckOutOfScoopFoldout(paraContainer, new HashSet<Toggle>()));
                    }

                    paraValueChanged = false;
                }).Every(100);
            }

            UIToolkitUtils.OnAttachToPanelOnce(this, _ => UIToolkitUtils.CheckOutOfScoopFoldout(this, new HashSet<Toggle>()));
        }


        public void SetValueWithoutNotify(object[] newValue)
        {
            _parameterValues = newValue;
        }

        public object[] value
        {
            get => _parameterValues;
            set
            {
                if (value == _parameterValues)
                {
                    return;
                }

                object[] previous = this.value;
                SetValueWithoutNotify(value);

                using ChangeEvent<object[]> evt = ChangeEvent<object[]>.GetPooled(previous, value);
                evt.target = this;
                SendEvent(evt);
            }
        }
    }
}
#endif

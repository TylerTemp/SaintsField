using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using SaintsField.Editor.Utils;
using SaintsField.Utils;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.Renderer.BaseRenderer.EditUnityEvent
{
#if UNITY_6000_0_OR_NEWER
    [UxmlElement]
#endif
    public partial class InvocationItemElement: VisualElement
    {
#if !UNITY_6000_0_OR_NEWER
        // public new class UxmlTraits : BindableElement.UxmlTraits { }
        public new class UxmlFactory : UxmlFactory<InvocationItemElement, UxmlTraits> { }
#endif

        public InvocationItemElement(): this(null)
        {
        }

        private readonly Label _functionName;
        // private readonly VisualElement _functionParams;
        private readonly ObjectField _unityObject;
        private readonly Label _parentName;
        private readonly Label _lambda;

        private Delegate _invocation;

        // ReSharper disable once MemberCanBePrivate.Global
        public InvocationItemElement(Delegate invocation)
        {
            VisualTreeAsset template =
                Util.LoadResource<VisualTreeAsset>("UIToolkit/UnityEvent/InvocationItem.uxml");
            template.CloneTree(this);

            _functionName = this.Q<Label>("functionName");
            // _functionParams = this.Q<VisualElement>("functionParams");

            _unityObject = this.Q<ObjectField>("unityObject");
            // _unityObject.objectType = typeof(UnityEngine.Object);
            _unityObject.SetEnabled(false);
            _parentName = this.Q<Label>("parentName");
            _lambda = this.Q<Label>("lambda");

            BindOrUpdateInvocation(invocation);
        }

        public void BindOrUpdateInvocation(Delegate invocation)
        {
            if (ReferenceEquals(_invocation, invocation))
            {
                return;
            }

            _invocation = invocation;

            MethodInfo method = invocation.Method;
            object target = invocation.Target;

            // uObject
            bool isUnityObject = target is UnityEngine.Object;
            _unityObject.SetValueWithoutNotify(target as UnityEngine.Object);
            UIToolkitUtils.SetDisplayStyle(_unityObject, isUnityObject ? DisplayStyle.Flex : DisplayStyle.None);

            // lambda
            bool isLambda = method.Name.Contains("<", StringComparison.Ordinal) &&
                            (method.IsDefined(typeof(CompilerGeneratedAttribute), false) ||
                             method.DeclaringType != null &&
                             method.DeclaringType.IsDefined(typeof(CompilerGeneratedAttribute), false));
            UIToolkitUtils.SetDisplayStyle(_lambda, isLambda ? DisplayStyle.Flex : DisplayStyle.None);

            // name
            UIToolkitUtils.SetDisplayStyle(_parentName, isUnityObject ? DisplayStyle.None : DisplayStyle.Flex);
            string parentText;
            string parentTooltip;
            if (RuntimeUtil.IsNull(target))
            {
                parentText = "<color=grey>null</color>";
                parentTooltip = "null";
            }
            else
            {
                parentText = $"{target}<color=grey>({target.GetType().FullName})</color>";
                parentTooltip = $"{target}\n({target.GetType().FullName})";
            }
            SetText(_parentName, parentText);
            if(_parentName.tooltip != parentTooltip)
            {
                _parentName.tooltip = parentTooltip;
            }

            SetText(_functionName, $"{method.Name}<color=grey>.{method.DeclaringType?.FullName}</color>");

            // ParameterInfo[] parameters = method.GetParameters();
            // // SetText(_functionParams, $"({parameterText})");
            // _functionParams.Clear();
            // foreach (ParameterInfo parameter in parameters)
            // {
            //     // _functionParams.Add(new Label($"{parameter.ParameterType.FullName} {parameter.Name}"));
            //     _functionParams.Add(new TypeLabel(parameter.ParameterType));
            // }
        }

        private static void SetText(Label label, string text)
        {
            if (label.text != text)
            {
                label.text = text;
            }
        }
    }
}

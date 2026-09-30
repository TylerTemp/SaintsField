using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Utils;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.Renderer.BaseRenderer.EditUnityEvent
{
#if UNITY_6000_0_OR_NEWER
    [UxmlElement]
#endif
    public partial class RuntimeCallElement: VisualElement
    {
#if !UNITY_6000_0_OR_NEWER
        // public new class UxmlTraits : BindableElement.UxmlTraits { }
        public new class UxmlFactory : UxmlFactory<RuntimeCallElement, UxmlTraits> { }
#endif

        public RuntimeCallElement() : this(null)
        {
        }

        private readonly VisualElement _body;
        private readonly VisualElement _container;

        private readonly Button _removeButton;

        private readonly HelpBox _helpBox;

        private Action<Delegate> _removeListener;
        private Delegate _listener;

        // ReSharper disable once MemberCanBePrivate.Global
        public RuntimeCallElement(object runtimeCall)
        {
            VisualTreeAsset template =
                Util.LoadResource<VisualTreeAsset>("UIToolkit/UnityEvent/RuntimeCall.uxml");
            template.CloneTree(this);

            _body = this.Q<VisualElement>("body");

            _container = _body.Q<VisualElement>("content");

            _removeButton = _body.Q<Button>("deleteButton");
            _removeButton.clicked += () => _removeListener.Invoke(_listener);

            _helpBox = this.Q<HelpBox>("helpBox");

            BindOrUpdateCall(runtimeCall);
        }

        public void BindOrUpdateCall(object runtimeCall, Action<Delegate> removeListener = null)
        {
            _removeListener = removeListener;
            _listener = GetListenerDelegate(runtimeCall);
            _removeButton.SetEnabled(_listener != null && _removeListener != null);

            if (_listener == null)
            {
                UIToolkitUtils.SetDisplayStyle(_body, DisplayStyle.None);
                UIToolkitUtils.SetHelpBox(_helpBox, "listener null");
                return;
            }

            UIToolkitUtils.SetHelpBox(_helpBox, "");
            UIToolkitUtils.SetDisplayStyle(_body, DisplayStyle.Flex);

            Delegate[] inv = _listener.GetInvocationList();
            InvocationItemElement[] invEle = GetOrCreateRows(inv.Length).ToArray();

            for (int index = 0; index < inv.Length; index++)
            {
                Delegate invocation = inv[index];
                InvocationItemElement element = invEle[index];

                element.BindOrUpdateInvocation(invocation);
            }
        }

        private readonly List<InvocationItemElement> _rows = new List<InvocationItemElement>();
        private IEnumerable<InvocationItemElement> GetOrCreateRows(int totalCount)
        {
            int leftCount = totalCount;
            Queue<InvocationItemElement> existed = new Queue<InvocationItemElement>(_rows);
            while (leftCount > 0 && existed.Count > 0)
            {
                InvocationItemElement cached = existed.Dequeue();
                yield return cached;
                leftCount--;
            }

            if (existed.Count > 0)
            {
                foreach (InvocationItemElement extra in existed)
                {
                    _rows.Remove(extra);
                    extra.RemoveFromHierarchy();
                }
            }
            else if (leftCount > 0)
            {
                // Debug.Log($"need create {leftCount} chips");
                for (int index = 0; index < leftCount; index++)
                {
                    // Debug.Log($"create {index} chip");
                    InvocationItemElement newCreated = new InvocationItemElement();
                    _rows.Add(newCreated);
                    _container.Add(newCreated);
                    yield return newCreated;
                }
            }
        }

        private static Delegate GetListenerDelegate(object call)
        {
            if (call == null)
            {
                return null;
            }

            for (Type type = call.GetType(); type != null; type = type.BaseType)
            {
                foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Public |
                                                           BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    if (typeof(Delegate).IsAssignableFrom(field.FieldType))
                    {
                        return field.GetValue(call) as Delegate;
                    }
                }
            }

            return null;
        }
    }
}

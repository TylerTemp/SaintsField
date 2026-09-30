using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.Renderer.BaseRenderer.EditUnityEvent
{
#if UNITY_6000_0_OR_NEWER
    [UxmlElement]
#endif
    public partial class UnityEventContainer: VisualElement
    {
#if !UNITY_6000_0_OR_NEWER
        // public new class UxmlTraits : UnityEventContainer.UxmlTraits { }
        public new class UxmlFactory : UxmlFactory<UnityEventContainer, UxmlTraits> { }
#endif

        private static readonly MethodInfo RemoveRuntimeListener = typeof(UnityEventBase).GetMethod(
            "RemoveListener", BindingFlags.Instance | BindingFlags.NonPublic, null,
            new[] { typeof(object), typeof(MethodInfo) }, null);

        // ReSharper disable once MemberCanBePrivate.Global
        public UnityEventContainer(): this("")
        {

        }

        // private readonly string _baseLabel;
        private readonly Label _paramLabel;
        // private readonly Label _label;

        private readonly Label _emptyLabel;
        private readonly VisualElement _paramsPanel;
        private readonly VisualElement _paramsContainer;

        private MethodInfo _invokeMethod;
        private UnityEventBase _invokeTarget;
        private object[] _parameterValues;

        public override VisualElement contentContainer { get; }

        private Texture2D _foldoutOnIcon;
        private Texture2D _foldoutOffIcon;

        private Texture2D FoldoutOnIcon => _foldoutOnIcon
            ? _foldoutOnIcon
            : _foldoutOnIcon = Util.LoadResource<Texture2D>("d_IN_foldout_on");
        private Texture2D FoldoutOffIcon => _foldoutOffIcon
            ? _foldoutOffIcon
            : _foldoutOffIcon = Util.LoadResource<Texture2D>("d_IN_foldout");

        private bool _expanded = true;

        public UnityEventContainer(string label)
        {
            VisualTreeAsset template =
                Util.LoadResource<VisualTreeAsset>("UIToolkit/UnityEvent/UnityEventContainer.uxml");
            template.CloneTree(this);

            Button foldoutButton = this.Q<Button>("foldoutButton");
            foldoutButton.Q<Label>("label").text = label;
            _paramLabel = foldoutButton.Q<Label>("params");
            Image foldoutImage = foldoutButton.Q<Image>();
            foldoutImage.image = FoldoutOnIcon;

            Button invokeButton = this.Q<Button>("invokeButton");
            _paramsPanel = this.Q<VisualElement>("paramsPanel");
            _paramsPanel.style.display = DisplayStyle.None;

            _paramsContainer = _paramsPanel.Q<VisualElement>("paramsContainer");

            Button pramsOkButton = _paramsPanel.Q<Button>("okButton");
            Button pramsCancelButton = _paramsPanel.Q<Button>("cancelButton");

            invokeButton.clicked += OnInvokeButtonClicked;
            pramsOkButton.clicked += OnParamsOkButtonClicked;
            pramsCancelButton.clicked += () => _paramsPanel.style.display = DisplayStyle.None;

            _emptyLabel = this.Q<Label>("emptyLabel");

            VisualElement body = this.Q<VisualElement>("body");
            contentContainer = body.Q<VisualElement>("items");


            foldoutButton.clicked += () =>
            {
                _expanded = !_expanded;
                foldoutImage.image = _expanded ? FoldoutOnIcon : FoldoutOffIcon;
                body.style.display = _expanded ? DisplayStyle.Flex : DisplayStyle.None;
            };

            UIToolkitUtils.OnAttachToPanelOnceWithEnsure(this, OnAttachOnPanel);
        }

        private void OnInvokeButtonClicked()
        {
            if (_paramsPanel.style.display == DisplayStyle.Flex)
            {
                _paramsPanel.style.display = DisplayStyle.None;
                return;
            }

            _paramsPanel.style.display = DisplayStyle.None;

            if (_unityEventBase == null)
            {
                return;
            }

            if (_unityEventBase is UnityEvent unityEvent)
            {
                InvokeUnityEvent(unityEvent.Invoke);
                return;
            }

            _invokeMethod = GetInvokeMethod(_unityEventBase.GetType());
            if (_invokeMethod == null)
            {
                Debug.LogError($"Unable to find Invoke method for {_unityEventBase.GetType()}.");
                return;
            }
            _invokeTarget = _unityEventBase;

            MethodParametersPanel parameterPanel = new MethodParametersPanel(_invokeMethod, false,
                new object[] { _invokeTarget }, null, $"{GetHashCode()}.Invoke");
            _parameterValues = parameterPanel.value;
            parameterPanel.RegisterValueChangedCallback(evt => _parameterValues = evt.newValue);

            _paramsContainer.Clear();
            _paramsContainer.Add(parameterPanel);
            _paramsPanel.style.display = DisplayStyle.Flex;
        }

        private void OnParamsOkButtonClicked()
        {
            if (_invokeTarget != null && _invokeMethod != null)
            {
                InvokeUnityEvent(() => _invokeMethod.Invoke(_invokeTarget, _parameterValues));
            }
            _paramsPanel.style.display = DisplayStyle.None;
        }

        private static MethodInfo GetInvokeMethod(Type eventType)
        {
            for (Type type = eventType;
                 type != null && typeof(UnityEventBase).IsAssignableFrom(type);
                 type = type.BaseType)
            {
                MethodInfo invokeMethod = type.GetMethods(
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                    .FirstOrDefault(method => method.Name == nameof(UnityEvent.Invoke));
                if (invokeMethod != null)
                {
                    return invokeMethod;
                }
            }

            return null;
        }

        private static void InvokeUnityEvent(Action invoke)
        {
            try
            {
                invoke();
            }
            catch (TargetInvocationException e)
            {
                Debug.LogException(e.InnerException ?? e);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void OnAttachOnPanel()
        {
            schedule.Execute(LoopCheckUpdate).Every(100);
        }

        private void LoopCheckUpdate()
        {
            CheckAndRefreshCalls();
        }

        private readonly List<RuntimeCallElement> _rows = new List<RuntimeCallElement>();

        private IEnumerable<RuntimeCallElement> GetOrCreateRows(int totalCount)
        {
            int leftCount = totalCount;
            Queue<RuntimeCallElement> existed = new Queue<RuntimeCallElement>(_rows);
            while (leftCount > 0 && existed.Count > 0)
            {
                RuntimeCallElement cached = existed.Dequeue();
                yield return cached;
                leftCount--;
            }

            if (existed.Count > 0)
            {
                foreach (RuntimeCallElement extra in existed)
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
                    RuntimeCallElement newCreated = new RuntimeCallElement();
                    _rows.Add(newCreated);
                    Add(newCreated);
                    yield return newCreated;
                }
            }
        }

        private UnityEventBase _unityEventBase;

        public void SetValueWithoutNotify(UnityEventBase unityEvent)
        {
            _unityEventBase = unityEvent;
            ParameterInfo[] parameters = unityEvent == null
                ? Array.Empty<ParameterInfo>()
                : GetInvokeMethod(unityEvent.GetType())?.GetParameters() ?? Array.Empty<ParameterInfo>();
            _paramLabel.text = parameters.Length == 0
                ? ""
                : $"({string.Join(", ", parameters.Select(each => ReflectUtils.StringifyType(each.ParameterType)))})";
            CheckAndRefreshCalls();
        }

        private void CheckAndRefreshCalls()
        {
            if (_unityEventBase == null)
            {
                return;
            }

            object calls = GetFieldValue(_unityEventBase, "m_Calls");
            IList runtimeCalls = calls == null
                ? null
                : GetFieldValue(calls, "m_RuntimeCalls") as IList;
            // _container.Add(new Label(runtimeCalls?.ToString()));
            if (runtimeCalls == null)
            {
                if (_emptyLabel.text != "RuntimeCalls is null")
                {
                    _emptyLabel.text = "RuntimeCalls is null";
                }
                UIToolkitUtils.SetDisplayStyle(_emptyLabel, DisplayStyle.Flex);
                CleanCurrentList();
                return;
            }

            if (runtimeCalls.Count == 0)
            {
                if (_emptyLabel.text != "No Runtime Call")
                {
                    _emptyLabel.text = "No Runtime Call";
                }
                UIToolkitUtils.SetDisplayStyle(_emptyLabel, DisplayStyle.Flex);
                CleanCurrentList();
                return;
            }

            UIToolkitUtils.SetDisplayStyle(_emptyLabel, DisplayStyle.None);

            RuntimeCallElement[] rowGen = GetOrCreateRows(runtimeCalls.Count).ToArray();
            for (int index = 0; index < runtimeCalls.Count; index++)
            {
                object runtimeCall = runtimeCalls[index];
                RuntimeCallElement callElement = rowGen[index];

                callElement.BindOrUpdateCall(runtimeCall, RemoveListenerAndRefresh);

                if (contentContainer.IndexOf(callElement) != index)
                {
                    Insert(index, callElement);
                }
            }
        }

        private void RemoveListenerAndRefresh(Delegate listener)
        {
            if (_unityEventBase == null || RemoveRuntimeListener == null)
            {
                return;
            }

            RemoveRuntimeListener.Invoke(_unityEventBase, new object[] { listener.Target, listener.Method });
            CheckAndRefreshCalls();
        }

        private void CleanCurrentList()
        {
            foreach (RuntimeCallElement unityEventRuntimeRow in _rows)
            {
                unityEventRuntimeRow.RemoveFromHierarchy();
            }
            _rows.Clear();
        }

        private static object GetFieldValue(object instance, string fieldName)
        {
            if (instance == null)
            {
                return null;
            }

            for (Type type = instance.GetType(); type != null; type = type.BaseType)
            {
                FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public |
                                                           BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                if (field != null)
                {
                    return field.GetValue(instance);
                }
            }

            return null;
        }


    }
}

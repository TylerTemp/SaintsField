#if UNITY_2021_3_OR_NEWER
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace SaintsField.Editor.UIToolkitElements
{
    public class BlueBar : VisualElement
    {
        private readonly SerializedObject _serializedObject;
        private readonly string _propertyPath;
        private readonly VisualElement _container;
        private bool _overrideStyled;
        private bool _refreshScheduled;
        private bool _callbacksRegistered;

        internal readonly UnityEvent PropertyChangedEvent = new UnityEvent();

        public BlueBar(SerializedProperty property, VisualElement container)
        {
            _serializedObject = property.serializedObject;
            _propertyPath = property.propertyPath;
            _container = container;

            pickingMode = PickingMode.Ignore;
            style.position = Position.Absolute;
            style.display = DisplayStyle.None;
            style.width = 2;
            style.backgroundColor = new Color(5/255f, 147/255f, 224/255f);
            style.top = 1;
            style.bottom = 1;
            style.left = -15;

            this.TrackPropertyValue(property, OnTrackedPropertyChanged);
            // RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            UIToolkitUtils.OnAttachToPanelOnceWithEnsure(this, RegisterPrefabCallbacks);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);

            _overrideStyled = OverrideStyle(property, _overrideStyled, _container, this);
        }

        private static bool IsTargetAffected(SerializedObject serializedObject, GameObject instance)
        {
            if (serializedObject == null || instance == null)
            {
                return false;
            }

            try
            {
                foreach (Object target in serializedObject.targetObjects)
                {
                    if (target == null)
                    {
                        continue;
                    }

                    GameObject targetGameObject = target switch
                    {
                        GameObject gameObject => gameObject,
                        Component component => component.gameObject,
                        _ => null,
                    };

                    if (targetGameObject == instance || (targetGameObject != null &&
                        PrefabUtility.GetOutermostPrefabInstanceRoot(targetGameObject) == instance))
                    {
                        return true;
                    }
                }
            }
            catch (System.NullReferenceException)
            {
                // The inspector can dispose its SerializedObject before the element detaches.
            }
            catch (System.ObjectDisposedException)
            {
                // The inspector can dispose its SerializedObject before the element detaches.
            }

            return false;
        }

        private static bool TryRefreshProperty(SerializedObject serializedObject, string propertyPath, out SerializedProperty property)
        {
            property = null;
            if (serializedObject == null)
            {
                return false;
            }

            try
            {
                if (serializedObject.targetObject == null)
                {
                    return false;
                }

                serializedObject.UpdateIfRequiredOrScript();
                property = serializedObject.FindProperty(propertyPath);
                return property != null;
            }
            catch (System.NullReferenceException)
            {
                // A queued refresh can outlive the inspector's SerializedObject.
            }
            catch (System.ObjectDisposedException)
            {
                // A queued refresh can outlive the inspector's SerializedObject.
            }

            return false;
        }

        private static bool OverrideStyle(SerializedProperty property, bool currentOverride, VisualElement container, VisualElement blueBar)
        {
            bool isOverride = property.prefabOverride;
            if (currentOverride == isOverride)
            {
                return isOverride;
            }

            if (isOverride)
            {
                container.AddToClassList(BindingExtensions.prefabOverrideUssClassName);
            }
            else
            {
                container.RemoveFromClassList(BindingExtensions.prefabOverrideUssClassName);
            }

            blueBar.style.display = isOverride ? DisplayStyle.Flex : DisplayStyle.None;
            return isOverride;
        }

        // private void OnAttachToPanel(AttachToPanelEvent evt) => RegisterPrefabCallbacks();

        private void RegisterPrefabCallbacks()
        {
            if (_callbacksRegistered)
            {
                return;
            }

#if UNITY_2023_1_OR_NEWER
            PrefabUtility.prefabInstanceApplied += OnPrefabInstanceChanged;
            PrefabUtility.prefabInstanceReverting += OnPrefabInstanceChanged;
#endif
            PrefabUtility.prefabInstanceUpdated += OnPrefabInstanceChanged;
            _callbacksRegistered = true;
        }

        private void OnTrackedPropertyChanged(SerializedProperty property) => RefreshOverrideStyle(property);

        private void RefreshOverrideStyle(SerializedProperty property)
        {
            if (!SerializedUtils.IsOk(property))
            {
                return;
            }

            _overrideStyled = OverrideStyle(property, _overrideStyled, _container, this);
            PropertyChangedEvent.Invoke();
        }

        private void OnPrefabInstanceChanged(GameObject instance)
        {
            if (!IsTargetAffected(_serializedObject, instance) || _refreshScheduled)
            {
                return;
            }

            _refreshScheduled = true;
            EditorApplication.delayCall += RefreshAfterPrefabChange;
        }

        private void RefreshAfterPrefabChange()
        {
            EditorApplication.delayCall -= RefreshAfterPrefabChange;
            _refreshScheduled = false;
            if (TryRefreshProperty(_serializedObject, _propertyPath, out SerializedProperty property))
            {
                RefreshOverrideStyle(property);
            }
        }

        private void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            if (!_callbacksRegistered)
            {
                return;
            }

#if UNITY_2023_1_OR_NEWER
            PrefabUtility.prefabInstanceApplied -= OnPrefabInstanceChanged;
            PrefabUtility.prefabInstanceReverting -= OnPrefabInstanceChanged;
#endif
            PrefabUtility.prefabInstanceUpdated -= OnPrefabInstanceChanged;
            EditorApplication.delayCall -= RefreshAfterPrefabChange;
            _refreshScheduled = false;
            _callbacksRegistered = false;
        }
    }

    public class EmptyPrefabOverrideElement: VisualElement
    {
        public EmptyPrefabOverrideElement(SerializedProperty property)
        {
            BlueBar blueBar = new BlueBar(property, this);
            Add(blueBar);
        }
    }

    public class EmptyPrefabOverrideField: BaseField<Object>
    {
        public EmptyPrefabOverrideField(string label, VisualElement visualInput, SerializedProperty property): base(label, visualInput)
        {
            style.overflow = Overflow.Visible;
            BlueBar blueBar = new BlueBar(property, this);
            Add(blueBar);
        }
    }

    public class FoldoutPrefabOverrideElement: Foldout
    {
        protected readonly UnityEvent PropertyChangedEvent = new UnityEvent();

        public FoldoutPrefabOverrideElement(SerializedProperty property)
        {
            BlueBar blueBar = new BlueBar(property, this)
            {
                style =
                {
                    height = SaintsPropertyDrawer.SingleLineHeight,
                },
            };
            blueBar.PropertyChangedEvent.AddListener(OnBlueBarPropertyChanged);
            hierarchy.Add(blueBar);
        }

        private void OnBlueBarPropertyChanged() => PropertyChangedEvent.Invoke();
    }
}
#endif

#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System;
using System.Linq;
using SaintsField.Editor.UIToolkitElements.ToasterDrawer;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor
{
    public static class Toast
    {
        public struct ActionOptions
        {
            public string Label;
            public System.Action OnClick;
        }

        public struct Options
        {
            public string Description;
            public double? Duration;
            public string Icon;
            public Color? IconColor;
            public bool CloseButton;
            public ActionOptions? Action;
            public Action<ToasterElement> OnDismiss;
            public Action<ToasterElement> OnAutoClose;
        }

        public static ToasterElement Show(string message, Options? options = null, VisualElement element = null)
        {
            return Create(message, ToasterType.Default, options, element);
        }

        public static ToasterElement Success(string message, Options? options = null, VisualElement element = null)
        {
            return Create(message, ToasterType.Success, options, element);
        }

        public static ToasterElement Info(string message, Options? options = null, VisualElement element = null)
        {
            return Create(message, ToasterType.Info, options, element);
        }

        public static ToasterElement Warning(string message, Options? options = null, VisualElement element = null)
        {
            return Create(message, ToasterType.Warning, options, element);
        }

        public static ToasterElement Error(string message, Options? options = null, VisualElement element = null)
        {
            return Create(message, ToasterType.Error, options, element);
        }

        public static ToasterElement Loading(string message, Options? options = null, VisualElement element = null)
        {
            return Create(message, ToasterType.Loading, options, element);
        }

        public static void Dismiss(ToasterElement toasterElement = null, VisualElement element = null)
        {
            if (toasterElement != null)
            {
                toasterElement.Dismiss();
                return;
            }

            VisualElement root = FindRoot(element);
            ToasterContainer container = root?.Q<ToasterContainer>();
            if (container == null)
            {
                return;
            }

            foreach (ToasterElement each in container.Children().OfType<ToasterElement>().ToArray())
            {
                each.Dismiss();
            }
        }

        private static ToasterElement Create(string message, ToasterType type, Options? options, VisualElement element)
        {
            VisualElement root = FindRoot(element);
            if (root == null)
            {
                Debug.LogWarning("Toast could not find an active UI Toolkit root.");
                return null;
            }

            ToasterContainer container = root.Q<ToasterContainer>();
            if (container == null)
            {
                container = new ToasterContainer();
                root.Add(container);
            }
            else if (container.parent != root)
            {
                container.RemoveFromHierarchy();
                root.Add(container);
            }

            container.BringToFront();

            ToasterElement toasterElement = new ToasterElement().Show(message, type);
            ApplyOptions(toasterElement, options);
            return container.Enqueue(toasterElement);
        }

        private static void ApplyOptions(ToasterElement toasterElement, Options? options)
        {
            if (!options.HasValue)
            {
                return;
            }

            Options value = options.Value;
            toasterElement.SetDescription(value.Description);
            toasterElement.SetCustomIcon(value.Icon, value.IconColor);
            toasterElement.SetCloseButton(value.CloseButton);

            if (value.Duration.HasValue)
            {
                toasterElement.SetDuration(value.Duration.Value);
            }

            if (!value.Action.HasValue)
            {
                toasterElement.SetAction(null, null);
            }
            else
            {
                ActionOptions action = value.Action.Value;
                toasterElement.SetAction(action.Label, action.OnClick);
            }

            if (value.OnDismiss != null)
            {
                toasterElement.Dismissed.AddListener(each => value.OnDismiss(each));
            }

            if (value.OnAutoClose != null)
            {
                toasterElement.AutoClosed.AddListener(each => value.OnAutoClose(each));
            }
        }

        private static VisualElement FindRoot(VisualElement element)
        {
            if (element == null)
            {
                EditorWindow window = EditorWindow.focusedWindow ?? EditorWindow.mouseOverWindow;
                return window?.rootVisualElement;
            }

            foreach (EditorWindow window in Resources.FindObjectsOfTypeAll<EditorWindow>())
            {
                VisualElement root = window.rootVisualElement;
                if (root == element || root.Contains(element))
                {
                    return root;
                }
            }

            while (element.parent != null)
            {
                element = element.parent;
            }

            return element;
        }
    }
}
#endif

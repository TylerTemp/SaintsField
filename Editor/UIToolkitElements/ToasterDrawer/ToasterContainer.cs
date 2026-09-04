#if UNITY_2021_3_OR_NEWER
using System;
using SaintsField.Editor.Utils;
using UnityEngine.UIElements;

namespace SaintsField.Editor.UIToolkitElements.ToasterDrawer
{
#if UNITY_6000_0_OR_NEWER
    [UxmlElement]
#endif
    public partial class ToasterContainer : VisualElement
    {
#if !UNITY_6000_0_OR_NEWER
        public new class UxmlFactory : UxmlFactory<ToasterContainer, UxmlTraits> { }
#endif

        private const int VisibleGhostCount = 2;

        private const string ItemClass = "toaster-container__item";
        private const string LatestClass = "toaster-container__item--latest";
        private const string StackedLatestClass = "toaster-container__item--latest-stacked";
        private const string NearGhostClass = "toaster-container__item--ghost-near";
        private const string FarGhostClass = "toaster-container__item--ghost-far";
        private const string HiddenGhostClass = "toaster-container__item--ghost-hidden";

        private static StyleSheet _styleSheet;

        public long DefaultDurationMilliseconds = 4000;

        public ToasterContainer()
        {
            AddToClassList("toaster-container");
            _styleSheet ??= Util.LoadResource<StyleSheet>("UIToolkit/Toaster/toasterStyle.uss");
            styleSheets.Add(_styleSheet);
        }

        public ToasterElement Enqueue(ToasterElement toasterElement)
        {
            if (toasterElement == null)
            {
                throw new ArgumentNullException(nameof(toasterElement));
            }

            toasterElement.Dismissed.RemoveListener(OnCloseRequested);
            toasterElement.AutoClosed.RemoveListener(OnCloseRequested);
            toasterElement.Dismissed.AddListener(OnCloseRequested);
            toasterElement.AutoClosed.AddListener(OnCloseRequested);
            toasterElement.ApplyDefaultDuration(DefaultDurationMilliseconds);
            toasterElement.AddToClassList(ItemClass);

            if (toasterElement.parent != this)
            {
                Add(toasterElement);
            }

            UpdateStackStyles();

            return toasterElement;
        }

        private void OnCloseRequested(ToasterElement toasterElement)
        {
            toasterElement.Dismissed.RemoveListener(OnCloseRequested);
            toasterElement.AutoClosed.RemoveListener(OnCloseRequested);

            if (toasterElement.parent == this)
            {
                toasterElement.RemoveFromHierarchy();
                UpdateStackStyles();
            }
        }

        private void UpdateStackStyles()
        {
            int toasterCount = 0;
            for (int index = 0; index < childCount; index++)
            {
                if (hierarchy[index] is ToasterElement)
                {
                    toasterCount++;
                }
            }

            int depth = 0;
            for (int index = childCount - 1; index >= 0; index--)
            {
                if (hierarchy[index] is not ToasterElement toaster)
                {
                    continue;
                }

                toaster.RemoveFromClassList(LatestClass);
                toaster.RemoveFromClassList(StackedLatestClass);
                toaster.RemoveFromClassList(NearGhostClass);
                toaster.RemoveFromClassList(FarGhostClass);
                toaster.RemoveFromClassList(HiddenGhostClass);

                if (depth == 0)
                {
                    toaster.SetGhostStyle(false);
                    toaster.AddToClassList(LatestClass);
                    toaster.EnableInClassList(StackedLatestClass, toasterCount > 1);
                }
                else
                {
                    toaster.SetGhostStyle(true);
                    if (depth == 1)
                    {
                        toaster.AddToClassList(NearGhostClass);
                    }
                    else if (depth <= VisibleGhostCount)
                    {
                        toaster.AddToClassList(FarGhostClass);
                    }
                    else
                    {
                        toaster.AddToClassList(HiddenGhostClass);
                    }
                }

                depth++;
            }
        }
    }
}
#endif
